using GuessMyPhoto.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessMyPhoto
{
    public static class AppInfo
    {
        private static UserModel user;

        ///// <summary>
        ///// if user initialized returns user.
        ///// if user not initialized, but UserId is avilable initialize user than returns it.
        ///// Returns null when login or register is required.
        ///// </summary>
        public static UserModel AppUser
        {
            get { return user; }
            set
            {
                if (value != null && !string.IsNullOrWhiteSpace(value.Name) && !string.IsNullOrWhiteSpace(value.Email))
                    user = value;
            }
        }

        //private async static void TryGetUserData()
        //{
        //    User user = new User();
        //    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        //    var UId = localSettings.Values["UserId"];
        //    if (UId == null)
        //    {
        //        return;
        //    }
        //    await user.GetUserData(UId as string);
        //}
        public static async Task<UserModel> InitializeUser()
        {
            if (user == null)
            {
                var id = LogicHelper.GetUserIdFromStorage();
                if (!string.IsNullOrWhiteSpace(id))
                    await User.GetUserData(id);
            }
            return user;
        }
        public static void DeinitializeUser()
        {
            user = null;
        }
    }
}
