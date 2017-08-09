using UnityEngine;
using Extension.Attributes;

namespace BaseSystems.Singleton
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        #region Init
        /// <summary>
        /// Is this DontDestroyOnload ?
        /// </summary>
        [Comment("Make this DontDestroyOnLoad?"), SerializeField]
        private bool isPersistant = false;

        /// <summary>
        /// Just to prevent duplicate when using DontDestroyOnLoad, dont use it to do anything else.
        /// </summary>
        public static T Instance { get; private set; }
        #endregion

        #region Monobehaviours
        protected virtual void Awake ()
        {
            if (Instance != null && Instance.GetInstanceID() != this.GetInstanceID())
            {
                // Destroy this instances because it already exist.
                Debug.Log(string.Format("An instance of {0} already exist : {1}, destroy this instance : {2}!!", gameObject.name
                                                                                                               , Instance.GetInstanceID()
                                                                                                               , this.GetInstanceID()));
                Destroy(gameObject);
            }
            else
            {
                // Set instance.
                Instance = this as T;
                if (isPersistant)
                {
                    DontDestroyOnLoad(this);
                }
            }
        }
        #endregion
    }
}
