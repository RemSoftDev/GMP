using GuessMyPhoto.Enums;
using GuessMyPhoto.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace GuessMyPhoto.Models.Game
{
    public class Game
    {
        public static async Task<List<Puzzle>> GetGameList(string lang, GameSort sort, GameTypes type)
        {
            List<Puzzle> result = new List<Puzzle>();
            try
            {
                //salt - 03f42642c97513dc478fb6e8b4e5c723 
                //gamelist_eecefb74842cef5ca056cae740ac7fb8/ 
                string userId = AppInfo.AppUser.UserId;
                string hash = LogicHelper.CreateMD5(userId, "03f42642c97513dc478fb6e8b4e5c723");
                string sortString = "";
                switch (sort)
                {
                    case GameSort.StarsDesc:
                        sortString = "GAMES.stars Desc";
                        break;
                    case GameSort.StarsAsc:
                        sortString = "GAMES.stars Asc";
                        break;
                    case GameSort.OwlsDesc:
                        sortString = "GAMES.owls Desc";
                        break;
                    case GameSort.OwlsAsc:
                        sortString = "GAMES.owls Asc";
                        break;
                    case GameSort.CreatedDesc:
                        sortString = "GAMES.created Desc";
                        break;
                    case GameSort.TotalStarsDesc:
                        sortString = "GAMES.total_stars Desc";
                        break;
                }
                string gameType = "";
                switch (type)
                {
                    case GameTypes.PuzzleListFromFriends:
                        gameType = "0";
                        break;
                    case GameTypes.AllPuzzles:
                        gameType = "1";
                        break;
                    case GameTypes.MyPuzzleList:
                        gameType = "2";
                        break;
                    case GameTypes.MyPlayedPuzzleList:
                        gameType = "3";
                        break;
                    case GameTypes.MyCallengesList:
                        gameType = "7";
                        break;
                    case GameTypes.ChallengeListWithLang:
                        gameType = "8";
                        break;
                    case GameTypes.ChallengeListAllCahllenges:
                        gameType = "9";
                        break;
                }
                List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&uid=" + userId,
                "&from=" + lang,
                "&sort=" + sortString,
                "&gtype=" + gameType
            };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("gamelist_eecefb74842cef5ca056cae740ac7fb8/", prms);
                List<GameModel> obj = JsonConvert.DeserializeObject<List<GameModel>>(response);

                foreach (var item in obj)
                {
                    item.GameType = int.Parse(gameType);
                    result.Add(new Puzzle(item));
                }
            }
            catch (Exception)
            {
                await new MessageDialog("An error occurred retrieving puzzles. Please try again later.").ShowAsync();
            }

            return result;
        }

        public static async Task<GameCreateResponseModel> CreateGame(GameModel model)
        {
            //salt - 93bee810428b8b1ecf18ea443c91a07a
            //creategame_871c1a7c904fe292d43a666626c2b0aa/
            string hash = LogicHelper.CreateMD5(model.CreatorUid, "93bee810428b8b1ecf18ea443c91a07a");
            Dictionary<string, string> prms = new Dictionary<string, string>
            {
                {"hash", hash },
                {"uid", model.CreatorUid },
                {"gtype", model.CreateGameType },
                {"lang", model.Language },
                {"word", model.Word }
            };
            //WriteableBitmap image = await LogicHelper.ResizeWritableBitmap(model.Photo, 720, 360);
            HttpHelper helper = new HttpHelper();
            string response = await helper.PostImage("creategame_871c1a7c904fe292d43a666626c2b0aa/", prms, model.Photo);
            dynamic obj = JsonConvert.DeserializeObject(response);
            int status = obj["Respons"].First["status"];
            GameCreateResponseModel result = new GameCreateResponseModel();
            switch (status)
            {
                case 100:
                    result.Status = GameCreateStatus.Ok;
                    result.GameId = obj["Respons"].First["gid"];
                    break;
                case 101:
                    result.Status = GameCreateStatus.HashError;
                    break;
                case 102:
                    result.Status = GameCreateStatus.DbError;
                    break;
                case 103:
                    result.Status = GameCreateStatus.PictureFileError;
                    break;
                case 104:
                    result.Status = GameCreateStatus.PictureCuttingError;
                    break;
                case 105:
                    result.Status = GameCreateStatus.NameError;
                    break;
                case 106:
                    result.Status = GameCreateStatus.UserNotAllowed;
                    break;
                case 110:
                    result.Status = GameCreateStatus.MisingParameters;
                    break;
                case 999:
                    result.Status = GameCreateStatus.TokenError;
                    break;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="gid"></param>
        /// <param name="userIds">List of ids for receivers of the challenge comma separation.Mix with:
        /// gids and/or fbids “FB”+id and/or emails and/or phone-no´s (include countrycode ex. 4526366100)</param>
        /// <returns></returns>
        public static async Task<ChallengeCreateResponseModel> CreateChallenge(string gid, List<string> userIds)
        {
            //salt - 51454d19b05c53cb3551e3dc617ef4c4
            //createchallenge_9ab92ffe8a2f88cd4381570211622dfb/
            try
            {
                string uid = AppInfo.AppUser.UserId;
                string hash = LogicHelper.CreateMD5(uid, "51454d19b05c53cb3551e3dc617ef4c4");
                string ids = GetIdsString(userIds);
                List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&uid=" + uid,
                "&gid=" + gid,
                "&ids=" + ids
            };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("createchallenge_9ab92ffe8a2f88cd4381570211622dfb/", prms);
                dynamic obj = JsonConvert.DeserializeObject(response);
                int status = obj["Respons"].First["status"];
                ChallengeCreateResponseModel result = new ChallengeCreateResponseModel();
                switch (status)
                {
                    case 100:
                        result.Status = CreateChallengeStatus.Ok;
                        result.ChallengeId = obj["Respons"].First["cid"];
                        break;
                    case 101:
                        result.Status = CreateChallengeStatus.HashError;
                        break;
                    case 102:
                        result.Status = CreateChallengeStatus.DbError;
                        break;
                    case 103:
                        result.Status = CreateChallengeStatus.NoPlayers;
                        break;
                    case 104:
                        result.Status = CreateChallengeStatus.NoGame;
                        break;
                    case 106:
                        result.Status = CreateChallengeStatus.UserNotAllowed;
                        break;
                    case 110:
                        result.Status = CreateChallengeStatus.MissingParameters;
                        break;
                    case 999:
                        result.Status = CreateChallengeStatus.TokenError;
                        break;
                }

                return result;
            }
            catch
            {
                return new ChallengeCreateResponseModel { Status = CreateChallengeStatus.HashError };
            }
        }

        public static async Task<GameStartResponse> StartGame(string gameId)
        {
            //salt - 13032c9256e73dde7v4fd1eaqw12158
            //game_start_440593f67221c50b647de6734734674/
            try
            {
                string uid = AppInfo.AppUser.UserId;
                string hash = LogicHelper.CreateMD5(uid, "13032c9256e73dde7v4fd1eaqw12158");
                string gid = gameId;
                List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&uid=" + uid,
                "&gid=" + gid
            };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("game_start_440593f67221c50b647de6734734674/", prms);
                List<GameStartResponse> obj = JsonConvert.DeserializeObject<List<GameStartResponse>>(response);
                GameStartResponse result = obj.First();

                return result;
            }
            catch (Exception ex)
            {
                GameStartResponse result = new GameStartResponse();
                result.StatusCode = 123;

                return result;
            }
        }

        public static async Task<GameEndResponse> EndGame(string gameId, int score)
        {
            //salt - de86cc2bab8e407e85f67587b7511395
            //game_end_221c5656cvf56rtf67gyr45545de67340/
            try
            {
                string gid = gameId;
                string uid = AppInfo.AppUser.UserId;
                string hash = LogicHelper.CreateMD5(uid, "de86cc2bab8e407e85f67587b7511395");
                List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&uid=" + uid,
                "&gid=" + gid,
                "&score=" + score
            };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("game_end_221c5656cvf56rtf67gyr45545de67340/", prms);
                List<GameEndResponse> obj = JsonConvert.DeserializeObject<List<GameEndResponse>>(response);
                GameEndResponse result = obj.First();

                return result;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        public static async Task<GameSetStarsStatus> SetStars(string gameId, int stars)
        {
            //salt - 413a53a91318ed910b2cd26a0f2687f0
            //game_set_stars_94fc0c136ad7fd3dc652c618bda5801f/
            try
            {
                string uid = AppInfo.AppUser.UserId;
                string gid = gameId;
                string hash = LogicHelper.CreateMD5(uid, "413a53a91318ed910b2cd26a0f2687f0");
                List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&uid=" + uid,
                "&gid=" + gid,
                "&stars=" + stars.ToString()
            };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("game_set_stars_94fc0c136ad7fd3dc652c618bda5801f/", prms);
                dynamic obj = JsonConvert.DeserializeObject(response);
                int status = obj.First["status"];
                switch (status)
                {
                    case 100:
                        return GameSetStarsStatus.Ok;
                    case 101:
                        return GameSetStarsStatus.HashError;
                    case 102:
                        return GameSetStarsStatus.NotPlayedOrNotStarted;
                    case 105:
                        return GameSetStarsStatus.GameError;
                    case 109:
                        return GameSetStarsStatus.UserError;
                    default:
                        return GameSetStarsStatus.HashError;
                }
            }
            catch (Exception)
            {
                return GameSetStarsStatus.OtherIssues;
            }
        }

        public static async Task<GameReportStatus> ReportGame(string gid, GameReportTypes type)
        {
            try
            {
                //salt - b98f8d75ce206d56b4cf759bd13e3cde
                //game_report_6b761ed643a18e27c6556bf21cbc527c/
                string uid = AppInfo.AppUser.UserId;
                string hash = LogicHelper.CreateMD5(uid, "b98f8d75ce206d56b4cf759bd13e3cde");
                string rtype = "";
                switch (type)
                {
                    case GameReportTypes.MisspelledWord:
                        rtype = "1";
                        break;
                    case GameReportTypes.BadWord:
                        rtype = "2";
                        break;
                    case GameReportTypes.BadPhoto:
                        rtype = "3";
                        break;
                    case GameReportTypes.CopyrightedContent:
                        rtype = "4";
                        break;
                    case GameReportTypes.InappropriateContent:
                        rtype = "5";
                        break;
                    case GameReportTypes.NudeContent:
                        rtype = "6";
                        break;
                    case GameReportTypes.OtherIssues:
                        rtype = "7";
                        break;
                }
                List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&uid=" + uid,
                "&gid=" + gid,
                "&rtype=" + rtype
            };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("game_report_6b761ed643a18e27c6556bf21cbc527c/", prms);
                dynamic obj = JsonConvert.DeserializeObject(response);
                int status = obj.First["status"];

                switch (status)
                {
                    case 100:
                        return GameReportStatus.Ok;
                    case 101:
                        return GameReportStatus.HashError;
                    case 102:
                        return GameReportStatus.NotPlayedOrNotStarted;
                    case 103:
                        return GameReportStatus.SendError;
                    case 104:
                        return GameReportStatus.ReportError;
                    case 105:
                        return GameReportStatus.GameError;
                    case 109:
                        return GameReportStatus.UserError;
                    default:
                        return GameReportStatus.HashError;
                }
            }
            catch (Exception)
            {
                return GameReportStatus.OtherIssues;
            }
            
        }

        public static async Task<List<PlayerModel>> GetPlayersList(string lid, string uid, PlayerListType type)
        {
            try
            {
                //salt - dc3000b3719bae69a676536f6a25eac1 
                //playerlist_4ff48620c7048686919e4ca2837200d5/ 
                string hash = LogicHelper.CreateMD5(uid, "dc3000b3719bae69a676536f6a25eac1");
                string ltype = "";
                switch (type)
                {
                    case PlayerListType.InGame:
                        ltype = "1";
                        break;
                    case PlayerListType.InChallenge:
                        ltype = "2";
                        break;
                    case PlayerListType.FriendsForChallenges:
                        ltype = "3";
                        break;
                }
                List<string> prms = new List<string>
            {
                "?hash=" + hash,
                "&uid=" + uid,
                "&lid=" + lid,
                "&ltype=" + ltype
            };
                HttpHelper helper = new HttpHelper();
                string response = await helper.Get("playerlist_4ff48620c7048686919e4ca2837200d5/", prms);
                List<PlayerModel> result = JsonConvert.DeserializeObject<List<PlayerModel>>(response);

                return result;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        public async Task<List<PlayerModel>> GetHighscore(string lang, PlayerListType listType)
        {
            //highscore_f783c35d932f4d9fde4dc965d077477e/
            string ltype = "";
            switch (listType)
            {
                case PlayerListType.InGame:
                    ltype = "1";
                    break;
                case PlayerListType.InChallenge:
                    ltype = "2";
                    break;
                case PlayerListType.FriendsForChallenges:
                    ltype = "3";
                    break;
            }
            List<string> prms = new List<string>
            {
                "?Lang=" + lang,
                "&Ltype=" + ltype
            };
            HttpHelper helper = new HttpHelper();
            string response = await helper.Get("highscore_f783c35d932f4d9fde4dc965d077477e/", prms);
            List<PlayerModel> result = JsonConvert.DeserializeObject<List<PlayerModel>>(response);

            return result;
        }


        private static string GetIdsString(List<string> ids)
        {
            string result = "";
            foreach (var item in ids)
            {
                result += item + ",";
            }
            int len = result.Length;
            result = result.Substring(0, len - 1);

            return result;
        }
    }
}
