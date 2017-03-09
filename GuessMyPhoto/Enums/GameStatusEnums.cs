using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessMyPhoto.Enums
{
    /// <summary>
    /// 100 = OK, 101 = HASH error, 102 = Unexpected DB check error, 103 = Picture error file, 104 = Picture error cutting, 105 = Name error, 106 = User not allowed, 110 = Missing parameters, 999 = Token error
    /// </summary>
    public enum GameCreateStatus
    {
        Ok,
        HashError,
        DbError,
        PictureFileError,
        PictureCuttingError,
        NameError,
        UserNotAllowed,
        MisingParameters,
        TokenError,
        PostError
    }

    /// <summary>
    /// 1 = List players in a game, 2 = List players in a challenges, 3 = List friends for challenges
    /// </summary>
    public enum PlayerListType
    {
        InGame,
        InChallenge,
        FriendsForChallenges
    }

    /// <summary>
    /// 100 = OK, 101 = HASH error, 102 = Unexpected DB check error, 103 = No players, 104 = No game, 106 = User not allowed, 110 = Missing parameters, 999 = Token error
    /// </summary>
    public enum CreateChallengeStatus
    {
        Ok,
        HashError,
        DbError,
        NoPlayers,
        NoGame,
        UserNotAllowed,
        MissingParameters,
        TokenError
    }

    /// <summary>
    /// 100 = OK
    ///101 = Hash error
    ///102 = Already played – or started
    ///103 = DB intern error
    /// </summary>
    public enum GameStartStatus
    {
        Ok,
        HashError,
        AlredyPlayedOrStarted,
        DbError,
        OtherIssues
    }

    /// <summary>
    /// 100 = OK
    ///101 = Hash error
    ///102 = Not played – or not started
    ///103 = DB intern error
    /// </summary>
    public enum GameEndStatus
    {
        Ok,
        HashError,
        NotPlayedOrNotStarted,
        DbError
    }

    /// <summary>
    /// 100 = OK
    ///101 = Hash error
    ///102 = Not played – or not started
    ///105 = Game error
    ///109 = User error
    /// </summary>
    public enum GameSetStarsStatus
    {
        Ok,
        HashError,
        NotPlayedOrNotStarted,
        GameError,
        UserError,
        OtherIssues
    }

    /// <summary>
    /// =1 misspelled word
    ///=2 bad word
    ///= 3 bad photo
    ///= 4 copyrighted content
    ///= 5 inappropriate content
    ///= 6 nude content
    ///= 7 other issues
    /// </summary>
    public enum GameReportTypes
    {
        MisspelledWord,
        BadWord,
        BadPhoto,
        CopyrightedContent,
        InappropriateContent,
        NudeContent,
        OtherIssues
    }

    /// <summary>
    /// 100 = OK
    ///101 = Hash error
    ///102 = Not played – or not started
    ///103 = Send error(error specified)
    ///104 = Report error
    ///105 = Game error
    ///109 = User error
    /// </summary>
    public enum GameReportStatus
    {
        Ok,
        HashError,
        NotPlayedOrNotStarted,
        SendError,
        ReportError,
        GameError,
        UserError,
        OtherIssues
    }

    /// <summary>
    /// 0 = Puzzle list from my friends
    ///1 = Puzzle list ALL PUZZLES
    ///2 = My puzzle list
    ///3 = My played puzzle list
    ///7 = My challenges list
    ///8 = Challenge list – with LANG
    ///9 = Challenge list ALL CHALLELLENGES
    /// </summary>
    public enum GameTypes
    {
        PuzzleListFromFriends,
        AllPuzzles,
        MyPuzzleList,
        MyPlayedPuzzleList,
        MyCallengesList,
        ChallengeListWithLang,
        ChallengeListAllCahllenges
    }

    /// <summary>
    /// GAMES.stars Desc
    ///GAMES.stars Asc
    ///GAMES.owls Desc
    ///GAMES.owls Asc
    ///GAMES.created Desc(should be default)
    ///GAMES.total_stars Desc
    /// </summary>
    public enum GameSort
    {
        StarsDesc,
        StarsAsc,
        OwlsDesc,
        OwlsAsc,
        CreatedDesc,
        TotalStarsDesc
    }
}
