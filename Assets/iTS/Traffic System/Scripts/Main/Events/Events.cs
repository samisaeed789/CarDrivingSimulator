using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ITS.Events
{
    public class Events
    {
        public struct EventNames
        {
            public const string LastPointReleased ="OnLastPoinReleased";
            public const string UnReserveAll = "OnUnReserveAll";
        }
        
        private Dictionary<string, List<Action>> listeners = new Dictionary<string, List<Action>>(100);
        private bool isDispatching;

        public void Dispatch(string eventName)
        {
            isDispatching = true;

            if (listeners.ContainsKey(eventName) == false)
            {
                return;
            }

            var listener = listeners[eventName];

            foreach (var item in listener)
            {
                item.Invoke();
            }


            isDispatching = false;
        }

        public void Subscribe(string eventName, Action listener)
        {
            Assert.IsFalse(isDispatching);

            if (listeners.ContainsKey(eventName) == false)
            {
                listeners.Add(eventName, new List<Action>(10));
            }

            listeners[eventName].Add(listener);
        }

        public void Unsubscribe(string eventName, Action listener)
        {
            Assert.IsFalse(isDispatching);
            if (listeners.ContainsKey(eventName) == false)
            {
                return;
            }

            listeners[eventName].Remove(listener);

            if (listeners[eventName].Count == 0)
            {
                listeners.Remove(eventName);
            }
        }
    }
}