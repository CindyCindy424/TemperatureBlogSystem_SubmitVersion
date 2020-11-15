<template>
  <div>
  <div class="loginBackground" :style="backgroundDiv">
  </div>
  <div>
  <div>
    <div :class="['intro']">
      <p :class="['introWord1']" align="left">{{introduction1}}
      </p>
      <p :class="['introWord2']" align="left">{{introduction2}}
      </p>
   </div>
   <div :class="['loginArea']">
      <div :class="['Edge']" style="position:absolute;left:-1px;top:-1px;"></div>
      <div :class="['Edge']" style="position:absolute;left:-1px;top:424px;"></div>
      <p :class="['loginTitle']">登录Temperature</p>
      <el-alert v-if="isLoginSuccess===0" title="密码错误" type="error" show-icon
      style="position:absolute;top:320px;left:222px;width:300px;"></el-alert>
      <el-alert v-else-if="isLoginSuccess===1" title="登陆成功，正在跳转！" type="success" show-icon
      style="position:absolute;top:320px;left:222px;width:300px;"></el-alert>
      <table :class="['inputToken']">
        <tr>
          <td>用户名：</td>
        </tr>
        <tr>
          <td>密码：</td>
        </tr>
      </table>
      <input :class="['inputBoxLogin']" v-model="account">
      <input type="password" :class="['inputBoxLogin']" style="top:-30px;" v-model="password">
      <el-button type="danger" style="position:absolute;top:360px;left:480px;" @click="submit">登录</el-button>
   </div>
  </div>
  </div>
  <footer :class="['footer']">
    <p style="margin:0 auto;"> Copyright ©2020 Temperature team, All Rights Reserved.</p>
  </footer>
  </div>
</template>

<script>
export default {
  name: 'Login',
  data () {
    return {
      isBackground: true,
      backgroundDiv: {
        backgroundRepeat: 'no-repeat',
        backgroundSize: 'cover',
        backgroundImage: 'url(' + require('../assets/images/login2.jpg') + ')'
      },
      introduction1: 'Temperature博客网是由数据库小组成员共同设计实现的，以分享生活，交流经验为主旨的博客网站。',
      introduction2: '加入Temperature的世界，用文字和图片尽情描绘生活的细节!',
      isLoginSuccess: -1,
      account: '',
      password: '',
      token: ''
    }
  },
  methods: {
    submit: function () {
      var xhr = new XMLHttpRequest()
      xhr.onreadystatechange = () => {
        if (xhr.readyState === 4 && xhr.status === 200) {
          var response = xhr.responseText
          var returnModel = JSON.parse(response)
          this.token = returnModel.token
          this.$emit('updateToken')
          if (returnModel.loginFlag === 1) {
            this.isLoginSuccess = 1
            setTimeout(() => { this.$router.push({path: '/mainPage'}) }, 3000)
          } else {
            this.isLoginSuccess = 0
          }
        }
      }
      xhr.open('POST', 'http://139.224.255.43:7779/Account/Login?nick_name=' + this.account + '&password=' + this.password)
      xhr.send()
    }
  },
  watch: {
    isLoginSuccess: function (newIsLoginSuccess, oldIsLoginSuccess) {
      if (newIsLoginSuccess === 0) {
        setTimeout(function () { this.isLoginSuccess = -1 }, 300)
      } else if (newIsLoginSuccess === 1) {
        this.$emit('loginSuc')
        this.$emit('propAccount', this.account)
      }
    }
  }
}
</script>

<style>
.loginBackground{
    position: absolute;
    width: 100%;
    height: 100%;
    top:0px;
    left:0px;
    z-index:-2;

}
.intro{
  position: relative;
  width: 390px;
  height: 450px;
  left: 25%;
  top: 250px;

  background: rgba(48, 46, 46, 0.7);
  border-radius: 36px;

}
.introWord1{
    position: relative;
    width: 291px;
    height: 240px;
    left: 50px;
    top: 100px;

    font-family: Microsoft YaHei;
    font-style: normal;
    font-weight: normal;
    font-size: 22px;
    line-height: 32px;
    color: #DDD8D8;
}
.introWord2{
    position: relative;
    width: 291px;
    height: 371px;
    left: 50px;
    top: 0px;

    font-family: Microsoft YaHei;
    font-style: normal;
    font-weight: normal;
    font-size: 22px;
    line-height: 32px;

    color: #DDD8D8;
}
.loginArea{
  position: absolute;
  width: 615px;
  height: 450px;
  left: 44%;
  top: 273px;

  background: #FFFFFF;
  border: 1px solid #FFFFFF;
  box-sizing: border-box;
  border-radius: 28px;
}
.loginTitle{
  position: relative;
  width: 343px;
  height: 52.21px;
  left: 220px;
  top: 40px;
  font-family: Microsoft YaHei;
  font-style: normal;
  font-weight: normal;
  font-size: 24px;
  line-height: 32px;
  display: flex;
  align-items: center;
  text-align: center;

  color: #535151;
}
.inputToken{
  position: relative;
  width: 200px;
  height: 146.17px;
  left: 80px;
  top: 70px;

  font-family: Microsoft YaHei;
  font-style: normal;
  font-weight: normal;
  font-size: 18px;
  line-height: 24px;
  align-items: center;
  text-align: center;

  color: #000000;
}
.inputBoxLogin{
  position: relative;
  width: 327px;
  height: 45px;
  left: 80px;
  top: -60px;
  background: #FFFFFF;
  border: 1px solid #9C9A9A;
  box-sizing: border-box;
}
.Edge{
  background: #ffffff;
  width: 31px;
  height: 25px;
}
.footer{
position:relative;
bottom:-350px;
font-family: Microsoft YaHei;
font-style: normal;
font-weight: normal;
font-size: 18px;
line-height: 24px;
display: flex;
align-items: center;
text-align: center;
color: #999494;
}
</style>
