namespace BaseSystems.DesignPatterns.Observer
{
    /// <summary>
    /// All events here can be sent via observer.
    /// </summary>
    public enum EventsID
    {
        None = 0,
        OnGameStart,
        OnGameOver,
        OnPlayerCampingDetected,
    }
}