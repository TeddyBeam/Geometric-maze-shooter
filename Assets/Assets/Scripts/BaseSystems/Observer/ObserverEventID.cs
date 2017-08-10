namespace BaseSystems.Observer
{
    /// <summary>
    /// All events here can be sent via observer.
    /// </summary>
    public enum ObserverEventID
    {
        None = 0,

        #region Gameplay events
        OnGameStart,
        OnGamePause,
        OnPlayerCampingDetected,
        OnGameOver,
        #endregion

        #region Managers' events
        /// <summary>
        /// Brand new user, havent play the game yet.
        /// </summary>
        OnNewPlayer,

        /// <summary>
        /// First time user open the game again.
        /// </summary>
        OnFirstTimePlayAgain,

        /// <summary>
        /// Request data from DataTransporter.
        /// </summary>
        OnDataRequest,

        /// <summary>
        /// DataTransporter will post this with PlayerData param, after received OnDataRequest.
        /// </summary>
        OnDataLoaded,

        OnRewardVideoShowed,
        OnOpenSceneOpened,
        OnShowLeaderBoardRequest,
        OnHighestScoreChanged,
        #endregion
    }
}