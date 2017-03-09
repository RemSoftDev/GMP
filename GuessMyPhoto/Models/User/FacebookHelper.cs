using Facebook;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace GuessMyPhoto.Models.User
{
    public class FacebookHelper
    {
        public async Task<FacebookAccount> Login()
        {

            FacebookAccount res = await ShowUserInfo(await LoginHelper());


            return res;
        }

        public async Task CreateFbPost()
        {
            try
            {
               
                //await fb.PostTaskAsync("/v2.2/me/feed/", new { name = "GuessMyPhoto", message = "Try GuessMyPhoto", caption = "http://www.guessmyphoto.net/", description = "Wait for you in GuessMyPhoto!" });
                //await fb.PostTaskAsync("/me/feed", new { name = "GuessMyPhoto", message = "Try GuessMyPhoto"});
                //await fb.PostTaskAsync("/v2.2/me/feed/", prms as object);
                //WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, new Uri("https://www.facebook.com/dialog/share?app_id=405668006193095&display=popup&href=https://www.guessmyphoto.net/&redirect_uri=https://www.guessmyphoto.net/"));
                //WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, new Uri("https://www.facebook.com/dialog/share?app_id=405668006193095&display=popup&source=https://guessmyphoto.net/images/facebook2.png"));


                //WebAuthenticationResult retult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, new Uri("https://graph.facebook.com/pageid/photos?access_token=EAACEdEose0cBAKpTvK1mDRfiZAa9rNLHZADTT5JxFUoRSZB6iSj33kGTL3kcZCXfzaC4RuAtVB9CHzNNCDdW3Qm4ZCUoNAjKn8vQZCKgxQ4DGULCvNQ3BOZCPN54sH6kxKGv19aqKz4PiWIIte8sbBSa3ZCWJvcF6LhC4XM64W4QaQZDZD&method=post&message=waterfall&source=https://guessmyphoto.net/images/facebook2.png"));

                var Uri = string.Format("https://www.facebook.com/dialog/feed?app_id=405668006193095&link=https://guessmyphoto.net/&picture=https://guessmyphoto.net/images/facebook2.png&name=GuessMyPhoto&description=I%20challenge%20you%20in%20GuessMyPhoto,%20can%20you%20beat%20me?&redirect_url={0}&display=popup", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString());
                    
                var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.UseTitle, new Uri(Uri), new Uri(WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString(), UriKind.Absolute));
                //WebAuthenticationBroker.AuthenticateAndContinue(new Uri(Uri));
              //  var fb = new FacebookClient();
              //  var logIn = fb.GetLoginUrl(
              //new
              //{
              //    client_id = 405668006193095,
              //    redirect_uri = new Uri(WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString())
              //});


              //  var logOut = fb.GetLogoutUrl(new
              //  {
              //      next = logIn,
              //      access_token = LoginHelper()
              //  });

              //   WebAuthenticationBroker.AuthenticateAndContinue(logOut);
                // return httpResponseBody;
            }
            catch (Exception ex)
            {
                SmtpHelper.email_send(string.Format("Message {0}, {1} StackTrace {2} {3} InnerException {4} {5} Data {6}", ex.Message, Environment.NewLine, ex.StackTrace, Environment.NewLine, ex.InnerException, Environment.NewLine, ex.Data));
            }

        }

        public async Task ShareOnFb()
        {
            //string token = await LoginHelper();
            //if (!string.IsNullOrEmpty(token))
            //{
            //    FacebookClient fb = new FacebookClient(token);
            //    dynamic res = await fb.PostTaskAsync("/me/feed", new { name = "GuessMyPhoto", caption = "Try GuessMyPhoto", link = "http://www.guessmyphoto.net/", description = "I just created puzzle in GuessMyPhoto!" });
            var Uri = string.Format("https://www.facebook.com/dialog/feed?app_id=405668006193095&link=https://guessmyphoto.net/&picture=https://guessmyphoto.net/images/facebook.png&name=GuessMyPhoto&description=I%20just%20created%20a%20photo,%20can%20you%20guess%20my%20photo?&redirect_url={0}&display=popup", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString());

            var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.UseTitle, new Uri(Uri), new Uri(WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString(), UriKind.Absolute));

        }

        public async Task<string> GetFbProfilePicture(string faceBookId)
        {
            WebResponse response = null;
            string pictureUrl = string.Empty;
            try
            {
                WebRequest request = WebRequest.Create(string.Format("https://graph.facebook.com/{0}/picture?type=large&w‌​idth=320&height=320", faceBookId));
                response = await request.GetResponseAsync();
                pictureUrl = response.ResponseUri.ToString();
            }
            catch (Exception ex)
            {
                SmtpHelper.email_send(string.Format("Message {0}, {1} StackTrace {2} {3} InnerException {4} {5} Data {6}", ex.Message, Environment.NewLine, ex.StackTrace, Environment.NewLine, ex.InnerException, Environment.NewLine, ex.Data));
            }
            return pictureUrl;
        }

        private async Task<string> LoginHelper()
        {
            string token = null;
            try
            {
                //string clientId = App.FbClientId;
                string scope = "email,user_about_me,public_profile";//,;
                                                                    //string redirectUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
                                                                    //var fb = new FacebookClient();
                                                                    //Uri loginUrl = fb.GetLoginUrl(new
                                                                    //{
                                                                    //    client_id = clientId,
                                                                    //    redirect_uri = redirectUri,
                                                                    //    response_type = "token",
                                                                    //    scope = scope
                                                                    //});
                                                                    //Uri startUri = loginUrl;
                                                                    ////int ind = startUri.ToString().LastIndexOf("ook.com/");
                                                                    ////string newString = startUri;
                                                                    //Uri endUri = new Uri(redirectUri, UriKind.Absolute);
                                                                    //WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
                                                                    //string token = ParseAuthenticationResult(result);

                //return token;
                string redirectUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
                var requestUri = new Uri(string.Format("https://www.facebook.com/v2.8/dialog/oauth?client_id={0}&display=popup&response_type=token&redirect_uri={1}&scope={2}", App.FbClientId, redirectUri, scope));
                WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, requestUri, new Uri(redirectUri, UriKind.Absolute));
                token = ParseAuthenticationResult(result);
            }
            catch (Exception ex)
            {
                SmtpHelper.email_send(string.Format("Message {0}, {1} StackTrace {2} {3} InnerException {4} {5} Data {6}", ex.Message, Environment.NewLine, ex.StackTrace, Environment.NewLine, ex.InnerException, Environment.NewLine, ex.Data));
            }
            return token;
        }

        private string ParseAuthenticationResult(WebAuthenticationResult result)
        {
            string token = "";
            switch (result.ResponseStatus)
            {
                case WebAuthenticationStatus.ErrorHttp:
                    //Debug.WriteLine("Error");
                    break;
                case WebAuthenticationStatus.Success:
                    var pattern = string.Format("{0}#access_token={1}&expires_in={2}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri(), "(?<access_token>.+)", "(?<expires_in>.+)");
                    var match = Regex.Match(result.ResponseData, pattern);

                    var access_token = match.Groups["access_token"];
                    var expires_in = match.Groups["expires_in"];

                    token = access_token.Value;
                    //TokenExpiry = DateTime.Now.AddSeconds(double.Parse(expires_in.Value));

                    break;
                case WebAuthenticationStatus.UserCancel:
                    //Debug.WriteLine("Operation aborted");
                    break;
                default:
                    break;
            }

            return token;
        }

        private async Task<FacebookAccount> ShowUserInfo(string accessToken)
        {
            FacebookAccount account = null;
            if (!string.IsNullOrEmpty(accessToken))
            {
                FacebookClient client = new FacebookClient(accessToken);
                //dynamic user = await client.GetTaskAsync("me");
                dynamic user = new object();
                try
                {
                     user = await client.GetTaskAsync("me/?fields=email,name");
                }
                catch (Exception ex)
                {
                    SmtpHelper.email_send(string.Format("Message {0}, {1} StackTrace {2} {3} InnerException {4} {5} Data {6}", ex.Message, Environment.NewLine, ex.StackTrace, Environment.NewLine, ex.InnerException, Environment.NewLine, ex.Data));                                                                    
                }
                //string res = user.id;
                account = new FacebookAccount
                {
                    Id = user.id,
                    Name = user.name,
                    Email = user.email

                };
            }
            return account;
        }
    }

    public class FacebookAccount
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
