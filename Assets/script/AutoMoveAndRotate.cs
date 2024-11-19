using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class AutoMoveAndRotate : MonoBehaviour
    {
        public List<Transform> wheels; // List to hold all the wheel transforms
        public Vector3andSpace moveUnitsPerSecond;
        public Vector3andSpace rotateDegreesPerSecond;
        public bool ignoreTimescale;
        private float m_LastRealTime;

        private void Start()
        {
            m_LastRealTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (ignoreTimescale)
            {
                deltaTime = (Time.realtimeSinceStartup - m_LastRealTime);
                m_LastRealTime = Time.realtimeSinceStartup;
            }

            // Rotate each wheel in the list
            foreach (var wheel in wheels)
            {
                if (wheel != null)
                {
                    wheel.Rotate(rotateDegreesPerSecond.value * deltaTime, rotateDegreesPerSecond.space);
                }
            }

            // Optionally, move the main object
            transform.Translate(moveUnitsPerSecond.value * deltaTime, moveUnitsPerSecond.space);
        }

        [Serializable]
        public class Vector3andSpace
        {
            public Vector3 value;
            public Space space = Space.Self;
        }
    }
}
