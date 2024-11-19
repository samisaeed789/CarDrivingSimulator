using ITS.Utils;
using UnityEngine;

namespace ITS.AI
{
    public partial class TSTrafficAI
    {
        [Header("Throttle Module Settings")]
        public float maxDriftAngle = 3f;
        
        public class TSThrottleModule : TSAIBaseModule
        {
            private float _accelResult = 0;
            public override void OnFixedUpdate()
            {
                UpdateThrottle();
            }
            
            private void UpdateThrottle()
            {
                if (_trafficAI._brake > 0.1f)
                {
                    _trafficAI.SetThrottle(0f);
                    return;
                }
                
                var driftAngle = TSUtils.CalculateDriftAngle(_trafficAI._carSpeed);
                var allowedDriftAngle = Mathf.Abs(driftAngle) < _trafficAI.maxDriftAngle;
                var allowedMaxSpeed = _trafficAI.CarSpeed < Mathf.Min(_trafficAI._maxSpeed, _trafficAI._maxCurrentPointSpeed);
                var minimumSpeed = _trafficAI.CarSpeed < 1f;
                var shouldIncrease = allowedMaxSpeed && allowedDriftAngle || minimumSpeed;
                var deltaAccel = (shouldIncrease ? 0.02f : -1f);//FixeddeltaTime
                _accelResult = Mathf.Clamp01(_accelResult + deltaAccel);
                _trafficAI.SetThrottle(_accelResult);
            }
        }
    }
}