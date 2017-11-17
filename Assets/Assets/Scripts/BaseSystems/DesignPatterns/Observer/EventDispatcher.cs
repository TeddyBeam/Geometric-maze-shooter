using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseSystems.DesignPatterns.Observer
{
    public class EventDispatcher : IEventDispatcher
    {
        /// <summary>
        /// Store all listeners.
        /// </summary>
        Dictionary<EventsID, Action<object>> listenersDict;

        public EventDispatcher()
        {
            listenersDict = new Dictionary<EventsID, Action<object>>();
        }

        /// <summary>
        /// Register to listen for eventID.
        /// </summary>
        /// <param name="eventID">EventID that object want to listen</param>
        /// <param name="callback">Callback will be invoked when this eventID be raised</param>
        public void RegisterListener(EventsID eventID, Action<object> callback)
        {
            // checking params
            Debug.Assert(callback != null, string.Format("AddListener, event {0}, callback is null !!", eventID.ToString()));
            Debug.Assert(eventID != EventsID.None, "RegisterListener, event = None !!");

            // check if listener exist in distionary
            if (listenersDict.ContainsKey(eventID))
            {
                // add callback to our collection
                listenersDict[eventID] += callback;
            }
            else
            {
                // add new key-value pair
                listenersDict.Add(eventID, null);
                listenersDict[eventID] += callback;
            }
        }


        /// <summary>
        /// Posts the event. This will notify all listener that has been registered for this event.
        /// </summary>
        /// <param name="eventID">Event that object want to listen.</param>
        /// <param name="param">Parameter. Listener can make a cast to get the data.</param>
		public void PostEvent(EventsID eventID, object param = null)
        {
            if (!listenersDict.ContainsKey(eventID))
            {
                Debug.Log("No listener for this event : " + eventID);
                return;
            }

            Action<object> callbacks = listenersDict[eventID];
            if (callbacks != null)
            {
                callbacks(param);
            }
            else
            {
                Debug.Log("PostEvent " + eventID + " but there is no remaining listener. Remove this key");
                listenersDict.Remove(eventID);
            }
        }

        /// <summary>
        /// Removes the listener. Use to unregister listener.
        /// </summary>
        /// <param name="eventID">Event that object want to listen.</param>
        /// <param name="callback">Callback action.</param>
		public void RemoveListener(EventsID eventID, Action<object> callback)
        {
            // checking params
            Debug.Assert(callback != null, string.Format("RemoveListener, event {0}, callback is null.", eventID.ToString()));
            Debug.Assert(eventID != EventsID.None, "AddListener, event = None.");

            if (listenersDict.ContainsKey(eventID))
            {
                listenersDict[eventID] -= callback;
            }
            else
            {
                Debug.LogWarning("RemoveListener, tried to remove nonexistent key : " + eventID);
            }
        }

        /// <summary>
        /// Clears all the listeners.
        /// </summary>
        public void ClearAllListener()
        {
            listenersDict.Clear();
        }
    }
}
