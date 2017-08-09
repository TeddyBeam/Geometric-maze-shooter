using UnityEngine;
using Extension.Attributes;
using Extension.ExtraTypes;
using System;

namespace BaseSystems.Observer
{
    public class ObserverEventsHelper : MonoBehaviour
    {
        [SerializeField, Comment("All events here will be called at Start.")]
        private ObserverEventID[] startEventsCall;

        [SerializeField]
        private OberverEventButtonDictionary eventButtonsDict;

        private void Start()
        {
            foreach(ObserverEventID eventID in startEventsCall)
            {
                this.PostEvent(eventID);
            }

            foreach(ObserverEventID eventID in eventButtonsDict.Keys)
            {
                eventButtonsDict[eventID].onClick.AddListener(() => CallObserverEvent(eventID));
            }
        }

        public void CallObserverEvent (ObserverEventID eventID)
        {
            this.PostEvent(eventID);
        }
    }
}
