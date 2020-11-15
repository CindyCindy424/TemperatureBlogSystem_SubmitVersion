import Vue from 'vue'
import Router from 'vue-router'
import Login from '../components/Login.vue'
import Register from '../components/Register.vue'
import MySpace from '../components/MySpace.vue'
import MainPage from '../components/MainPage.vue'
import Album from '../components/Album.vue'
import Photo from '../components/Photo.vue'
import Favourite from '../components/Favourite.vue'
import TopicArea from '../components/TopicArea.vue'
import PostTopic from '../components/PostTopic.vue'
import ViewTopic from '../components/ViewTopic.vue'
import MyArticle from '../components/MyArticle.vue'
import ViewArticle from '../components/ViewArticle.vue'
import UploadArticle from '../components/UploadArticle.vue'
import MyHome from '../components/MyHome.vue'
import SearchResult from '../components/SearchResult.vue'
Vue.use(Router)
const routes = [{
  path: '/',
  name: 'Login',
  component: Login
},
{
  path: '/MyHome',
  name: 'MyHome',
  component: MyHome
},
{
  path: '/SearchResult',
  name: 'SearchResult',
  component: SearchResult
},
{
  path: '/UploadArticle',
  name: 'UploadArticle',
  component: UploadArticle
},
{
  path: '/Favourite',
  name: 'Favourite',
  component: Favourite
},
{
  path: '/PostTopic',
  name: 'PostTopic',
  component: PostTopic
},
{
  path: '/ViewTopic',
  name: 'ViewTopic',
  component: ViewTopic
},
{
  path: '/Photo',
  name: 'Photo',
  component: Photo
},
{
  path: '/Album',
  name: 'Album',
  component: Album
},
{
  path: '/Login',
  name: 'Login',
  component: Login
},
{
  path: '/Register',
  name: 'Register',
  component: Register
},
{
  path: '/MySpace',
  name: 'MySpace',
  component: MySpace
},
{
  path: '/MainPage',
  name: 'MainPage',
  component: MainPage
},
{
  path: '/TopicArea',
  name: 'TopicArea',
  component: TopicArea
},
{
  path: '/MyArticle',
  name: 'MyArticle',
  component: MyArticle
},
{
  path: '/ViewArticle',
  name: 'ViewArticle',
  component: ViewArticle
}
]

export default new Router({
  routes
})
