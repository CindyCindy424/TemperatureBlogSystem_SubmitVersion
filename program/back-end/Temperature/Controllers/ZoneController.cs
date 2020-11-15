using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Temperature.Models;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Temperature.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class ZoneController : Controller
    {
        private blogContext entity = new blogContext(); //整体数据库类型


        /// <summary>
        /// 返回分区名
        /// </summary>
        /// <param name="zoneID">分区id</param>
        /// <returns></returns>
        /// <remarks>
        ///     返回：
        ///     
        ///     flag：0 未操作
        ///     
        ///     flag：1 成功
        ///     
        ///         返回：{ Flag = returnFlag, zoneName = name }
        ///         
        ///     flag：2 不存在该分区
        ///     
        ///         返回：{ Flag = returnFlag, errorMsg = msg }
        /// </remarks>
        [HttpPost]
        public JsonResult getZoneName(int zoneID)
        {
            var returnFlag = 0;
            string msg = "";
            string name = "";
            try
            {
                var zoneName =
                (from u in entity.Zone
                 where u.ZoneId == zoneID
                 select u.ZoneName).Distinct();
                name = zoneName.FirstOrDefault();
                if (zoneName.FirstOrDefault() == default)
                {
                    returnFlag = 2;//没有该分区
                }
                else
                {
                    returnFlag = 1;
                }
            }
            catch(Exception e)
            {
                returnFlag = 0;
                msg = e.Message;
            }

            if(returnFlag == 1)
            {
                return Json(new { Flag = returnFlag, zoneName = name });
            }
            else
            {
                return Json(new { Flag = returnFlag, errorMsg = msg });
            }

               
        }

        /// <summary>
        /// 获取各分区文章数量
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///     返回：
        ///         { u.ZoneId, u.ZoneName, u.ZoneArticleNum }
        /// </remarks>
        [HttpPost]
        public JsonResult getArticleRatio()
        {
            var result =
                (from u in entity.Zone
                 select new { id = u.ZoneId, name = u.ZoneName,value =  u.ZoneArticleNum }).Distinct();
            return Json(result);
        }

        /// <summary>
        /// 获取各分区话题数量
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///     返回：
        ///         { u.ZoneId, u.ZoneName, u.ZoneTopicNum }
        /// </remarks>
        [HttpPost]
        public JsonResult getTopicRatio()
        {
            var result =
                (from u in entity.Zone
                 select new { id = u.ZoneId, name = u.ZoneName, value = u.ZoneTopicNum }).Distinct();
            return Json(result);
        }

    }
}
