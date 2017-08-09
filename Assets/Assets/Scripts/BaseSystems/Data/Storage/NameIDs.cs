namespace BaseSystems.Data.Storage
{
    /// <summary>
    /// Use to identify objects.
    /// </summary>
    /// Used in object pooler, spawner, save data...
    public enum NameIDs
    {
        None = 0,

        #region Ships' name
        Ship01,
        Ship02,
        Ship03,
        Ship04,
        Ship05,
        Ship06,
        Ship07,
        Ship08,
        Ship09,
        Ship10,
        Ship11,
        Ship12,
        Ship13,
        Ship14,
        Ship15,
        #endregion

        #region Enemies' name
        NormalEnemy,
        TinyEnemy,
        #endregion

        #region Bullets' name
        NormalBullet
        #endregion
    }
}