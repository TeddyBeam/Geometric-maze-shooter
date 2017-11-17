namespace BaseSystems.Serializer
{
    public interface ISerializeHelper
    {
        bool TrySave<T>(string saveName, T param) where T : class;
        bool TryLoad<T>(string saveName, out T param) where T : class;
        bool DeleteSaveFile(string saveName);
    }
}