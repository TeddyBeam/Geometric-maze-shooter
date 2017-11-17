using UnityEngine;
using Extension.Attributes;

namespace BaseSystems.DesignPatterns.Singleton
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        /// <summary>
        /// Is this DontDestroyOnload ?
        /// </summary>
        [Comment("Make this DontDestroyOnLoad?"), SerializeField]
        private bool isPersistant = false;

        private static volatile T instance;
        private static object synRoot = new object();

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (synRoot)
                    {
                        // instance not exist, then create new one
                        if (instance == null)
                        {
                            // create new Gameobject, and add EventDispatcher component
                            GameObject singletonObject = new GameObject(typeof(T).ToString());
                            instance = singletonObject.AddComponent<T>();
                            Debug.Log(string.Format("Created singleton : {0}", singletonObject.name));
                        }
                    }
                }
                return instance;
            }
            private set { }
        }

        protected virtual void Awake ()
        {
            if (instance != null && instance.GetInstanceID() != this.GetInstanceID())
            {
                // Destroy this instances because it already exist.
                Debug.Log(string.Format("An instance of {0} already exist : {1}, destroy this instance : {2}!!", gameObject.name
                                                                                                               , instance.GetInstanceID()
                                                                                                               , this.GetInstanceID()));
                Destroy(gameObject);
            }
            else
            {
                // Set instance.
                instance = this as T;
                if (isPersistant)
                {
                    DontDestroyOnLoad(this);
                }
            }
        }
    }
}
