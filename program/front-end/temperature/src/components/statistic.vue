<template>
  <div>
    <figure>
      <chart
        :options="pieArticle"
        ref="pieArticle"
        auto-resize
      />
      <chart
        :options="pieTopic"
        ref="pieTopic"
        auto-resize
      />
    </figure>
    <word-cloud-chart
            id="echarts05"
            title="词云"
            :data="echarts05Data"
            width="800px"
            height="600px"
          />
  </div>
</template>

<script>
import ECharts from './ECharts.vue'
import 'echarts/lib/chart/pie'
import 'echarts/lib/chart/bar'
import theme from '../theme.json'
ECharts.registerTheme('ovilia-green', theme)

export default{
  components: {
    chart: ECharts
    // WordCloudChart,
  },
  data () {
    return {
      wordCloudData: Array.from({length: 100}, (v, i) => ({
        name: `测试${i + 1}`,
        value: 10 + i
      })),
      chart: null,
      pieArticle: {
        color: [
          '#C8F2EE',
          '#C8EDF8',
          '#B1C7E7',
          '#A6CED1',
          '#D4E9A4',
          '#F0D472',
          '#F6A5A8',
          '#F2A468',
          '#EB88E1',
          '#ECD8FE',
          '#DABDB9',
          '#DFDFDF'
        ],
        title: {
          text: '各分区文章占比图',
          x: 'center'
        },
        tooltip: {
          trigger: 'item',
          formatter: '{a} <br/>{b} : {c} ({d}%)'
        },
        legend: {
          orient: 'vertical',
          left: 'left',
          data: ['学习', '生活', '开发', '游戏', '娱乐', '体育', '影视', '资讯', '时尚', '舞蹈', '音乐', '其他']
        },
        series: [
          {
            name: '文章分区',
            type: 'pie',
            radius: '55%',
            center: ['50%', '60%'],
            data: [],
            itemStyle: {
              emphasis: {
                shadowBlur: 10,
                shadowOffsetX: 0,
                shadowColor: 'rgba(0, 0, 0, 0.5)'
              }
            }
          }
        ]
      },
      pieTopicIf: false,
      pieTopic: {
        color: [
          '#C8F2EE',
          '#C8EDF8',
          '#B1C7E7',
          '#A6CED1',
          '#D4E9A4',
          '#F0D472',
          '#F6A5A8',
          '#F2A468',
          '#EB88E1',
          '#ECD8FE',
          '#DABDB9',
          '#DFDFDF'
        ],
        title: {
          text: '各话题分区占比图',
          x: 'center'
        },
        tooltip: {
          trigger: 'item',
          formatter: '{a} <br/>{b} : {c} ({d}%)'
        },
        legend: {
          orient: 'vertical',
          left: 'left',
          data: ['学习', '生活', '开发', '游戏', '娱乐', '体育', '影视', '资讯', '时尚', '舞蹈', '音乐', '其他']
        },
        series: [
          {
            name: '话题分区',
            type: 'pie',
            radius: '55%',
            center: ['50%', '60%'],
            data: [],
            itemStyle: {
              emphasis: {
                shadowBlur: 10,
                shadowOffsetX: 0,
                shadowColor: 'rgba(0, 0, 0, 0.5)'
              }
            }
          }
        ]
      },
      echarts05Data: [
        {name: '数据库', value: 15000},
        {name: '学习', value: 10081},
        {name: '内存', value: 9386},
        {name: '效率', value: 7500},
        {name: '周杰伦', value: 500},
        {name: 'MAMAMOO', value: 6500},
        {name: 'DataBase', value: 6500},
        {name: 'SQL', value: 6000},
        {name: '微软', value: 4500},
        {name: 'Oracle', value: 3800},
        {name: '谷歌', value: 3000},
        {name: 'MySQL', value: 2500},
        {name: '腾讯', value: 2300},
        {name: '安全', value: 2000},
        {name: '华为', value: 1900},
        {name: 'SQLite', value: 1800},
        {name: '网络安全', value: 1700},
        {name: 'Vue', value: 1600},
        {name: 'C#', value: 1500},
        {name: 'Linux', value: 1200},
        {name: 'Ubuntu', value: 1100},
        {name: '操作系统', value: 900},
        {name: 'nasm', value: 800},
        {name: 'GitHub', value: 700}
      ]
    }
  },
  mounted () {
    this.getStatisticData()
  },
  methods: {
    getTokenFromCookie () {
      // console.log(document.cookie);
      var cookie = document.cookie
      var cookieArr = cookie.split(';')
      for (var i = 0; i < cookieArr.length; i++) {
        var keyAndValue = cookieArr[i].split('=')
        if (keyAndValue[0].trim() === 'token') {
          return keyAndValue[1]
        }
      }
    },
    getStatisticData () {
      this.ajax_getStatisticData = new XMLHttpRequest()
      this.ajax_getStatisticData.open('POST', 'http://139.224.255.43:7779/Zone/getArticleRatio', true)
      this.ajax_getStatisticData.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_getStatisticData.onreadystatechange = this.getSD
      this.ajax_getStatisticData.send()

      this.ajax_getStatisticData2 = new XMLHttpRequest()
      this.ajax_getStatisticData2.open('POST', 'http://139.224.255.43:7779/Zone/getTopicRatio', true)
      this.ajax_getStatisticData2.setRequestHeader('Authorization', 'Bearer ' + this.getTokenFromCookie())
      this.ajax_getStatisticData2.onreadystatechange = this.getSD2
      this.ajax_getStatisticData2.send()
    },
    getSD () {
      if (this.ajax_getStatisticData.readyState === 4 && this.ajax_getStatisticData.status === 200) {
        var receive = JSON.parse(this.ajax_getStatisticData.responseText)
        // console.log(receive);
        if (receive !== undefined) {
          this.pieArticle.series[0].data = receive
        } else {
          this.$alert('getArticleRatio', '请求错误', {confirmButtonText: '确定'})
        }
      }
    },
    getSD2 () {
      if (this.ajax_getStatisticData2.readyState === 4 && this.ajax_getStatisticData2.status === 200) {
        var receive = JSON.parse(this.ajax_getStatisticData2.responseText)
        // console.log(receive);
        if (receive !== undefined) {
          this.pieTopic.series[0].data = receive
          this.pieAnim()
        } else {
          this.$alert('getTopicRatio', '请求错误', {confirmButtonText: '确定'})
        }
      }
    },
    pieAnim () {
      var dataIndex = -1
      var pieArticle = this.$refs.pieArticle
      var pieTopic = this.$refs.pieTopic
      var dataLen = pieArticle.options.series[0].data.length
      setInterval(() => {
        pieArticle.dispatchAction({
          type: 'downplay',
          seriesIndex: 0,
          dataIndex
        })
        pieTopic.dispatchAction({
          type: 'downplay',
          seriesIndex: 0,
          dataIndex
        })
        dataIndex = (dataIndex + 1) % dataLen
        pieArticle.dispatchAction({
          type: 'highlight',
          seriesIndex: 0,
          dataIndex
        })
        // 显示 tooltip
        pieArticle.dispatchAction({
          type: 'showTip',
          seriesIndex: 0,
          dataIndex
        })
        pieTopic.dispatchAction({
          type: 'highlight',
          seriesIndex: 0,
          dataIndex
        })
        // 显示 tooltip
        pieTopic.dispatchAction({
          type: 'showTip',
          seriesIndex: 0,
          dataIndex
        })
      }, 1000)
      // this.connected = true;
      this.connected = true
    }
  }
}
</script>

<style>
  figure{
    display:flex;
    width: max-content;
    margin:2em auto;
    border:1px solid rgba(0, 0, 0, .1);
    border-radius:8px;
    box-shadow: 0 0 45px rgba(0, 0, 0, .2);
    padding:1.5em 2em;
  }
  .echarts{
    width:40vw;
    min-width:400px;
    height: 300px
  }
  .chart {
    width: 100%;
    height: 300px;
  }
</style>
