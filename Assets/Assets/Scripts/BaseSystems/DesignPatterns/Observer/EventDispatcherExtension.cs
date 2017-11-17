using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseSystems.DesignPatterns.Observer
{
    public static class EventDispatcherExtension
    {
        /// <summary>
        /// Register to listen for eventID.
        /// </summary>
        /// <param name="eventID">EventID that object want to listen</param>
        /// <param name="callback">Callback will be invoked when this eventID be raised</param>
        public static void RegisterListener(this object sender, EventsID eventID, Action<object> callback)
        {
            SingletonEventDispatcher.Instance.RegisterListener(eventID, callback);
        }


        /// <summary>
        /// Posts the event. This will notify all listener that register for this event
        /// </summary>
        /// <param name="eventID">Event that object want to listen</param>
        /// <param name="param">Parameter. Listener can make a cast to get the data</param>
        public static void PostEvent(this object sender, EventsID eventID, object param)
        {
            SingletonEventDispatcher.Instance.PostEvent(eventID, param);
        }

        /// <summary>
        /// Posts the event. This will notify all listener that register for this event
        /// </summary>
        /// <param name="eventID">Event that object want to listen</param>
        public static void PostEvent(this object sender, EventsID eventID)
        {
            SingletonEventDispatcher.Instance.PostEvent(eventID, null);
        }

        /// <summary>
        /// Removes the listener. Use to unregister listener
        /// </summary>
        /// <param name="eventID">Event that object want to listen.</param>
        /// <param name="callback">Callback action.</param>
        public static void RemoveListener(this object sender, EventsID eventID, Action<object> callback)
        {
            SingletonEventDispatcher.Instance.RemoveListener(eventID, callback);
        }
    }
}
