namespace BaseSystems.Data.Serializer
{
    public interface ISerializer
    {
        bool TrySave<T>(string saveName, T param) where T : class;
        bool TryLoad<T>(string saveName, out T param) where T : class;
        bool DeleteSaveFile(string saveName);
    }
}