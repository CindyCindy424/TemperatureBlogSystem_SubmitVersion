using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Temperature.Models;

namespace Temperature.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class FavouriteController : Controller
    {
        private blogContext entity = new blogContext(); //整体数据库类型

        /// <summary>
        /// 新建收藏夹(禁止同一用户创建同名收藏夹）
        /// </summary>
        /// <param name="nick_name">用户名</param>
        /// <param name="folderName">收藏夹名</param>
        /// <returns></returns>
        ///<remarks>
        ///     返回信息：
        ///     {
        ///         ReturnFlag = flag,
        ///         user = nick_name,
        ///         folder = folderName
        ///     }
        ///     
        ///     flag:
        ///     
        ///     0：未操作
        ///     
        ///     1：成功
        ///     
        ///     2：该用户已存在同名收藏夹
        /// </remarks>
        [HttpPost]
        public JsonResult createFolderByNickName(string nick_name,string folderName)
        {
            var flag = 0;
            var userid =
                    (from c in entity.User
                     where c.NickName == nick_name
                     select c.UserId).Distinct();
            var id = userid.FirstOrDefault();
            //var user = entity.User.Find(id); //在数据库中根据key找到相应记录

            //检查之前是否已经有过同名收藏夹
            var checkFolder =
                (from c in entity.Favourite
                 where (c.FavouriteName == folderName && c.UserId == id)
                 select c.FavouriteId).Distinct();
            var check = checkFolder.FirstOrDefault();
            if(check != default )
            {
                //Response.StatusCode = 400;//该用户已存在同名收藏夹
                flag = 2;
                return Json(new {ReturnFlag = flag, UserName = nick_name, Folder_Name = folderName, result = "Already Exists!" });
            }

            var folder = new Favourite();
            folder.FavouriteName = folderName;
            folder.ArticleNum = 0;
            folder.UserId = id;
            entity.Favourite.Add(folder);
            entity.SaveChanges();
            //Response.StatusCode = 200;
            flag = 1;
            return Json(new { ReturnFlag = flag, user = nick_name, folder = folderName });
        }

        /// <summary>
        /// 向收藏夹中添加文章
        /// </summary>
        /// <param name="nick_name">用户名</param>
        /// <param name="folderName">文件夹名</param>
        /// <param name="articleID">文章ID</param>
        /// <returns></returns>
        /// <remarks>
        ///     flag
        ///     
        ///     0：未操作
        ///     
        ///     1：成功
        ///     
        ///         返回：{ReturnFlag = flag, folderID = F_id, articleId = articleID }
        ///         
        ///     2：没找到该用户
        ///     
        ///         返回：{ReturnFlag = flag, UserID = id, result = "NOT FOUND"}
        ///         
        ///     3：没找到该文件夹
        ///     
        ///         返回：{ReturnFlag = flag, folderID = F_id, result = "NOT FOUND" }
        ///         
        ///     4：没有该文章
        ///     
        ///         返回：{ ReturnFlag = flag, ArticleID = articleID, result = "Article NOT FOUND!"}
        ///         
        /// </remarks>
        [HttpPost]
        public JsonResult addArticleByID(string nick_name,string folderName, int articleID)
        {
            var flag = 0;
            //根据用户名找到用户ID
            var userid =
                    (from c in entity.User
                     where c.NickName == nick_name
                     select c.UserId).Distinct();
            var id = userid.FirstOrDefault();
            string msg = "";
            if (id == default)
            {
                flag = 2;
                //Response.StatusCode = 405;//没找到该用户
                return Json(new { ReturnFlag = flag, UserID = id, result = "NOT FOUND" });
            }

            var folderid =
                    (from c in entity.Favourite
                     where (c.FavouriteName == folderName && c.UserId == id)
                     select c.FavouriteId).Distinct();
            var F_id = folderid.FirstOrDefault();
            if(F_id == default)
            {
                //Response.StatusCode = 404;//没找到该文件夹
                flag = 3;
                return Json(new { ReturnFlag = flag, folderID = F_id, result = "NOT FOUND" });
            }

            try
            {
                //FAVOURITE里面更新收藏夹文章数量
                var folder = entity.Favourite.Find(F_id);
                var num = folder.ArticleNum;
                if (num == default)
                    folder.ArticleNum = 1;
                else
                    folder.ArticleNum = num + 1;
                entity.Entry(folder).State = EntityState.Modified;
                //entity.SaveChanges();

                //ARTICLE表里面更新文章收藏量
                var article = entity.Article.Find(articleID);
                if (article == default)
                {
                    //Response.StatusCode = 400;//没有该文章
                    flag = 4;
                    return Json(new { ReturnFlag = flag, ArticleID = articleID, result = "Article NOT FOUND!" });
                }
                var collectNum = article.CollectNum;
                if (collectNum == default)
                    article.CollectNum = 1;
                else
                    article.CollectNum = collectNum + 1;
                entity.Entry(article).State = EntityState.Modified;
                //entity.SaveChanges();

                //加入FAVOURITE_ARTICLE表
                var item = new FavouriteArticle();
                item.FavouriteId = F_id;
                item.ArticleId = articleID;
                item.FavouriteTime = DateTime.Now;
                entity.FavouriteArticle.Add(item);
                entity.SaveChanges();

                //Response.StatusCode = 200;//成功
                flag = 1;
                return Json(new { ReturnFlag = flag, folderID = F_id, articleId = articleID });
            }
            catch (Exception e)
            {
                flag = 0;
                msg = e.Message;
                
            }
            return Json(new { Flag = flag, errorMsg = msg });
            
        }

        /// <summary>
        /// 获取收藏夹总览信息
        /// </summary>
        /// <param name="nick_name">用户名</param>
        /// <param name="folderName">收藏夹名</param>
        /// <returns></returns>
        /// <remarks>
        ///     flag
        ///     
        ///     0：未操作
        ///     
        ///     1：成功
        ///     
        ///         返回：{ReturnFlag = flag, INFO = info }
        ///         
        ///     2：没找到该用户
        ///     
        ///         返回：{ReturnFlag = flag, UserName= nick_name, result = "NOT FOUND"}
        ///         
        ///     3：没找到该文件夹
        ///     
        ///         返回：{ReturnFlag = flag, FolderName=folderName, result = "NOT FOUND" }
        ///         
        /// </remarks>
        [HttpPost]
        public JsonResult getFolderInfoByName(string nick_name, string folderName)
        {
            var flag = 0;
            //根据用户名找到用户ID
            var userid =
                    (from c in entity.User
                     where c.NickName == nick_name
                     select c.UserId).Distinct();
            var id = userid.FirstOrDefault();
            if (id == default)
            {
                //Response.StatusCode = 405;//没找到该用户
                flag = 2;
                return Json(new { ReturnFlag = flag, UserName= nick_name, result = "NOT FOUND" });
            }

            //根据用户ID和收藏夹名找到对应收藏夹项
            var folderid =
                    (from c in entity.Favourite
                     where (c.FavouriteName == folderName && c.UserId == id)
                     select c.FavouriteId).Distinct();
            var F_id = folderid.FirstOrDefault();
            if (F_id == default)
            {
                //Response.StatusCode = 404;//没找到该文件夹
                flag = 3;
                return Json(new { ReturnFlag = flag,FolderName=folderName, result = "NOT FOUND" });
            }

            var info = entity.Favourite.Find(F_id);
            //Response.StatusCode = 200;//成功
            flag = 1;
            return Json(new { ReturnFlag = flag, INFO = info });
        }

        /// <summary>
        /// 删除收藏夹
        /// </summary>
        /// <param name="nick_name">用户名</param>
        /// <param name="folderName">收藏夹名</param>
        /// <returns></returns>
        /// <remarks>
        ///     flag
        ///     
        ///     0：未操作
        ///     
        ///     1：成功
        ///     
        ///         返回：{ReturnFlag = flag, UserName = nick_name, FolderName = folderName,result ="success!" }
        ///         
        ///     2：没找到该用户
        ///     
        ///         返回：{ReturnFlag = flag, UserName = nick_name,  result = "NOT FOUND"}
        ///         
        ///     3：没找到该文件夹
        ///     
        ///         返回：{ReturnFlag = flag, FolderName = folderName, result = "NOT FOUND" }
        /// </remarks>
        [HttpPost]
        public JsonResult deleteFolderByName(string nick_name, string folderName)
        {
            var flag = 0;
            //根据用户名找到用户ID
            var userid =
                    (from c in entity.User
                     where c.NickName == nick_name
                     select c.UserId).Distinct();
            var id = userid.FirstOrDefault();
            if (id == default)
            {
                //Response.StatusCode = 405;//没找到该用户
                flag = 2;
                return Json(new { ReturnFlag = flag, UserName = nick_name, result = "NOT FOUND" });
            }

            //根据用户ID和收藏夹名找到对应收藏夹项
            var folderid =
                    (from c in entity.Favourite
                     where (c.FavouriteName == folderName && c.UserId == id)
                     select c.FavouriteId).Distinct();
            var F_id = folderid.FirstOrDefault();
            if (F_id == default)
            {
                //Response.StatusCode = 404;//没找到该文件夹
                flag = 3;
                return Json(new { ReturnFlag = flag, FolderName = folderName, result = "NOT FOUND" });
            }

            //ARTICLE表中找到对应记录,并更新记录
            var articles =
                (from c in entity.FavouriteArticle
                 from u in entity.Article
                 where c.FavouriteId == F_id && c.ArticleId==u.ArticleId
                 select u).ToList(); //这里需要把查询的内容转化为list，不然后面的foreach会报重复连接数据库的错误
            foreach(var article in articles)
            {
                article.CollectNum--;
                entity.Entry(article).State = EntityState.Modified;
                entity.SaveChanges();
            }

            //获得FAVORITE_ARTICLE表中记录
            var F_A_record =
                (from c in entity.FavouriteArticle
                 where c.FavouriteId == F_id
                 select c).ToList();
            foreach (var record in F_A_record)
            {
                var articleID = record.ArticleId;
                //var article = entity.Article.Find(articleID);

                //FAVORITE_ARTICLE表中删除项
                entity.Entry(record).State = EntityState.Deleted;
                entity.SaveChanges();
            }

            //FAVOURITE表中删除整个文件夹
            var info = entity.Favourite.Find(F_id);
            entity.Entry(info).State = EntityState.Deleted;
            entity.SaveChanges();

            //Response.StatusCode = 200;
            flag = 1;
            return Json(new { ReturnFlag = flag, UserName = nick_name,FolderName = folderName,result ="success!" });
        }

        /// <summary>
        /// 更新收藏夹名字
        /// </summary>
        /// <param name="nick_name">用户名</param>
        /// <param name="oldName">旧收藏夹名</param>
        /// <param name="newName">新收藏夹名</param>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// 
        ///     flag
        ///     
        ///     0：未操作
        ///     
        ///     1：成功
        ///     
        ///         返回：{ReturnFlag = flag, UserName = nick_name, OldName = oldName, NewName = newName  }
        ///         
        ///     2：没找到该用户
        ///     
        ///         返回：{ReturnFlag = flag, UserName = nick_name, result = "NOT FOUND"}
        ///         
        ///     3：没找到该文件夹
        ///     
        ///         返回：{ReturnFlag = flag, folderName = oldName, result = "NOT FOUND" }
        ///         
        /// </remarks>
        [HttpPost]
        public JsonResult updateFolderNameByName(string nick_name, string oldName,string newName)
        {
            var flag = 0;
            //根据用户名找到用户ID
            var userid =
                    (from c in entity.User
                     where c.NickName == nick_name
                     select c.UserId).Distinct();
            var id = userid.FirstOrDefault();
            if (id == default)
            {
                //Response.StatusCode = 405;//没找到该用户
                flag = 2;
                return Json(new { ReturnFlag = flag, UserName = nick_name, result = "NOT FOUND" });
            }

            //根据用户ID和收藏夹名找到对应收藏夹项
            var folderid =
                    (from c in entity.Favourite
                     where (c.FavouriteName == oldName && c.UserId == id)
                     select c.FavouriteId).Distinct();
            var F_id = folderid.FirstOrDefault();
            if (F_id == default)
            {
                //Response.StatusCode = 404;//没找到该文件夹
                flag = 3;
                return Json(new { ReturnFlag = flag, folderName = oldName, result = "NOT FOUND" });
            }

            var info = entity.Favourite.Find(F_id);
            info.FavouriteName = newName;
            entity.Entry(info).State = EntityState.Modified;
            entity.SaveChanges();

            //Response.StatusCode = 200;//成功
            flag = 1;
            return Json(new { ReturnFlag = flag, UserName = nick_name, OldName = oldName, NewName = newName });
        }

        /// <summary>
        /// 删除收藏夹中的文章
        /// </summary>
        /// <param name="nick_name">用户名</param>
        /// <param name="folderName">收藏夹名</param>
        /// <param name="articleID">文章ID</param>
        /// <returns></returns>
        /// <remarks>
        ///     flag
        ///     
        ///     0：未操作
        ///     
        ///     1：成功
        ///     
        ///         返回：{ReturnFlag = flag, result = "successful deleted"}
        ///         
        ///     2：没找到该用户
        ///     
        ///         返回：{ ReturnFlag = flag,UserName = nick_name, result = "NOT FOUND" }
        ///         
        ///     3：没找到该文件夹
        ///     
        ///         返回：{ReturnFlag = flag, FolderName = folderName, result = "NOT FOUND" }
        ///         
        ///     4：收藏夹里没有该文章
        ///     
        ///         返回：{ReturnFlag = flag, folderID = F_id, ArticleID = articleID, result = "NOT FOUND"}
        ///         
        ///     5：收藏夹为空
        ///     
        ///         返回：{ReturnFlag = flag, result = "EMPTY FOLDER"}
        ///         
        /// </remarks>
        [HttpPost]
        public JsonResult deleteArticleByID(string nick_name, string folderName, int articleID)
        {
            var flag = 0;
            //根据用户名找到用户ID
            var userid =
                    (from c in entity.User
                     where c.NickName == nick_name
                     select c.UserId).Distinct();
            var id = userid.FirstOrDefault();
            if (id == default)
            {
                //Response.StatusCode = 405;//没找到该用户
                flag = 2;
                return Json(new { ReturnFlag = flag,UserName = nick_name, result = "NOT FOUND" });
            }

            //根据用户ID和收藏夹名找到对应收藏夹项
            var folderid =
                    (from c in entity.Favourite
                     where (c.FavouriteName == folderName && c.UserId == id)
                     select c.FavouriteId).Distinct();
            var F_id = folderid.FirstOrDefault();
            if (F_id == default)
            {
                //Response.StatusCode = 404;//没找到该文件夹
                flag = 3;
                return Json(new { ReturnFlag = flag, FolderName = folderName, result = "NOT FOUND" });
            }

            var folder = entity.Favourite.Find(F_id);

            var item = entity.FavouriteArticle.Find(F_id, articleID);
            if(item == default)
            {
                //Response.StatusCode = 406;//收藏夹里没有该文章
                flag = 4;
                return Json(new { ReturnFlag = flag, folderID = F_id, ArticleID = articleID, result = "NOT FOUND" });
            }

            //FAVORITE表中文章数量修改
            var num = folder.ArticleNum;
            if (num ==default)
            {
                //Response.StatusCode = 400;//收藏夹内没有文章
                flag = 5;
                return Json(new { ReturnFlag = flag, result = "EMPTY FOLDER" });
            }
            folder.ArticleNum = num -1;
            entity.Entry(folder).State = EntityState.Modified;
            //entity.SaveChanges();

            //ARTICLE中收藏数更新
            var article = entity.Article.Find(articleID);
            article.CollectNum--;
            entity.Entry(article).State = EntityState.Modified;
            //entity.SaveChanges();

            //FAVORITE_ARTICLE表删除
            //var item = entity.FavouriteArticle.Find(F_id, articleID);
            
            entity.Entry(item).State = EntityState.Deleted;
            entity.SaveChanges();

            //Response.StatusCode = 200;
            flag = 1;
            return Json(new { ReturnFlag = flag, result = "successful deleted" });
        }

        /// <summary>
        /// 查看收藏夹内收藏文章(ID)
        /// </summary>
        /// <param name="nick_name">用户名</param>
        /// <param name="folderName">收藏夹名</param>
        /// <returns></returns>
        /// <remarks>
        ///     flag 
        ///     
        ///     0：未操作
        ///     
        ///     1：成功
        ///     
        ///         返回：{ ReturnFlag = flag, Item = item }
        ///         
        ///     2：没找到该用户
        ///     
        ///         返回：{ ReturnFlag = flag,UserName = nick_name, result = "NOT FOUND" }
        ///         
        ///     3：没找到该文件夹
        ///     
        ///         返回：{ReturnFlag = flag, FolderName = folderName, result = "NOT FOUND" }
        ///         
        /// </remarks>
        [HttpPost]
        public JsonResult getArticleListByName(string nick_name, string folderName)
        {
            var flag = 0;
            //根据用户名找到用户ID
            var userid =
                    (from c in entity.User
                     where c.NickName == nick_name
                     select c.UserId).Distinct();
            var id = userid.FirstOrDefault();
            if (id == default)
            {
                //Response.StatusCode = 405;//没找到该用户
                flag = 2;
                return Json(new { ReturnFlag = flag, UserName = nick_name, result = "NOT FOUND" });
            }

            //根据用户ID和收藏夹名找到对应收藏夹ID
            var folderid =
                    (from c in entity.Favourite
                     where (c.FavouriteName == folderName && c.UserId == id)
                     select c.FavouriteId).Distinct();
            var F_id = folderid.FirstOrDefault();
            if (F_id == default)
            {
                //Response.StatusCode = 404;//没找到该文件夹
                flag = 3;
                return Json(new { ReturnFlag = flag, FolderName = folderName, result = "NOT FOUND" });
            }

            /*var item =
                (from u in entity.FavouriteArticle
                 where u.FavouriteId == F_id
                 select u ).Distinct();*/

            var item =
                (from u in entity.FavouriteArticle
                    join mid in entity.Article on u.ArticleId equals mid.ArticleId 
                    join right in entity.User on mid.UserId equals right.UserId  //文章作者信息
                 where u.FavouriteId == F_id
                 select new { u.FavouriteId,u.ArticleId,u.FavouriteTime,right.UserId,right.NickName,mid.Title}).Distinct();

            //Response.StatusCode = 200;//成功
            flag = 1;
            return Json(new { ReturnFlag = flag, Item = item });
        }

        /// <summary>
        /// 获取用户收藏夹列表 （返回收藏夹id 收藏夹名和文章数）
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        /// <remarks>
        ///      返回：
        ///      
        ///      flag：0 未操作/出错
        ///      
        ///          返回：{ Flag = flag, errorMsg = msg }
        ///         
        ///      flag：1 成功
        ///      
        ///          返回：{ Flag = flag, List = result }
        ///          
        ///      flag：2 该用户不存在
        ///      
        ///          返回：{ Flag = flag, errorMsg = "User Not Found" }
        ///         
        /// </remarks>
        [HttpPost]
        public JsonResult getFolderList(int userID)
        {
            int flag = 0;
            string msg = "";
            try
            {
                var user = entity.User.Find(userID);
                if(user == default)
                {
                    flag = 2;//该用户不存在
                    return Json(new { Flag = flag, errorMsg = "User Not Found" });
                }
                else
                {
                    var result =
                    (from u in entity.Favourite
                     where u.UserId == userID
                     select new { u.FavouriteId, u.FavouriteName, u.ArticleNum }).Distinct();
                    //if(result == default)
                    flag = 1;//成功
                    return Json(new { Flag = flag, List = result });
                }
               
            }
            catch(Exception e)
            {
                msg = e.Message;
                flag = 0;
            }
            return Json(new { Flag = flag, errorMsg = msg });
        }

        /// <summary>
        /// 返回收藏夹内文章的具体信息【分页】
        /// </summary>
        /// <param name="folderID">收藏夹id</param>
        /// <param name="pageNum">页号</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns></returns>
        /// <remarks>
        ///      返回：
        ///      
        ///      flag：0 未操作/出错
        ///      
        ///          返回：{ Flag = flag, errorMsg = msg }
        ///         
        ///      flag：1 成功
        ///      
        ///          返回：{ Flag = flag, Result = result }
        ///          
        ///      flag：2 该用户不存在
        ///      
        ///          返回：{ Flag = flag, errorMsg = "Folder Not Found" }
        ///         
        /// </remarks>
        [HttpPost]
        public JsonResult getFolderArticleInfo(int folderID, int pageNum, int pageSize)
        {
            /*var content = (from c in entity.Topic
                           where c.UserId == int.Parse(userID)
                           select c).OrderByDescending(c => c.TopicUploadTime).Skip((pageNum - 1) * pageSize).Take(pageSize); //最新的在前面*/
            int flag = 0;
            string msg = "";
            try
            {
                var folder = entity.Favourite.Find(folderID);
                if(folder == default)
                {
                    flag = 2;
                    return Json(new { Flag = flag, errorMsg = "Folder Not Found" });
                }
                else
                {
                    var result =
                (from u in entity.FavouriteArticle
                 join mid in entity.Article on u.ArticleId equals mid.ArticleId
                 join right in entity.User on mid.UserId equals right.UserId  //文章作者信息
                 where u.FavouriteId == folderID
                 select new { u.FavouriteId, 
                     u.ArticleId,
                     u.FavouriteTime,
                     mid.Title, 
                     right.UserId, 
                     right.NickName,
                     mid.ArticleContent,
                     mid.ArticleLikes,
                     mid.CollectNum,
                     mid.ReadNum,
                     mid.ArticleUploadTime,
                     mid.ZoneId,
                       }).Distinct().OrderByDescending(right => right.ArticleUploadTime).Skip((pageNum - 1) * pageSize).Take(pageSize);
                    flag = 1;
                    return Json(new { Flag = flag, Result = result });



                   /* var result =
                (from c in entity.FavouriteArticle
                 join right in entity.Article on c.ArticleId equals right.ArticleId
                 where c.FavouriteId == folderID
                 select right).Distinct().OrderByDescending(right => right.ArticleUploadTime).Skip((pageNum - 1) * pageSize).Take(pageSize);
                    flag = 1;
                    return Json(new { Flag = flag, Result = result });*/
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
                flag = 0;
            }
            return Json(new { Flag = flag, errorMsg = msg });





        }

    }
}
