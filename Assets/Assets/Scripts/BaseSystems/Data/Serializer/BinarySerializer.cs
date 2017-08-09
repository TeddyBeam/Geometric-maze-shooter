using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace BaseSystems.Data.Serializer
{
    public class BinarySerializer : ISerializer
    {
        // private const string DEFAULT_SAVE_NAME = "playerData.data";

        #region Save, Load, Delete
        /// <summary>
        /// Save all data into the persistent data path if it exists,
        /// otherwise create a new one.
        /// </summary>
        /// <typeparam name="T">Type of the database</typeparam>
        /// <param name="data">The data want to save.</param>
        /// <returns>Check if saved successfully.</returns>
        public bool TrySave<T>(string saveName, T param) where T : class
        {
            if (typeof(T).IsSerializable)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(Path.Combine(Application.persistentDataPath, saveName), FileMode.OpenOrCreate);
                try
                {
                    binaryFormatter.Serialize(fileStream, param);
                    Debug.Log("All data has been save into " + Application.persistentDataPath);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Error {0} occur when try to save into {1}", e, Application.persistentDataPath));
                    return false;
                }
                finally
                {
                    fileStream.Close();
                }
            }
            else
            {
                Debug.LogError("Error: save object should be marked as serializable first.");
                return false;
            }
        }

        /// <summary>
        /// Load all data in the persistent data path into param.
        /// </summary>
        /// <typeparam name="T">Type of the database.</typeparam>
        /// <param name="param">All the data will be loaded into this.</param>
        /// <returns>Check if the save file exist or not.</returns>
        public bool TryLoad<T>(string saveName, out T param) where T : class
        {
            string savePath = Path.Combine(Application.persistentDataPath, saveName);
            if (File.Exists(savePath))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(savePath, FileMode.Open);
                try
                {
                    T data = (T)binaryFormatter.Deserialize(fileStream);
                    Debug.Log("All data has been loaded from. " + Application.persistentDataPath);
                    param = data;
                    return true;
                }
                catch (Exception e)
                {
                    Debug.Log(string.Format("Error {0} occur when try to load from {1}", e, Application.persistentDataPath));
                    param = default(T);
                    return false;
                }
                finally
                {
                    fileStream.Close();
                }
            }
            else
            {
                Debug.LogWarning(string.Format("Load error, coundn't find {0} in {1}", saveName, Application.persistentDataPath));
                param = default(T);
                return false;
            }
        }

        public bool DeleteSaveFile(string fileName)
        {
            string savePath = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log(fileName + " has been deleted.");
                return true;
            }
            else
            {
                Debug.LogWarning(string.Format("Delete error, coundn't find {0} in {1}", fileName, Application.persistentDataPath));
                return false;
            }
        }
        #endregion
    }
}