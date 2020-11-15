<template>
<div id="app">
    <div :class="['navigation']">
        <img src="/static/images/TempratureLogo.png" :class="{logo:isLogo}">
        <router-link to="/MainPage" :class="['title']">Temperature</router-link>
        <input :class="['navigationSearch']" v-model="toBeSearched">
        <img src="/static/images/search.png" @click="this.pushToSearchResult" style="position:absolute;left:775px;top:17px;height:30px;width:30px;z-index:999;" >
        <el-menu :default-active="activeIndex" class="el-menu-demo" mode="horizontal" text-color="#FFFFFF"
        @select="handleSelect">
          <el-menu-item index="/favourite" v-if='isLogin' :class="['favourite']" @click="this.pushToFavourite">收藏夹</el-menu-item>
          <el-submenu index="/compose" v-if='isLogin' :class="['compose']" active-text-color="#FFFFFF">
            <template slot="title">创作</template>
            <el-menu-item index="/compose/text" style="color:black;font-size:20px;height:40px;border-bottom:2px solid grey;" @click="this.pushToArticle">文章</el-menu-item>
            <el-menu-item index="/compose/topic" style="color:black;font-size:20px;height:40px;border-bottom:2px solid grey;" @click="this.pushToTopic">话题</el-menu-item>
          </el-submenu>
          <el-submenu index="/MyArea" v-if='isLogin' :class="['myArea']" active-text-color="#FFFFFF">
            <template slot="title">我的</template>
            <el-menu-item style="color:black;font-size:20px;height:40px;border-bottom:2px solid grey;" @click="this.pushToMyArticle">文章</el-menu-item>
            <el-menu-item style="color:black;font-size:20px;height:40px;border-bottom:2px solid grey;" @click="this.pushToMyTopic">话题</el-menu-item>
                        <el-menu-item style="color:black;font-size:20px;height:40px;border-bottom:2px solid grey;" @click="this.pushToHome">个人中心</el-menu-item>
            <el-menu-item style="color:black;font-size:20px;height:40px;" @click="this.pushToMyPic">图片</el-menu-item>

          </el-submenu>
        </el-menu>
        <router-link  to="/Login"   :class="['login']" v-show="isLogin==0">登录</router-link>
        <router-link to="/Register" :class="['register']" v-show="isLogin==0">注册</router-link>
        <el-avatar v-show="isLogin==1" style="position:absolute;top:10px;left:90%;z-index:999;" :src="this.avator"></el-avatar>
        <router-link :to="{path:'/MySpace',query:{account:this.account}}" v-text="account" :class="['mySpace']" v-show="isLogin"></router-link>
    </div>
    <router-view ref="view" :userName="this.account" :userID="this.userID" @turnToRegister="turnToRegister()" @loginSuc="loginSuccess()" @propAccount="changeAccount" @updateToken='updateToken()'></router-view>
</div>
</template>

<script>
export default {
  name: 'app',
  data: function () {
    return {
      currentPage: 'Login',
      isLogo: true,
      isLogin: false,
      isRegister: true,
      account: '',
      activeIndex: '1',
      activeIndex2: '1',
      avator: '',
      token: '',
      userID: '',
      inputSearch: '',
      toBeSearched: ''
    }
  },
  beforeCreate () {
    document.querySelector('body').setAttribute('style', 'margin:0px;padding:0px;')
  },
  created () {
    this.token = document.cookie.split(';')[0].split('=')[1]
    if (typeof (this.token) === 'undefined' && this.isLogin === true) {
      this.isLogin = false
      this.$router.push({path: '/login'})
      window.alert('登录已过期，请重新登录')
    }
    if (this.isLogin === false) {
      this.$router.push({path: '/login'})
    }
  },
  methods: {
    loginSuccess: function () {
      this.isLogin = true
    },
    getUserID: function () {
      var xhr = new XMLHttpRequest()
      xhr.onreadystatechange = () => {
        var response = xhr.responseText
        var returnModel = JSON.parse(response)
        this.userID = returnModel.userInfo.userId
      }
      var headerToken = document.cookie.split(';')[0].split('=')[1]
      xhr.open('POST', 'http://139.224.255.43:7779/Account/getUserInfoByNickName?nick_name=' + this.account)
      xhr.setRequestHeader('Authorization', 'Bearer ' + headerToken)
      xhr.send()
    },
    changeAccount: function (evtValue) {
      this.account = evtValue
    },
    handleSelect (key, keyPath) {
      console.log(key, keyPath)
    },
    updateToken: function () {
      this.token = this.$refs.view.token
      var exdate = new Date()
      exdate.setTime(exdate.getTime() + 20 * 60 * 1000)
      document.cookie = 'token=' + this.token + ';expires=' + exdate.toUTCString()
    },
    turnToRegister: function () {
      this.currentPage = 'Register'
    },
    pushToMyPic: function () {
      this.$router.push({path: '/Album', query: {userID: this.userID, currentUserID: this.userID}})
    },
    pushToFavourite: function () {
      this.$router.push({path: '/Favourite', query: {userID: this.userID, currentUserName: this.account}})
    },
    pushToArticle: function () {
      this.$router.push({path: '/UploadArticle', query: {UploadArticlename: this.account}})
    },
    pushToHome: function () {
      this.$router.push({path: '/MyHome', query: {hostId: this.userID, myId: this.userID}})
    },
    pushToSearchResult: function () {
      this.$router.push({path: '/SearchResult', query: {SearchContent: this.toBeSearched, ArticleOrTopic: 1, userId: this.userID}})
    },
    pushToMyArticle: function () {
      this.$router.push({path: '/MyArticle', query: {MyArticleid: this.userID, MyArticlename: this.account}})
    },
    pushToTopic: function () {
      this.$router.push({path: '/PostTopic', query: {userID: this.userID}})
    },
    pushToMyTopic: function () {
      this.$router.push({path: '/TopicArea', query: {userID: this.userID, zoneID: 1}})
    }
  },
  watch: {
    account (oldVal, newVal) {
      var xhr2 = new XMLHttpRequest()
      xhr2.onreadystatechange = () => {
        if (xhr2.readyState === 4 && xhr2.status === 200) {
          var response = xhr2.responseText
          this.returnInfo = JSON.parse(response)
          this.getUserID()
          if (this.returnInfo.flag === 1) {
            this.avator = 'http://139.224.255.43:7779/BlogPics/Avator/' + this.returnInfo.path.split('\\')[2]
          }
        }
      }
      var headerToken = document.cookie.split(';')[0].split('=')[1]
      xhr2.open('POST', 'http://139.224.255.43:7779/Account/getAvatrResource?nick_name=' + this.account)
      xhr2.setRequestHeader('Authorization', 'Bearer ' + headerToken)
      xhr2.send()
    }
  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  height:100%;
}

#nav {
  padding: 30px;
}

#nav a {
  font-weight: bold;
  color: #2c3e50;
}

#nav a.router-link-exact-active {
  color: #42b983;
}
.navigation {
    position:fixed;
    top:0px;
    left:0px;
    background-color: transparent;
    width: 100%;
    height: 60px;
    z-index:999;
    backdrop-filter:blur(20px);
    background-color:rgba(0,0,0,0.25);
}
.logo {
    position: absolute;
    width: 38px;
    height: 38px;
    left: 5%;
    top: 10px;
    z-index:999;
}

.title {
    position: absolute;
    width: 173px;
    height: 33px;
    left: 7%;
    top: 15px;
    margin-top: 0%;
    font-family: Lucida Handwriting;
    font-style: italic;
    font-weight: normal;
    font-size: 24px;
    line-height: 33px;
    color: #FFFFFF;
    text-decoration: none;
    z-index:999;
}

.navigationSearch {
    position: absolute;
    width: 339px;
    height: 35px;
    left: 480px;
    top: 12px;
    background: transparent;
    border-radius: 30px;
    outline:none;
    text-indent:20px;
    font-family: SimSun;
    font-style: normal;
    font-weight: normal;
    font-size: 18px;
    z-index:999;
    border-width:0px;
    background-color:rgba(255,255,255,0.6);
}

.message {
    position: absolute;
    width: 72px;
    height: 24px;
    left: 50%;
    top: 0px;
    font-family: Microsoft YaHei;
    font-style: normal;
    font-weight: normal;
    font-size: 18px;
    line-height: 24px;
    /* identical to box height */
    z-index:999;
    text-decoration-line: none;
    color: #FFFFFF;
}

.favourite {
    position: absolute;
    width: 100px;
    height: 24px;
    left: 50%;
    top: 0px;
    font-family: Microsoft YaHei;
    font-style: normal;
    font-weight: normal;
    font-size: 18px;
    line-height: 24px;
    /* identical to box height */
    text-decoration-line: none;
    color: #FFFFFF;
    z-index:999;
}

.compose {
    position: absolute;
    width: 100px;
    height: 24px;
    left: 60%;
    top: 0px;

    font-family: Microsoft YaHei;
    font-style: normal;
    font-weight: normal;
    font-size: 24px;
    line-height: 24px;
    text-decoration-line: none;
    /* identical to box height */
    z-index:999;
}
.myArea{
    position: absolute;
    width: 100px;
    height: 24px;
    left: 70%;
    top: 0px;

    font-family: Microsoft YaHei;
    font-style: normal;
    font-weight: normal;
    font-size: 24px;
    line-height: 24px;
    text-decoration-line: none;
    /* identical to box height */
    z-index:999;
}
.mySpace {
    position: absolute;
    width: 36px;
    height: 24px;
    left: 93%;
    top: 19px;
    z-index:999;
    font-family: Microsoft YaHei;
    font-style: normal;
    font-weight: normal;
    font-size: 18px;
    line-height: 24px;
    /* identical to box height */
    color: #FFFFFF;
    text-decoration-line: none;
}

.login {
    position: absolute;
    width: 88px;
    height: 24px;
    left: 90%;
    top: 17px;
    font-family: Microsoft YaHei;
    font-style: normal;
    font-weight: normal;
    font-size: 18px;
    line-height: 24px;
    /* identical to box height */
z-index:999;
    text-decoration-line: none;
    color: #FFFFFF;
}

.register {
    position: absolute;
    width: 88px;
    height: 24px;
    left: 92.5%;
    top: 17px;
    font-family: Microsoft YaHei;
    font-style: normal;
    font-weight: normal;
    font-size: 18px;
    line-height: 24px;
    /* identical to box height */
z-index:999;
    text-decoration-line: none;

    color: #FFFFFF;
}
.el-submenu__title {
  font-size: 18px;
}
.el-menu--horizontal>.el-submenu.is-active .el-submenu__title {
    color: #FFFFFF;
}
.el-menu{
  background-color:transparent;
}
.el-menu-item{
  background-color:transparent!important;
  border-bottom: 0px;
}
.el-submenu{
  opacity:1;
  border-bottom: 0px;
}
.el-submenu:hover{
  background-color:transparent;
  opacity:0.5;
}
.el-menu-item:hover{
  background-color:transparent;
  opacity:0.5;
}
.el-submenu.is-active{
  background-color:transparent!important;
}
.el-menu.el-menu--horizontal {
    border-bottom: none;
}
.el-menu--horizontal>.el-submenu {
    float: none;
}
</style>
