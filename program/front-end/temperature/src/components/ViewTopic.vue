/* eslint-disable vue/valid-v-for */
<template>
  <div id="view-topic-page">
    <img src="/static/images/newbanner.jpg" style="width:100%;height:352px;">
    <div id="vtp-main">
      <div id="left-aside">
        <div id="author-info">
          <div id="author-info-img-and-text">
            <img id="author-info-img" :src="thisTopicUser.avatr===undefined?'':loc+thisTopicUser.avatr" />
            <div id="author-info-text">
              <div id="author-info-name">{{thisTopicUser.nickName}}</div>
              <div id="author-info-signature">{{thisTopicUser.userAnnouncement}}</div>
            </div>
          </div>
          <div id="author-info-buttons">
            <div id="follow" v-show="!isMyTopic" v-on:click="follow">{{isFollowed?"已关注":"+关注"}}</div>
            <router-link id="personel-center" :to="{path:'/MyHome', query:{hostId:thisTopic,userId:myID}}">个人中心</router-link>
          </div>
        </div>
        <div id="latest-topic" class="left-aside-topic-card">
          <div class="left-aside-subtitle">最新话题</div>
          <div class="left-aside-list">
            <router-link v-for="(item,index) in LTopics" :key="index" :to="{path:'/ViewTopic', query:{topicID:item.TopicId,userID:myID}}" class="left-aside-list-item">
              {{item.TopicTitle}}
            </router-link>
          </div>
        </div>
        <div id="hotest-topic" class="left-aside-topic-card">
          <div class="left-aside-subtitle">最热话题</div>
          <div class="left-aside-list">
            <router-link :to="{path:'/ViewTopic', query:{topicID:item.TopicId,userID:myID}}" class="left-aside-list-item" v-for="(item,index) in HTopics" :key="index">
              {{item.TopicTitle}}
            </router-link>
          </div>
        </div>
      </div>
      <div id="topic-and-answer">
        <div id="this-topic">
          <div id="topic-title">{{topicDetail.topicTitle}}</div>
          <div id="topic-message">
            <div id="date-and-time">{{topicDetail.topicUploadTime}}</div>
            <div id="topic-zone">分区：{{topicDetail.zoneName}}</div>
          </div>
          <div id="topic-content">{{topicDetail.topicContent}}</div>
        </div>
        <div id="answer-count">{{Answers.length}} 个回答</div>
        <div class="answer-item" v-for="(item,index) in Answers" :key="index">
          <div class="answer-author">
            <img class="answer-author-avatar" :src="loc+item.firstLevelComment.UserAvatr"/>
            <div class="answer-author-name">{{item.firstLevelComment.UserNickName}}</div>
          </div>
          <div class="answer-content">{{item.firstLevelComment.Content}}</div>
          <div class="answer-message-time">发布于 {{item.firstLevelComment.UploadTime}}</div>
          <div class="answer-statistic">
            <div class="answer-statistic-comment">评论 {{item.userComments.length}}</div>
          </div>
          <div class="answer-comments">
            <div class="comment-item" v-for="(com,index) in item.userComments" :key="index">
              <img class="comment-author-avatar" :src="loc+com.UserAvatr">
              <div class="comment-author-name">{{com.UserNickName}}</div>
              <div class="comment-content">{{com.Content}}</div>
            </div>
            <div class="leave-comment">
              <input class="leave-comment-input" type="text" placeholder="发表评论" v-model="item.myComment"/>
              <div class="leave-comment-button" v-on:click="commentAnswer(item)">发表</div>
            </div>
          </div>
        </div>
        <div id="my-answer-card">
          <div id="my-answer-card-subtitle">我来回答</div>
          <textarea id="my-answer-content" v-model="myAnswerContent"></textarea>
          <div id="upload-answer" v-on:click="answerTopic">上传回答</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default{
  data () {
    return {
      loc: 'http://139.224.255.43:7779/',
      thisTopicUser: {},
      isFollowed: false,
      topicDetail: 0,
      Answers: [],

      LTNum: 8,
      LTopics: [],
      HTNum: 8,
      HTopics: [],
      answerUserList: [],

      myAnswerContent: '',

      isloading: false
    }
  },
  created: function () {
    this.buildData()
  },
  computed: {
    thisTopic: function () {
      if (this.$route.query.topicID === undefined) {
        this.$alert('this.$route.query.topicID', '缺少值', {confirmButtonText: '确定'})
      }
      return this.$route.query.topicID
    },
    myID: function () {
      if (this.$route.query.userID === undefined) {
        this.$alert('this.$route.query.userID', '缺少值', {confirmButtonText: '确定'})
      }
      return this.$route.query.userID
    },
    isMyTopic: function () {
      // console.log(this.thisTopicUser.userId===this.myID);
      return this.thisTopicUser.userId === this.myID
    }
  },
  watch: {
    thisTopic: function (a, b) {
      this.buildData()
    }
  },
  methods: {
    buildData () { // 获取整个页面的数据结构
      this.getSingleTopicDetail() // 根据topicID获取话题信息->获取用户信息、获取回答
      this.getLatestTopics() // 获取最新话题
      this.getHottestTopics() // 获取最热话题
    },
    getTokenFromCookie () { // 从当下cookie获取token
      // console.log(document.cookie);
      var cookie = document.cookie
      var cookieArr = cookie.split(';')
      for (var i = 0; i < cookieArr.length; i++) {
        var keyAndValue = cookieArr[i].split('=')
        if (keyAndValue[0].trim() === 'token') {
          // console.log(keyAndValue[1]);
          return keyAndValue[1]
        }
      }
    },
    getLatestTopics () { // 获取最新话题
      var takeTopicNum = this.LTNum
      this.ajax_getLatestTopics = new XMLHttpRequest()
      this.ajax_getLatestTopics.open('POST', 'http://139.224.255.43:7779/Topic/getNewestTopic?takeTopicNum=' + takeTopicNum, true)
      this.ajax_getLatestTopics.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_getLatestTopics.onreadystatechange = this.getLT
      this.ajax_getLatestTopics.send()
    },
    getLT () { // 最新话题附属函数
      if (this.ajax_getLatestTopics.readyState === 4 && this.ajax_getLatestTopics.status === 200) {
        var receive = JSON.parse(JSON.parse(this.ajax_getLatestTopics.responseText).topics)
        // console.log(receive);
        this.LTopics = receive
      }
    },
    getHottestTopics () { // 获取最热话题
      var takeTopicNum = this.HTNum
      this.ajax_getHottestTopics = new XMLHttpRequest()
      this.ajax_getHottestTopics.open('POST', 'http://139.224.255.43:7779/Topic/getHotestTopic?takeTopicNum=' + takeTopicNum, true)
      this.ajax_getHottestTopics.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_getHottestTopics.onreadystatechange = this.getHT
      this.ajax_getHottestTopics.send()
    },
    getHT () { // 最热话题附属函数
      if (this.ajax_getHottestTopics.readyState === 4 && this.ajax_getHottestTopics.status === 200) {
        var receive = JSON.parse(JSON.parse(this.ajax_getHottestTopics.responseText).topics)
        // console.log(receive);
        this.HTopics = receive
      }
    },
    getTopicDetailByID () { // 获取话题回答
      var topicID = this.thisTopic
      this.ajax_getTopicDetailByID = new XMLHttpRequest()
      this.ajax_getTopicDetailByID.open('POST', 'http://139.224.255.43:7779/Topic/getTopicDetailByID?topicID=' + topicID, true)
      this.ajax_getTopicDetailByID.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_getTopicDetailByID.onreadystatechange = this.getTD
      this.ajax_getTopicDetailByID.send()
    },
    getTD () { // 话题回答附属函数
      if (this.ajax_getTopicDetailByID.readyState === 4 && this.ajax_getTopicDetailByID.status === 200) {
        var receive = JSON.parse(this.ajax_getTopicDetailByID.responseText).answerUserList
        this.answerUserList = receive
        // console.log(JSON.parse(this.ajax_getTopicDetailByID.responseText));
        this.sortOutAnswerList()
      }
    },
    sortOutAnswerList () { // 整理话题回答
      var answerList = this.answerUserList
      // console.log(answerList);
      for (var i = 0; i < answerList.length; i++) {
        answerList[i] = JSON.parse(answerList[i]) // 将每个回答变成对象
        // console.log(answerList[i]);
        answerList[i].firstLevelComment = JSON.parse(answerList[i].firstLevelComment) // 将话题信息变成对象
        answerList[i].firstLevelComment.UploadTime = answerList[i].firstLevelComment.UploadTime.replace('T', '  ')// 去掉时间中的"T"
        for (var j = 0; j < answerList[i].userComments.length; j++) { // 将评论变成对象
          answerList[i].userComments[j] = JSON.parse(answerList[i].userComments[j])
          // answerList[i].userComments[j].userComment=JSON.parse(answerList[i].userComments[j].userComment);
          // answerList[i].userComments[j].userInfo=JSON.parse(answerList[i].userComments[j].userInfo);
        }
        answerList[i].myComment = ''
      }
      // console.log(answerList);
      this.Answers = answerList
    },
    getUserInfoByID () { // 获取用户信息
      var userId = this.topicDetail.userId
      // console.log(this.topicDetail.userId);
      this.ajax_getUserInfoByID = new XMLHttpRequest()
      this.ajax_getUserInfoByID.open('POST', 'http://139.224.255.43:7779/Account/getUserInfoByID?user_id=' + userId, true)
      this.ajax_getUserInfoByID.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_getUserInfoByID.onreadystatechange = this.getUIBID
      this.ajax_getUserInfoByID.send()
    },
    getUIBID () { // 用户信息附属函数
      if (this.ajax_getUserInfoByID.readyState === 4 && this.ajax_getUserInfoByID.status === 200) {
        var receive = JSON.parse(this.ajax_getUserInfoByID.responseText)
        receive.userInfo.userAnnouncement = receive.userAnnouncement
        this.thisTopicUser = receive.userInfo
        // console.log(receive.userAnnouncement);
        this.checkFollow()
      }
    },
    getSingleTopicDetail () { // 获取话题信息
      var topicID = this.thisTopic
      this.ajax_getSingleTopicDetail = new XMLHttpRequest()
      this.ajax_getSingleTopicDetail.open('POST', 'http://139.224.255.43:7779/Topic/getSingleTopicDetail?topicID=' + topicID, true)
      this.ajax_getSingleTopicDetail.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_getSingleTopicDetail.onreadystatechange = this.getSTD
      this.ajax_getSingleTopicDetail.send()
    },
    getSTD () { // 话题信息附属函数
      if (this.ajax_getSingleTopicDetail.readyState === 4 && this.ajax_getSingleTopicDetail.status === 200) {
        var receive = JSON.parse(this.ajax_getSingleTopicDetail.responseText)
        if (receive.flag === 1) {
          receive.topicDetail.topicUploadTime = receive.topicDetail.topicUploadTime.replace('T', '  ')
          this.topicDetail = receive.topicDetail
          this.getTopicDetailByID() // 根据topicID获取回答（数据包含部分话题信息，但不完整，所以不用）
          this.getUserInfoByID() // 获取用户信息
          // console.log(receive);
        } else {
          this.$alert('getSingleTopicDetail失败', '请求失败', {confirmButtonText: '确定'})
        }
      }
    },
    follow () { // 关注功能
      var idOfBlogger = this.thisTopicUser.userId// alert(1);
      var idOfFans = this.myID
      // console.log(this.thisTopicUser);
      if (!this.isFollowed) {
        this.ajax_follow = new XMLHttpRequest()
        this.ajax_follow.open('POST', 'http://139.224.255.43:7779/Account/createFollowByID?idOfBlogger=' + idOfBlogger + '&idOfFans=' + idOfFans, true)
        this.ajax_follow.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
        this.ajax_follow.onreadystatechange = this.fw
        this.ajax_follow.send()
      } else {
        this.ajax_dfollow = new XMLHttpRequest()
        this.ajax_dfollow.open('POST', 'http://139.224.255.43:7779/Account/deleteFollowByID?idOfBlogger=' + idOfBlogger + '&idOfFans=' + idOfFans, true)
        this.ajax_dfollow.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
        this.ajax_dfollow.onreadystatechange = this.dfw
        this.ajax_dfollow.send()
      }
    },
    fw () { // 关注功能附属函数
      if (this.ajax_follow.readyState === 4 && this.ajax_follow.status === 200) {
        var receive = JSON.parse(this.ajax_follow.responseText)
        // console.log(receive);
        if (receive.createFlag === 1) {
          this.isFollowed = !this.isFollowed
        }
      }
    },
    dfw () { // 关注功能附属函数
      if (this.ajax_dfollow.readyState === 4 && this.ajax_dfollow.status === 200) {
        var receive = JSON.parse(this.ajax_dfollow.responseText)
        // console.log(receive);
        if (receive.getFlag === 1) {
          this.isFollowed = !this.isFollowed
        }
      }
    },
    answerTopic () {
      var forms = new FormData()
      forms.append('content', this.myAnswerContent)
      forms.append('topicID', this.thisTopic)
      forms.append('userID', this.myID)
      forms.append('parentID', -1)
      this.ajax_answerTopic = new XMLHttpRequest()
      this.ajax_answerTopic.open('POST', 'http://139.224.255.43:7779/Topic/createTopicAnswerByID', true)
      this.ajax_answerTopic.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_answerTopic.onreadystatechange = this.AT
      this.ajax_answerTopic.send(forms)
    },
    AT () {
      if (this.ajax_answerTopic.readyState === 4 && this.ajax_answerTopic.status === 200) {
        var receive = JSON.parse(this.ajax_answerTopic.responseText)
        if (receive.cerateTopicAnswerFlag === 1) {
          this.$alert('成功', '上传回答', {confirmButtonText: '确定'})
          // alert("上传回答成功！");
          this.myAnswerContent = ''
          this.getTopicDetailByID()
        } else {
          this.$alert('失败', '上传回答', {confirmButtonText: '确定'})
        }
        // console.log(receive);
      }
    },
    commentAnswer (item) {
      var forms = new FormData()
      forms.append('content', item.myComment)
      forms.append('topicID', this.thisTopic)
      forms.append('userID', this.myID)
      forms.append('parentID', item.firstLevelComment.TopicAnswerID)
      // console.log("http://139.224.255.43:7779/Topic/createTopicAnswerByID?content="+content+"&topicID="+topicID+"&userID="+userID+"&parentID="+parentID);
      this.ajax_commentAnswer = new XMLHttpRequest()
      this.ajax_commentAnswer.open('POST', 'http://139.224.255.43:7779/Topic/createTopicAnswerByID', true)
      this.ajax_commentAnswer.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_commentAnswer.onreadystatechange = this.CA
      this.ajax_commentAnswer.send(forms)
    },
    CA () {
      if (this.ajax_commentAnswer.readyState === 4 && this.ajax_commentAnswer.status === 200) {
        var receive = JSON.parse(this.ajax_commentAnswer.responseText)
        if (receive.cerateTopicAnswerFlag === 1) {
          this.$alert('成功', '评论操作', {confirmButtonText: '确定'})
          this.getTopicDetailByID()
        } else {
          this.$alert('失败', '评论操作', {confirmButtonText: '确定'})
        }
        // console.log(receive);
      }
    },
    checkFollow (item) {
      var idOfCurrentUser = this.myID
      var idOfCheckUser = this.thisTopicUser.userId
      // console.log(idOfCheckUser);
      this.ajax_checkFollow = new XMLHttpRequest()
      this.ajax_checkFollow.open('POST', 'http://139.224.255.43:7779/Account/checkFollow?idOfCurrentUser=' + idOfCurrentUser + '&idOfCheckUser=' + idOfCheckUser, true)
      this.ajax_checkFollow.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_checkFollow.onreadystatechange = this.CF
      this.ajax_checkFollow.send()
    },
    CF () {
      if (this.ajax_checkFollow.readyState === 4 && this.ajax_checkFollow.status === 200) {
        var receive = JSON.parse(this.ajax_checkFollow.responseText)
        if (receive.result === 'False') {
          this.isFollowed = false
        } else {
          this.isFollowed = true
        }
        // console.log(receive);
      }
    }
  }
}
</script>

<style>
  a{
    text-decoration: none;
    color: black;
  }
  #view-topic-page{
    position:relative;
    padding-bottom:40px;
    display:flex;
    flex-direction:column;
    align-items:center;
    text-align: left;
    min-width: calc(880px + 40px + 360px);
    background-color: #fdfdfd;
  }
  #vtp-main{
    display: flex;
    justify-content: center;
    padding: 20px 20px 0px 20px;
  }
  #left-aside{
    display: flex;
    flex-shrink: 0;
    flex-direction: column;
    width: 300px;
  }
  #author-info{
    display: flex;
    flex-direction: column;
    background-color: white;
    padding: 20px;
    box-shadow: 0px 0px 5px 0px rgba(0, 0, 0, 0.25);
  }
  #author-info-img-and-text{
    display: flex;
    align-items: center;
    height:max-content;
  }
  #author-info-img{
    width: 45px;
    height: 45px;
    border-radius: 45px;
  }
  #author-info-text{
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    height: 100%;
    margin-left: 20px;
  }
  #author-info-signature{
    color: #767373;
  }
  #author-info-buttons{
    display: flex;
    justify-content: space-between;
    margin-top: 20px;
    height: 30px;
    font-size: 14px;
    line-height: 14px;
  }
  #author-info-buttons>*{
    display: flex;
    justify-content: center;
    align-items: center;
    width: 76px;
    border-radius: 4px;
    cursor: pointer;
  }
  #follow{
    background: #B23535;
    color: white;
  }
  #personel-center{
    background: #DAD4D4;
  }
  .left-aside-topic-card{
    display: flex;
    flex-direction: column;
    margin-top: 20px;
    background: #FFFFFF;
    border-radius: 0px 20px 20px 20px;
    overflow: hidden;
    box-shadow: 0px 0px 5px 0px rgba(0, 0, 0, 0.25);
  }
  .left-aside-subtitle{
    width: max-content;
    padding: 20px 30px;
    background: #B23535;
    border-radius: 0px 0px 30px 0px;
    color: white;
    line-height: 18px;
    font-weight: bold;
    font-size: 18px;
  }
  .left-aside-list{
    padding: 10px 0px;
  }
  .left-aside-list-item{
    display: block;
    padding: 10px 20px;
    cursor: pointer;

    /*实现超过一行的显示省略号*/
    white-space: nowrap;
    text-overflow: ellipsis;
    overflow: hidden;
  }
  .left-aside-list-item:hover{
    color: #B23535;
  }
  #topic-and-answer{
    display: flex;
    flex-direction: column;
    width: 1000px;
    margin-left: 40px;
  }
  #this-topic{
    background-color: white;
    padding: 40px;
    box-shadow: 0px 0px 5px 0px rgba(0, 0, 0, 0.25);
  }
  #topic-title{
    font-weight: bold;
    font-size: 24px;
    line-height: 24px;
    overflow: hidden;
    word-wrap: break-word;
  }
  #topic-message{
    display: flex;
    margin-top: 10px;
    color: #6A6969;
    font-size: 13px;
    line-height: 13px;
  }
  #topic-zone{
    margin-left: 60px;
  }
  #topic-content{
    margin-top: 10px;
    overflow: hidden;
    word-wrap: break-word;
  }
  #answer-count{
    display: flex;
    align-items: center;
    height: 40px;
    margin-top: 10px;
    padding-left: 40px;
    background-color: white;
    font-size: 14px;
    line-height: 14px;
    color: #767373;
    box-shadow: 0px 0px 5px 0px rgba(0, 0, 0, 0.25);
  }
  .answer-item{
    display: flex;
    flex-direction: column;
    margin-top: 10px;
    padding: 40px;
    background-color: white;
    box-shadow: 0px 0px 5px 0px rgba(0, 0, 0, 0.25);
  }
  .answer-author{
    display: flex;
    align-items: center;
  }
  .answer-author-avatar{
    height: 40px;
    width: 40px;
    border-radius: 40px;
  }
  .answer-author-name{
    margin-left: 20px;
    font-size: 18px;
    line-height: 18px;
  }
  .answer-content{
    margin-top: 20px;
    overflow: hidden;
    word-wrap: break-word;
  }
  .answer-message-time{
    margin-top: 20px;
    font-size: 14px;
    line-height: 14px;
    color: #958F8F;
  }
  .answer-statistic{
    display: flex;
    margin-top: 20px ;
    font-size: 14px;
    line-height: 14px;
  }
  .answer-statistic-comment{
    cursor: pointer;
  }
  .answer-comments{
    display: flex;
    flex-direction: column;
    margin-top: 20px;
    box-shadow: inset 0px 0px 0px 1px #DAD4D4;
    padding: 20px;
  }
  .leave-comment{
    display: flex;
  }
  .leave-comment-input{
    flex-grow: 1;
    padding: 10px 20px;
    height: 28px;
    border-width: 0px;
    background-color: #F3F1F1;
    font-size: 14px;
    line-height: 28px;
  }
  .leave-comment-button{
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 0px 20px;
    background: #B23535;
    color: white;
    line-height: 16px;
    cursor: pointer;
  }
  .comment-item{
    display: flex;
    padding: 20px 0px;
  }
  .comment-author-avatar{
    flex-shrink: 0;
    width: 30px;
    height: 30px;
    border-radius: 30px;
  }
  .comment-author-name{
    flex-shrink: 0;
    margin-left: 20px;
    line-height: 30px;
    font-size: 14px;
  }
  .comment-content{
    margin: 6px 0px 0px 10px;
    font-size: 14px;
    color: #767373;
    line-height: 18px;
    overflow: hidden;
    word-wrap: break-word;
  }
  #my-answer-card{
    display: flex;
    flex-direction: column;
    margin-top: 20px;
    padding: 40px;
    background-color: white;
    box-shadow: 0px 0px 5px 0px rgba(0, 0, 0, 0.25);
  }
  #my-answer-card-subtitle{
    font-size: 20px;
    font-weight: bold;
    margin-bottom: 20px;
  }
  #my-answer-content{
    width: calc(100% - 40px);
    padding: 20px;
    height: 300px;
    font-size: 16px;
    font-family: "microsoft yahei";
  }
  #upload-answer{
    margin-top: 20px;
    padding: 10px 20px;
    align-self: flex-end;
    background-color: #B23535;
    color: white;
    border-radius: 4px;
    cursor: pointer;
  }

</style>
