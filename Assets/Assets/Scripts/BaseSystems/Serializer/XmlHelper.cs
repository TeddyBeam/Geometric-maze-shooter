using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace BaseSystems.Serializer
{
    public class XmlHelper : ISerializeHelper
    {
        /// <summary>
        /// Save all data into the persistent data path.
        /// </summary>
        /// <typeparam name="T">Type of the data. (Have to be ref type for now.)</typeparam>
        /// <param name="fileName">Save file name.</param>
        /// <param name="param">Object that will be saved.</param>
        /// <returns>Check if saved successfully.</returns>
        public bool TrySave<T>(string fileName, T param) where T : class
        {
            if (typeof(T).IsSerializable)
            {
                string savePath = Path.Combine(Application.persistentDataPath, fileName);
                StreamWriter writer = new StreamWriter(savePath);
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, param);
                    Debug.Log(string.Format("Saved {0} successfully into {1}.", param, savePath));
                    return true;
                }
                catch (Exception sE)
                {
                    Debug.Log(string.Format("Error when try to save {0} into {1}: {2}.",param, savePath, sE));
                    return false;
                }
                finally
                {
                    writer.Close();
                }
            }
            else
            {
                Debug.Log(param + " should be marked as serilizable first.");
                return false;
            }
        }

        /// <summary>
        /// Load saved data in the persistent data path.
        /// </summary>
        /// <typeparam name="T">Type of the data. (Have to be ref type for now.)</typeparam>
        /// <param name="fileName">Save file name.</param>
        /// <param name="param">All the data will be loaded into this.</param>
        /// <returns>Check if the save file exist or not.</returns>
        public bool TryLoad<T>(string fileName, out T param) where T : class
        {
            string savePath = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(savePath))
            {
                StreamReader fileStream = new StreamReader(savePath);
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    T data = (T)serializer.Deserialize(fileStream);
                    param = data;
                    Debug.Log(string.Format("Loaded {0} successfully from {1} into {2}.", param, savePath, param));
                    return true;
                }
                catch (XmlException)
                {
                    param = default(T);
                    Debug.Log(string.Format("Error when try to load {0} from {1}: Wrong xml format.", param, savePath));
                    return false;
                }
                catch (Exception sE)
                {
                    param = default(T);
                    Debug.Log(string.Format("Error when try to load {0} from {1}: {2}.", param, savePath, sE.Message));
                    return false;
                }
                finally
                {
                    fileStream.Close();
                }
            }
            else
            {
                param = default(T);
                Debug.Log(string.Format("Coundn't find {0} in {1}.", param, savePath));
                return false;
            }
        }

        public bool DeleteSaveFile(string fileName)
        {
            string savePath = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log(string.Format("Deleted {0} successfully in {1}.", fileName, savePath));
                return true;
            }
            else
            {
                Debug.Log(string.Format("Error when try to delete {0} in {1}.", fileName, savePath));
                return false;
            }
        }
    }
}
