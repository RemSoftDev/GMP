namespace GuessMyPhoto.Enums
{
    public enum UserDataUpdateStatus
    {
        //100 = OK 
        //101 = HASH error 
        //102 = User don’t exist 
        //103 = Email error sending 
        //104 = Email error check 
        //105 = User exists on email 
        //106 = User exists on fbid 
        //110 = Email send success
        Ok,
        HashError,
        UserDontExist,
        EmailSendingError,
        EmailCheckError,
        UserExistsOnEmail,
        UserExistsOnFbId,
        EmailSendSuccess
    }

    public enum RegisterStatus
    {
        //100 = OK 
        //101 = HASH error 
        //102 = Unexpected DB check error 
        //103 = Password error 
        //104 = Email error 
        //105 = User exists on email 
        //106 = User exists on fbid 
        //107 = Missing countrycode 
        //110 = Missing parameters 
        //999 = Token error
        Ok,
        HashError,
        DbError,
        PasswordError,
        EmailError,
        UserExistsOnEmail,
        UserExistsOnFbId,
        MissingCountrycode,
        MissingParameters,
        TokenError,
        PhotoUploadError,
        OtherIssues
    }

    public enum NativeLoginStatus
    {
        Ok,
        HashError,
        NoUser,
        LoginOrPasswordError
    }

    public enum FbLoginStatus
    {
        Ok,
        HashError,
        MissingParameters,
        DbError,
        CanceledByUser
    }

    /// <summary>
    /// 100 = OK – Email send success 
    /// 101 = HASH error 
    /// 102 = User don’t exist 
    /// 103 = Email error sending 
    /// 104 = Email error check 
    /// 105 = User exists on email 
    /// 106 = Facebook user don’t exists 
    /// 107 = No fbid 
    /// </summary>
    public enum FbDisconnectStatus
    {
        Ok,
        HashError,
        UserDontExist,
        EmailSendingError,
        EmailCheckError,
        UserExistsOnEmail,
        FacebookUserDontExists,
        NoFbId
    }

    /// <summary>
    /// 100 = OK, 101 = HASH error, 102 = User don’t exist, 103 = Unknown error, 104 = Unknown file 
    /// </summary>
    public enum UserPhotoUploadResponse
    {
        Ok,
        HashError,
        UnknownError,
        UnknownFile
    }

    /// <summary>
    /// 100 = OK, 101 = Hash error, 102 = Email error – stated in respons
    /// </summary>
    public enum UserForgotPasswordStatus
    {
        Ok,
        HashError,
        EmailError
    }
}
