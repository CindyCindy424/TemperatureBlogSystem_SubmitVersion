using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using Temperature.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Temperature.Controllers {
    //[Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class PhotoController : Controller {
        blogContext entity = new blogContext();
        string albumRootPath = "BlogPics/albums";

        /// <summary>
        /// 新建相簿
        /// </summary>
        /// <param name="albumIntro"></param>
        /// <param name="albumName"></param>
        /// <param name="userID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法创建，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        ///  "createAlbumFlag": 1,
        ///  "albumID": "2"
        /// }
        /// </remarks>
    [HttpPost]
        public ActionResult createAlbumByID(string albumIntro, string albumName, string userID) {
            DateTime dateTime = DateTime.Now; //创建当前时间
            int createAlbumFlag = 0;
            Album album = new Album();
            album.AlbumIntroduction = albumIntro;
            album.AlbumName = albumName;
            album.UserId = int.Parse(userID);
            album.AlbumTime = dateTime;

            try {
                entity.Album.Add(album);
                entity.SaveChanges();
                entity.Entry(album); //返回插入的记录并注入到album
                createAlbumFlag = 1;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            if(createAlbumFlag == 1) {
                string newPath = Path.Combine(albumRootPath, userID, album.AlbumId.ToString());
                if(!Directory.Exists(newPath)) { //创建相簿文件目录
                    Directory.CreateDirectory(newPath);
                }
            }

            return Json(new { createAlbumFlag = createAlbumFlag, albumID = album.AlbumId.ToString() });
        }

        /// <summary>
        /// 记录相簿访问
        /// </summary>
        /// <param name="albumID"></param>
        /// <param name="userID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法新建记录，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// "createAlbumHistoryFlag": 1
        /// 失败：
        /// "createAlbumHistoryFlag": 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult createAlbumHistoryByID(string albumID, string userID) {
            AlbumVisit albumVisit = new AlbumVisit();
            DateTime dateTime = DateTime.Now;
            int createAlbumHistoryFlag = 0;

            albumVisit.AlbumId = int.Parse(albumID);
            albumVisit.UserId = int.Parse(userID);
            albumVisit.AlbumVisitTime = dateTime;

            try {
                entity.AlbumVisit.Add(albumVisit);
                entity.SaveChanges();
                createAlbumHistoryFlag = 1;
            } catch(Exception e) {
                createAlbumHistoryFlag = 0;
            }

            return Json(new {createAlbumHistoryFlag = createAlbumHistoryFlag });

        }

        /// <summary>
        /// 上传图片到指定相册 支持多文件上传
        /// </summary>
        /// <param name="uploadedPhoto"></param>
        /// <param name="albumID"></param>
        /// <param name="userID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法新建图片，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// "Flag": 1
        ///"uploadPaths": "[\"BlogPics/albums\5\6\13302d1d-9371-40c5-8f88-5336a4706d17_background03.jpg\"]", //静态资源路径
        ///"coorespondingPhotoIDs": "[\"2\"]", //上传图片所获得的photoID 按照上传顺序排列
        ///
        /// 失败：
        /// "Flag": 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult createPhotoByID(IFormFileCollection uploadedPhoto, string albumID, string userID) {
            DateTime dateTime = DateTime.Now;
            int createPhotoFlag = 0;
            string uploadsFolder = "";
            string uniqueFileName = "";
            string filePath = "";
            string msg="";
            List<string> allFilePath = new List<string>();
            List<string> allPhotoID = new List<string>();

            if (uploadedPhoto == null) return BadRequest();

            

            for (int i = 0; i < uploadedPhoto.Count; i++) {
                try {
                    uploadsFolder = Path.Combine(albumRootPath, userID, albumID); //计算存储路径
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + uploadedPhoto[i].FileName;
                    filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    Photo photo = new Photo();
                    photo.AlbumId = int.Parse(albumID);
                    photo.PhotoLikes = 0;
                    photo.VisitNum = 0;
                    photo.PhotoAddress = filePath;
                    photo.PhotoUploadTime = dateTime;
                    photo.UserId = int.Parse(userID);

                    entity.Photo.Add(photo); //将图片信息添加到数据库中
                    entity.SaveChanges();
                    entity.Entry(photo); //获取新插入的photo的photoID

                    uploadedPhoto[i].CopyTo(new FileStream(filePath, FileMode.Create)); //存储图片到本地 持久化

                    allFilePath.Add(filePath);
                    allPhotoID.Add(photo.PhotoId.ToString());

                    createPhotoFlag = 1;
                }
                catch (Exception e) {
                    createPhotoFlag = 0;
                    msg = e.Message;
                    Console.WriteLine(e.Message);
                }
            }

            var resultJson = new {
                uploadPaths = JsonConvert.SerializeObject(allFilePath),
                coorespondingPhotoIDs = JsonConvert.SerializeObject(allPhotoID),
                Msg = msg,
                createPhotoFlag = createPhotoFlag,
            };

            return Json(resultJson);
        }

        /// <summary>
        /// 评论图片
        /// </summary>
        /// <param name="content"></param>
        /// <param name="photoID"></param>
        /// <param name="userID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法评论图片，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// "Flag": 1
        /// "photoCommentByID": "1", 、图片评论id
        /// 
        /// 失败：
        /// "Flag": 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult createPhotoCommentByID(string content, string photoID, string userID) {
            DateTime dateTime = DateTime.Now;
            int createPhotoCommentFlag = 0;

            PhotoComment photoComment = new PhotoComment();
            photoComment.PhotoCommentContent = content;
            photoComment.PhotoId = int.Parse(photoID);
            photoComment.UserId = int.Parse(userID);
            photoComment.PhotoCommentTime = dateTime;

            try {
                entity.PhotoComment.Add(photoComment);
                entity.SaveChanges();
                entity.Entry(photoComment);

                createPhotoCommentFlag = 1;
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                createPhotoCommentFlag = 0;
            }
            var returnJson = new {
                PhotoCommentByID = photoComment.PhotoCommentId.ToString(),
                createPhotoCommentFlag = createPhotoCommentFlag,
            };
            return Json(returnJson);
        }

        /// <summary>
        /// 删除相册本地内容以及数据库记录
        /// </summary>
        /// <param name="albumID"></param>
        /// <param name="userID"></param>
        /// <response code="200">成功</response>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteAlbumByID(string albumID, string userID) {
            int deleteAlbumFlag = 0;
            string deleteFilePath = "";
            string msg = "";

            try {
                deleteFilePath = Path.Combine(albumRootPath, userID, albumID);
                Directory.Delete(deleteFilePath, true); //删除本地文件夹 （删除文件不用写true）

                Album album = entity.Album.Single(c => c.AlbumId == int.Parse(albumID)); //查找
                entity.Album.Remove(album);
                entity.SaveChanges(); //提交删除

                deleteAlbumFlag = 1;
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                msg = e.Message;
                deleteAlbumFlag = 0;
            }
            return Json(new {deleteAlbumFlag = deleteAlbumFlag ,Msg = msg});
        }

        /// <summary>
        /// 删除相册访问历史记录
        /// </summary>
        /// <param name="albumID"></param>
        /// <param name="userID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法删除历史，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult deleteAlbumHistoryByID(string albumID, string userID) {
            int deleteAlbumHistoryFlag = 0;

            try {
                AlbumVisit albumVisit = entity.AlbumVisit.Single(c => c.AlbumId == int.Parse(albumID) && c.UserId == int.Parse(userID) );
                entity.AlbumVisit.Remove(albumVisit);
                entity.SaveChanges();

                deleteAlbumHistoryFlag = 1;
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                deleteAlbumHistoryFlag = 0;
            }
            return Json(new {deleteAlbumHistoryFlag = deleteAlbumHistoryFlag });
        }

        /// <summary>
        /// 删除某一张图片
        /// </summary>
        /// <param name="photoID"></param>
        /// <response code="200">成功</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult deletePhotoByID(string photoID) {
            int deletePhotoFlag = 0;
            string filePath = "";
            string msg = "";
            try {
                Photo photo = entity.Photo.Single(c => c.PhotoId == int.Parse(photoID));
                filePath = photo.PhotoAddress;
                entity.Photo.Remove(photo);
                entity.SaveChanges();

                System.IO.File.Delete(filePath); //删除单个文件

                deletePhotoFlag = 1;
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                msg = e.Message;
                deletePhotoFlag = 0;
            }

            return Json(new { deletePhotoFlag = deletePhotoFlag,Msg = msg }) ;
        }

        /// <summary>
        /// 刪除图片评论
        /// </summary>
        /// <param name="commentID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法删除，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult deletePhotoCommentByID(string commentID) {
            int deletePhotoCommentFlag = 0;

            try {
                PhotoComment photoComment = entity.PhotoComment.Single(c => c.PhotoCommentId == int.Parse(commentID));
                entity.PhotoComment.Remove(photoComment);
                entity.SaveChanges();

                deletePhotoCommentFlag = 1;
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                deletePhotoCommentFlag = 0;
            }
            return Json(new { deletePhotoCommentFlag = deletePhotoCommentFlag });
        }

        /// <summary>
        /// 获取相簿浏览历史信息
        /// </summary>
        /// <param name="albumID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法获取，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// "albumVisits": "[{\"visitor\":\"www\",\"visitTime\":\"2020/9/9 11:38:01\"}]",
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult getAlbumHistryByID(string albumID) {
            int getAlbumHistryFlag = 0;
            //IQueryable<AlbumVisit> albumVisits = null;
            List<Dictionary<string, string>> visitors = new List<Dictionary<string, string>>();
             try {
                var albumVisits = (from c in entity.AlbumVisit
                                    where c.AlbumId == int.Parse(albumID)
                                    select c);
                foreach(var visitor in albumVisits.ToList()) {
                    var visitedUser = new Dictionary<string, string>();
                    var user = entity.User.Single(c => c.UserId == visitor.UserId);
                    visitedUser.Add("visitor", user.NickName);
                    visitedUser.Add("visitTime", visitor.AlbumVisitTime.ToString());
                    visitors.Add(visitedUser);
                }

                getAlbumHistryFlag = 1;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                getAlbumHistryFlag = 0;
            }
            var returnJson = new {
                albumVisits = JsonConvert.SerializeObject(visitors),
                getAlbumHistryFlag = getAlbumHistryFlag,
            };

            return Json(returnJson);
        }




        /// <summary>
        /// 获取所有相簿
        /// </summary>
        /// <param name="userID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法获取，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// "albums": "[{\"AlbumId\":1,\"AlbumIntroduction\":\"1\",\"AlbumName\":\"22\",\"AlbumTime\":\"2020-09-07T16:12:53\",\"UserId\":3,\"User\":null,\"AlbumVisit\":[],\"Photo\":[]},
        /// {\"AlbumId\":2,\"AlbumIntroduction\":\"new album\",\"AlbumName\":\"hello album\",\"AlbumTime\":\"2020-09-09T11:36:01\",\"UserId\":3,\"User\":null,\"AlbumVisit\":[],\"Photo\":[]}]",
        /// {......}
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult getAllAlbumByPage(string userID, int pageNum, int pageSize)
        {
            int getAllAlbumFlag = 0;
            IEnumerable<object> albums = null;
            List<string> firstPhoto = new List<string>();

            try
            {
                albums = (from e in entity.Album
                          where e.UserId == int.Parse(userID)
                          orderby e.AlbumTime descending
                          select e).Skip((pageNum - 1) * pageSize).Take(pageSize); //最新的在前面

                //{
                //    albumId = e.AlbumId,
                //              albumIntroduction = e.AlbumIntroduction,
                //              albumName = e.AlbumName,
                //              albumTime = e.AlbumTime,
                //              userId = e.UserId
                //          }

                foreach (var album in albums.ToList())
                {

                    var realAlbum = (Album)album;
                    int id = realAlbum.AlbumId;

                    var photo = (from f in entity.Photo
                                 where f.AlbumId == id
                                 select f.PhotoAddress).FirstOrDefault();
                    if (photo == null) firstPhoto.Add("BlogPics\\albums\\defaultAlbumPhoto.jpg");
                    else firstPhoto.Add(photo.ToString());
                }
                getAllAlbumFlag = 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                getAllAlbumFlag = 0;
            }
            var returnJson = new
            {
                albums = JsonConvert.SerializeObject(albums),
                firstPhoto = firstPhoto,
                getAllAlbumFlag = getAllAlbumFlag,
            };

            return Json(returnJson);
        }

        /// <summary>
        /// 获取相簿内所有图片
        /// </summary>
        /// <param name="albumID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法获取，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// "photos": "[{\"PhotoId\":2,\"AlbumId\":2,\"PhotoLikes\":0,\"VisitNum\":0,\"PhotoAddress\":\"albums\3\2\413127ac-77f4-474f-8b3f-8561dc7c5d21_矢量日本特色文化集合元素.png\",\"PhotoUploadTime\":\"2020-09-09T11:41:36\",\"UserId\":3,\"Album\":null,\"User\":null,\"PhotoComment\":[],\"PhotoVisit\":[]},
        /// {......}]",
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult getAllPhotoByPage(string albumID, string userID, int pageNum, int pageSize) {
            DateTime dateTime = DateTime.Now;
            int getAllPhotoFlag = 0;
            IQueryable<Photo> photos = null;
            try {
                photos = (from c in entity.Photo
                          where c.AlbumId == int.Parse(albumID)
                          select c).OrderBy(c => c.PhotoUploadTime).Skip((pageNum - 1) * pageSize).Take(pageSize); //最新的在前面;

                AlbumVisit albumVisit = new AlbumVisit();
                albumVisit.AlbumId = int.Parse(albumID);
                albumVisit.UserId = int.Parse(userID);
                albumVisit.AlbumVisitTime = dateTime;
                entity.AlbumVisit.Add(albumVisit);
                entity.SaveChanges(); //更新访问记录

                getAllPhotoFlag = 1;
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                getAllPhotoFlag = 0;
            }
            var returnJson = new {
                photos = JsonConvert.SerializeObject(photos),
                getAllPhotoFlag = getAllPhotoFlag,
            };
            return Json(returnJson);
        }

        /// <summary>
        /// 返回图片详情
        /// </summary>
        /// <param name="photoID"></param>
        /// <returns></returns>
        /// <remarks>{
        ///  "photoDetail": {
        ///    "photoID": 16,
        ///    "albumID": 13,
        ///    "photoLikes": 16,
        ///    "visitNum": 0,
        ///    "photoUploadTime": "2020-08-31T09:58:07",
        ///    "userID": 1,
        ///    "userName": "a",
        ///    "avatar": null,
        ///    "commentNums": 3
        ///  },
        ///  "flag": 1
        ///}
        ///</remarks>
        [HttpPost]
        public JsonResult getPhotoDetail(string photoID) {
            int flag = 0;
             try {
                int count = (from c in entity.PhotoComment
                             where c.PhotoId == int.Parse(photoID)
                             select c).Count();
                var content = (from c in entity.Photo
                               join d in entity.User on c.UserId equals d.UserId
                               where c.PhotoId == int.Parse(photoID)
                               select new {
                                   photoID = c.PhotoId,
                                   albumID = c.AlbumId,
                                   photoLikes = c.PhotoLikes,
                                   visitNum = c.VisitNum,
                                   photoUploadTime = c.PhotoUploadTime,
                                   userID = c.UserId,
                                   userName = d.NickName,
                                   avatar = d.Avatr,
                                   commentNums = count
                               }).FirstOrDefault();
                flag = 1;

                return Json(new { photoDetail = content, flag = flag });


            } catch(Exception e) {
                Console.WriteLine(e.Message);
                return Json(new { flag = flag });
            }
        }

        /// <summary>
        /// 某图片的访问次数加1
        /// </summary>
        /// <param name="photoID"></param>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// "photo": {\"PhotoId\":2,\"AlbumId\":2,\"PhotoLikes\":0,\"VisitNum\":0,\"PhotoAddress\":\"albums\3\2\413127ac-77f4-474f-8b3f-8561dc7c5d21_矢量日本特色文化集合元素.png\",\"PhotoUploadTime\":\"2020-09-09T11:41:36\",\"UserId\":3,\"Album\":null,\"User\":null,\"PhotoComment\":[],\"PhotoVisit\":[]}
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpPost]
        public JsonResult increasePhotoVisit(string photoID) {
            int flag = 0;

            try {
                var photo = entity.Photo.Single(c => c.PhotoId == int.Parse(photoID) );
                photo.VisitNum += 1;
                entity.Photo.Update(photo);
                entity.SaveChanges();
                entity.Entry(photo);

                flag = 1;
                return Json(new { photo = JsonConvert.SerializeObject(photo), flag = flag });
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                flag = 0;

                return Json(new { photo = "", flag = flag });
            }
        }

        /// <summary>
        /// 获取图片评论
        /// </summary>
        /// <param name="photoID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法获取，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// "photoComments": "[{\"PhotoCommentId\":1,\"PhotoCommentContent\":\"这张图片真好看\",\"PhotoId\":2,\"UserId\":3,\"PhotoCommentTime\":\"2020-09-09T11:46:37\",\"Photo\":null,\"User\":null}]",
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpPost]
        [HttpPost]
        public ActionResult getPhotoCommentByID(string photoID) {
            int getPhotoCommentFlag = 0;
            IQueryable<Object> photoComments = null;
            try {
                photoComments = (from c in entity.PhotoComment
                                 join d in entity.User on c.UserId equals d.UserId
                                 where c.PhotoId == int.Parse(photoID)
                                 select new { 
                                    photoCommentID = c.PhotoCommentId,
                                    photoCommentContent = c.PhotoCommentContent,
                                    photoId = c.PhotoId,
                                    userId = c.UserId,
                                    photoCommentTime = c.PhotoCommentTime,
                                    userName = d.NickName,
                                    avatar = d.Avatr
                                 });

                getPhotoCommentFlag = 1;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                getPhotoCommentFlag = 0;
            }
            var returnJson = new {
                photoComments = JsonConvert.SerializeObject(photoComments),
                getPhotoCommentFlag = getPhotoCommentFlag,
            };
            return Json(returnJson);
        }

        /// <summary>
        /// 获取图片浏览历史
        /// </summary>
        /// <param name="photoID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法获取，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// "photoVisits": "[]",
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpPost]
        public ActionResult getPhotoHistoryByID(string photoID) {
            int getPhotoHistryFlag = 0;
            //IQueryable<AlbumVisit> albumVisits = null;
            List<Dictionary<string, string>> visitors = new List<Dictionary<string, string>>();
            try {
                var photoVisits = (from c in entity.PhotoVisit
                                   where c.PhotoId == int.Parse(photoID)
                                   select c);
                foreach (var visitor in photoVisits.ToList()) {
                    var visitedUser = new Dictionary<string, string>();
                    var user = entity.User.Single(c => c.UserId == visitor.UserId);
                    visitedUser.Add("visitor", user.NickName);
                    visitedUser.Add("visitTime", visitor.PhotoVisitTime.ToString());
                    visitors.Add(visitedUser);
                }

                getPhotoHistryFlag = 1;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                getPhotoHistryFlag = 0;
            }
            var returnJson = new {
                photoVisits = JsonConvert.SerializeObject(visitors),
                gePhotoHistryFlag = getPhotoHistryFlag,
            };

            return Json(returnJson);
        }

        /// <summary>
        /// 图片点赞量更新
        /// </summary>
        /// <param name="photoID"></param>
        /// <response code="200">成功</response>
        /// <response code="403">无法更新点赞，出现错误/异常</response>
        /// <returns></returns>
        /// <remarks>
        /// return {
        /// 成功：
        /// flag = 1
        /// "photoID": "2",
        /// "photoLikes": 1, //点赞数量
        /// 
        /// 失败：
        /// flag = 0
        /// }
        /// </remarks>
        [HttpGet]
        public ActionResult setPhotoLike(string photoID, string userID)
        {
            int setPhotoLikeFlag = 0;
            int likes = 0;

            try
            {
                var hasSetLike = (from c in entity.MessageLibrary
                                  where c.MessageId == int.Parse(photoID)
                                  select c).FirstOrDefault();
                if (hasSetLike != null && hasSetLike.MessageType == userID)
                    return Json(new { setPhotoLikeFlag = 1, hasSetLike = 1 });

                //新建点赞记录
                var record = new MessageLibrary();
                record.MessageId = int.Parse(photoID);
                record.MessageType = userID;
                entity.MessageLibrary.Add(record);
                entity.SaveChanges();

                var photo = entity.Photo.Single(c => c.PhotoId == int.Parse(photoID));
                photo.PhotoLikes += 1;
                likes = (int)photo.PhotoLikes;
                entity.Photo.Update(photo);
                entity.SaveChanges();

                setPhotoLikeFlag = 1;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                setPhotoLikeFlag = 0;
            }
            var returnJson = new
            {
                photoID = photoID,
                photoLikes = likes,
                setPhotoLikeFlag = setPhotoLikeFlag,
            };
            return Json(returnJson);
        }

        /// <summary>
        /// 返回用户是否点赞
        /// 1为已点赞 2为未点赞
        /// </summary>
        /// <param name="photoID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult checkPhotoLike(string photoID, string userID)
        {
            int flag = 0;

            try
            {
                var hasSetLike = entity.MessageLibrary.Single(c => c.MessageId == int.Parse(photoID));
                if (hasSetLike.MessageType == userID)
                {
                    flag = 1;
                    return Json(new { flag = flag, hasSetLike = 1 });
                }
                else
                {
                    flag = 0;
                    return Json(new { flag = flag, hasSetLike = 0 });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                flag = 0;
                return Json(new { flag = flag });
            }

        }

    }
}

public class AlbumWithPhoto
{
    int albumId;
    string albumIntroduction;
    string albumName;
    DateTime albumTime;
    int userId;
}