using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ITS.AI
{
    public partial class TSTrafficAI
    {
        public bool RespawnIfUpsideDown { get; private set; }
        private bool _isUpSideDown = false;
        private float _upSideDownTimer = 0f;
        
        public class TSRespawnModule : TSAIBaseModule
        {
            public override void OnFixedUpdate()
            {
                CheckUpsideDown();
            }
            
            private void CheckUpsideDown()
            {
                if (_trafficAI.RespawnIfUpsideDown == false) { return; }
                
                var weAreUpsideDown = _trafficAI._localEulerAngles.z > 60f && _trafficAI._localEulerAngles.z < 310f;
                
                if (weAreUpsideDown || _trafficAI.crashed)
                {
                    _trafficAI._upSideDownTimer += _trafficAI._deltaTime;
                    _trafficAI._isUpSideDown = true;
                    if (_trafficAI._upSideDownTimer > TSTrafficSpawner.RespawnUpSideDownTime)
                    {
                        _trafficAI.Disable();
                    }
                }
                else
                {
                    _trafficAI._upSideDownTimer = 0;
                    _trafficAI._isUpSideDown = false;
                }
            }
        }
    }
}
