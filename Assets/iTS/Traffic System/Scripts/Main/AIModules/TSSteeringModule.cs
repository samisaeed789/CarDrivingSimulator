using UnityEngine;

namespace ITS.AI
{
    public partial class TSTrafficAI
    {
        [Header("Steering Module Settings")]
        public float myLaneOffsetMin = -0.25f;
        public float myLaneOffsetMax = 0.25f;
        public float LookAheadDistance { get; private set; } = 0f;
        
        public class TSSteeringModule : TSAIBaseModule
        {
            private float MyLaneOffset { get; set; }

            public override void Initialize(TSTrafficAI trafficAI)
            {
                base.Initialize(trafficAI);
                MyLaneOffset = Random.Range(trafficAI.myLaneOffsetMin, trafficAI.myLaneOffsetMax);
            }

            public override void OnFixedUpdate()
            {
                _trafficAI._steering = GetSteer();
            }

            private float GetSteer()
            {
                var targetPoint = _trafficAI.CurrentSteerWaypoint.point + _trafficAI.PointOffset;
                var previousT = _trafficAI.PreviousSteerWaypoint.point + _trafficAI.PointOffset;
                var localTarget = GetTargetPoint(targetPoint, previousT);
                return localTarget.x / localTarget.magnitude;
            }

            private Vector2 GetTargetPoint(Vector3 point, Vector3 prev)
            {
                if (!float.IsNaN(Vector3.SqrMagnitude(prev)))
                {
                    var distanceBPoints = Vector3.Distance(point, prev);
                    var magnitude = (_trafficAI.RelativeWPosMagnitude - distanceBPoints);
                    var t = (_trafficAI.LookAheadDistance - magnitude) / distanceBPoints;
                    point = Vector3.Lerp(prev, point, Mathf.Abs(t));
                }

                var localTarget1 = _trafficAI.InverseTransformPoint(_trafficAI._wheelsCenterPosition,
                    _trafficAI._wheelsCenterRotation, Vector3.one, point);// _trafficAI.WheelsCenter.InverseTransformPoint(point);
                var x = localTarget1.x + MyLaneOffset;
                var z = Mathf.Max(localTarget1.z, 2);
                return new Vector2(x, z);
            }
        }
    }
}