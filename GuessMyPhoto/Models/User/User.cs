using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using GuessMyPhoto.Enums;
using Windows.UI.Xaml.Media.Imaging;

namespace GuessMyPhoto.Models.User
{
    class User
    {
        
      
        public void PushUserToken() //TODO: Learn how it works!
        {
            //salt - 112fffa02cad313d433d4114b39250d7
            Guid guid = Guid.NewGuid();
            string token = "";
            string hash = LogicHelper.CreateMD5(token, "112fffa02cad313d433d4114b39250d7");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user">required Email, Name, Password, CountryCode, optional PhoneNumber, FbId</param>
        /// <returns></returns>
        public static async Task<UserCreateResponse> CreateUser(UserModel user)
        {
            //salt - 19cfaaa821b2ac5fc401bfe225deb53c 
            //createuser_2c85e73bc2fa01ff29338a13caf46d88/
            try
            {
                string hash = LogicHelper.CreateMD5(user.Email, "19cfaaa821b2ac5fc401bfe225deb53c");
                List<string> prms = new List<string>()
            {
                "?hash=" + hash,
                "&email=" + user.Email,
                "&name=" + user.Name,
                "&password=" + user.Password,
                "&countrycode=" + user.CountryCode
            };
                if (user.PhoneNumber != null)
                {
                    prms.Add("&phoneno=" + user.PhoneNumber);
                }
                if (user.FbId != null)
                {
                    prms.Add("&fbid=" + user.FbId);
                }

                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("createuser_2c85e73bc2fa01ff29338a13caf46d88/", prms);
                dynamic obj = JsonConvert.DeserializeObject(response);
                UserCreateResponse result = new UserCreateResponse();
                int status = obj.First["status"];
                switch (status)
                {
                    case 100:
                        result.Status = RegisterStatus.Ok;
                        result.UserId = obj.First["uid"];
                        
                        LogicHelper.SetUserIdToStorage(result.UserId);

                        if (user.ProfilePhoto != null)
                        {
                            UserPhotoUploadResponse photoUploadResponse = await UploadProfilePhoto(user.UserId, user.ProfilePhoto);
                            //if (photoUploadResponse != UserPhotoUploadResponse.Ok)
                            //    result.Status = RegisterStatus.PhotoUploadError;
                        }
                        await GetUserData(result.UserId);
                        break;
                    case 101:
                        result.Status = RegisterStatus.HashError;
                        break;
                    case 102:
                        result.Status = RegisterStatus.DbError;
                        break;
                    case 103:
                        result.Status = RegisterStatus.PasswordError;
                        break;
                    case 104:
                        result.Status = RegisterStatus.EmailError;
                        break;
                    case 105:
                        result.Status = RegisterStatus.UserExistsOnEmail;
                        break;
                    case 106:
                        result.Status = RegisterStatus.UserExistsOnFbId;
                        break;
                    case 107:
                        result.Status = RegisterStatus.MissingCountrycode;
                        break;
                    case 110:
                        result.Status = RegisterStatus.MissingParameters;
                        break;
                    case 999:
                        result.Status = RegisterStatus.TokenError;
                        break;
                }

                return result;
            }
            catch
            {
                UserCreateResponse result = new UserCreateResponse();
                result.Status = RegisterStatus.OtherIssues;
                return result;
            }
        }

        public static async Task<UserPhotoUploadResponse> UploadProfilePhoto(string uid, WriteableBitmap image)
        {
            //salt - a04f16bad1b64acfeb7fd4b84b217c91
            //profilephoto_dbea40501e2ab81fa435e3b911d47f55/
            
            string hash = LogicHelper.CreateMD5(uid, "a04f16bad1b64acfeb7fd4b84b217c91");
            Dictionary<string, string> prms = new Dictionary<string, string>
            {
                {"Hash", hash },
                {"Uid", uid }
            };
            HttpHelper helper = new HttpHelper();
            string response = await helper.PostImage("profilephoto_dbea40501e2ab81fa435e3b911d47f55/", prms, image);
            dynamic obj = JsonConvert.DeserializeObject(response);

            int status = obj.First["status"];
            switch (status)
            {
                case 100:
                    return UserPhotoUploadResponse.Ok;
                case 101:
                    return UserPhotoUploadResponse.HashError;
                case 102:
                    return UserPhotoUploadResponse.UnknownError;
                case 103:
                    return UserPhotoUploadResponse.UnknownFile;
                default:
                    return UserPhotoUploadResponse.UnknownError;
            }
        }

        public static async Task<UserNativeLoginResponse> LoginNative(string email, string password)
        {
            //salt - ff9aa0bbc598d89cffb1e18edf2316d2 
            //login_native_98c6c5f42b0e82238a5d7d9972800cff
            try
            {
                string hash = LogicHelper.CreateMD5(email, "ff9aa0bbc598d89cffb1e18edf2316d2");
                List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&email=" + email,
                "&password=" + password
            };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("login_native_98c6c5f42b0e82238a5d7d9972800cff/", prms);

                dynamic obj = JsonConvert.DeserializeObject(response);
                UserNativeLoginResponse result = new UserNativeLoginResponse
                {
                    UserId = obj.First["uid"],
                    UserName = obj.First["uname"],
                    FacebookId = obj.First["fbid"]
                };
                int status = obj.First["status"];
                switch (status)
                {
                    case 100:
                        {
                            result.Status = NativeLoginStatus.Ok;
                            var localSettings = ApplicationData.Current.LocalSettings;
                            localSettings.Values["UserId"] = result.UserId;
                            AppInfo.AppUser = new UserModel
                            {
                                UserId = result.UserId,
                                Name = result.UserName,
                                Email = email,
                                FbId = result.FacebookId
                            };
                            break;
                        }
                    case 101:
                        {
                            result.Status = NativeLoginStatus.HashError;
                            break;
                        }
                    case 102:
                        {
                            result.Status = NativeLoginStatus.NoUser;
                            break;
                        }
                    case 103:
                        {
                            result.Status = NativeLoginStatus.LoginOrPasswordError;
                            break;
                        }
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<UserForgotPasswordResponse> ForgotPassword(string email)
        {
            //salt - 2f57af38610488bc8c686b6fcc133fb2 
            //sendpassword_4ffa763746353648ced17fdecba6324f/
            try
            {
                string hash = LogicHelper.CreateMD5(email, "2f57af38610488bc8c686b6fcc133fb2");
                List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&email=" + email,
                "&app=json"
            };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("sendpassword_4ffa763746353648ced17fdecba6324f/", prms);
                dynamic obj = JsonConvert.DeserializeObject(response);
                int status = obj["Respons"].First["status"];
                UserForgotPasswordResponse result = new UserForgotPasswordResponse();
                switch (status)
                {
                    case 100:
                        result.Status = UserForgotPasswordStatus.Ok;
                        result.Response = obj["Respons"].First["respons"];
                        break;
                    case 101:
                        result.Status = UserForgotPasswordStatus.HashError;
                        break;
                    case 102:
                        result.Status = UserForgotPasswordStatus.EmailError;
                        result.Response = obj["Respons"].First["respons"];
                        break;
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
            
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user">required UserId, Name, Email, CountryCode, PhoneNumber</param>
        /// <returns></returns>
        public static async Task<UserDataUpdateResponse> DataUpdate(UserModel user)
        {
            //salt - d60e82ae4324f250a260a2839cf88498
            //userdata_update_28356a03ad787901d008a1bd92bec877/
            try
            {
                string hash = LogicHelper.CreateMD5(user.UserId, "d60e82ae4324f250a260a2839cf88498");
                HttpHelper helper = new HttpHelper();
                List<string> prms = new List<string>
                {
                    "?hash=" + hash,
                    "&uid=" + user.UserId,
                    "&name=" + user.Name,
                    "&email=" + user.Email,
                    "&countrycode=" + user.CountryCode,
                    "&phoneno=" + user.PhoneNumber
                };
                if (user.FbId != null)
                {
                    prms.Add("&fbid=" + user.FbId);
                }
                string response = await helper.Get("userdata_update_28356a03ad787901d008a1bd92bec877/", prms);
                dynamic obj = JsonConvert.DeserializeObject(response);
                int status = obj.First["status"];
                UserDataUpdateResponse result = new UserDataUpdateResponse();
                switch (status)
                {
                    //100 = OK 
                    //101 = HASH error 
                    //102 = User don’t exist 
                    //103 = Email error sending 
                    //104 = Email error check 
                    //105 = User exists on email 
                    //106 = User exists on fbid 
                    //110 = Email send success
                    case 100:
                        result.Status = UserDataUpdateStatus.Ok;
                        result.Response = obj.First["respons"];
                        GetUserData(user.UserId);
                        break;
                    case 101:
                        result.Status = UserDataUpdateStatus.HashError;
                        break;
                    case 102:
                        result.Status = UserDataUpdateStatus.UserDontExist;
                        break;
                    case 103:
                        result.Status = UserDataUpdateStatus.EmailSendingError;
                        break;
                    case 104:
                        result.Status = UserDataUpdateStatus.EmailCheckError;
                        break;
                    case 105:
                        result.Status = UserDataUpdateStatus.UserExistsOnEmail;
                        break;
                    case 106:
                        result.Status = UserDataUpdateStatus.UserExistsOnFbId;
                        break;
                    case 110:
                        result.Status = UserDataUpdateStatus.EmailSendSuccess;
                        break;
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        public static async Task GetUserData(string userId)
        {
            //salt - 03f42642c97513dc478fb6e8b4e5c723
            //userdata_56f454036a322113c4f1fab5f98ad79d/
            try
            {
                string hash = LogicHelper.CreateMD5(userId, "03f42642c97513dc478fb6e8b4e5c723");
                List<string> prms = new List<string>
                {
                    "?hash=" + hash,
                    "&uid=" + userId
                };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("userdata_56f454036a322113c4f1fab5f98ad79d/", prms);
                dynamic obj = JsonConvert.DeserializeObject(response);
                UserModel result = new UserModel()
                {
                    UserId = obj["Respons"].First["uid"],
                    FbId = obj["Respons"].First["fbid"],
                    Name = obj["Respons"].First["uname"],
                    Email = obj["Respons"].First["email"],
                    PhoneNumber = obj["Respons"].First["phone"],
                    CountryCode = obj["Respons"].First["countrycode"],
                    TotalPlayedPuzzles = obj["Respons"].First["total_played_puzzles"],
                    TotalCreatedPuzzles = obj["Respons"].First["total_created_puzzles"],
                    TotalScore = obj["Respons"].First["total_score"],
                    TotalRating = obj["Respons"].First["total_rating"],
                };
                //result.UserId = obj.First.First.First.First.First;
                AppInfo.AppUser = result;

                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static async Task<UserFbLoginResponse> LoginFb(string friendsIds = null)
        {
            //salt - 29596333de6cb130c92e73de74f1a158 
            //login_facebook_67e82d4a63d7aa64440593f67221c50b/
            try
            {
                FacebookHelper fbhelper = new FacebookHelper();
                FacebookAccount account = await fbhelper.Login();
                UserFbLoginResponse result = null;
                if (account != null)
                {
                    string hash = LogicHelper.CreateMD5(account.Id, "29596333de6cb130c92e73de74f1a158");
                    List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&fbid=" + account.Id,
                "&name=" + account.Name,
                "&email=" + account.Email
            };
                    if (friendsIds != null)
                    {
                        prms.Add("&fbids=" + friendsIds);
                    }
                    HttpHelper helper = new HttpHelper();
                    string response = await helper.Get("login_facebook_67e82d4a63d7aa64440593f67221c50b/", prms);
                    dynamic obj = JsonConvert.DeserializeObject(response);
                    result = new UserFbLoginResponse
                    {
                        UserId = obj.First["uid"],
                        FbId = obj.First["fbid"]
                    };
                    int status = obj.First["status"];
                    switch (status)
                    {
                        case 100:
                            {
                                result.Status = FbLoginStatus.Ok;
                                LogicHelper.SetUserIdToStorage(result.UserId);
                                await GetUserData(result.UserId);
                                break;
                            }
                        case 101:
                            {
                                result.Status = FbLoginStatus.HashError;
                                break;
                            }
                        case 102:
                            {
                                result.Status = FbLoginStatus.MissingParameters;
                                break;
                            }
                        case 103:
                            {
                                result.Status = FbLoginStatus.DbError;
                                break;
                            }
                    }
                }

                return result;
            }
            catch
            {
                var result = new UserFbLoginResponse
                {
                    Status = FbLoginStatus.CanceledByUser
                };
                return result;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user">required UserId, Email, FbId</param>
        /// <returns></returns>
        public static async Task<UserDisconnectFbResponse> DisconnectFb(UserModel user)
        {
            //salt - d60e82ae4324f250a260a2839cf88498 
            //disconnect_facebook_fe839ba98fa33d6bdc6fb90951c531e0/
            try
            {
                string hash = LogicHelper.CreateMD5(user.UserId, "d60e82ae4324f250a260a2839cf88498");
                List<string> prms = new List<string>
                {
                    "?hash=" + hash,
                    "&uid=" + user.UserId,
                    "&email=" + user.Email,
                    "&fbid=" + user.FbId
                };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("disconnect_facebook_fe839ba98fa33d6bdc6fb90951c531e0/", prms);
                dynamic obj = JsonConvert.DeserializeObject(response);
                int status = obj.First["status"];
                UserDisconnectFbResponse result = new UserDisconnectFbResponse();
                switch (status)
                {
                    case 100:
                        result.Status = FbDisconnectStatus.Ok;
                        AppInfo.AppUser.FbId = null;
                        result.Response = obj.First["respons"];
                        break;
                    case 101:
                        result.Status = FbDisconnectStatus.HashError;
                        break;
                    case 102:
                        result.Status = FbDisconnectStatus.UserDontExist;
                        break;
                    case 103:
                        result.Status = FbDisconnectStatus.EmailSendingError;
                        break;
                    case 104:
                        result.Status = FbDisconnectStatus.EmailCheckError;
                        break;
                    case 105:
                        result.Status = FbDisconnectStatus.UserExistsOnEmail;
                        break;
                    case 106:
                        result.Status = FbDisconnectStatus.FacebookUserDontExists;
                        break;
                    case 107:
                        result.Status = FbDisconnectStatus.NoFbId;
                        break;
                }

                return result;
            }
            catch
            {
                return null;
            }
        }


    }
}
