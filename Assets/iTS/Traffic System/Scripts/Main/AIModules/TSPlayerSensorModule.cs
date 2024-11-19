using ITS.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace ITS.AI
{
    public partial class TSTrafficAI
    {
        [Header("Player Sensor Module Settings")]
        public float playerSensorLengthMultiplier = 1f;
        public float playerSensorWidthMultiplier = 1.05f;
        public string playerTag = "Player";
        public LayerMask playerLayer;

        [Header("Player Dectecion Reactions Settings")]
        public AudioSource playerDetectedSoundAudioSource;
        public AudioClip[] playerDetectedSoundClips = new AudioClip[0];
        public string[] playerDetectedAnimations;
        public string[] playerDetectedStates;
        public float minplayerDetectedSoundTime = 0.2f;
        public float maxplayerDetectedSoundTime = 0.5f;
        public Animator playerDetectedAnimator;
        public float minplayerDetectedAnimatorTime = 0.5f;
        public float maxplayerDetectedAnimatorTime = 0.5f;
        public Animation playerDetectedAnimationController;
        public float minplayerDetectedAnimationTime = 0.5f;
        public float maxplayerDetectedAnimationTime = 0.5f;
        public UnityEvent onDetectedPlayer = new UnityEvent();

        public class TSPlayerSensorModule : TSAIBaseModule
        {
            private int _qty;
            public float SqrDistance { get; private set; }
            private float timer = 0.2f;
            
            private Transform _transform;
            private readonly List<Collider> _players = new List<Collider>();
            
            private Collider[] results = new Collider[10];
            private float playerDetectedSoundTimming = 0f;
            private float playerDetectedSoundNext = 0f;
            private float playerDetectedAnimatorTimming = 0f;
            private float playerDetectedAnimatorNext = 0f;
            private float playerDetectedAnimationTimming = 0f;
            private float playerDetectedAnimationNext = 0f;
            private bool canPlayPlayerDetectedAudio;
            private bool canPlayPlayerDetectedAnimator;
            private bool canPlayPlayerDetectedAnimation;

            public override void Initialize(TSTrafficAI trafficAI)
            {
                base.Initialize(trafficAI);
                _players.Clear();
                _transform = trafficAI.transform;
            }

            public override void PostInitialize()
            {
                InitSoundsPlayables();
            }
            
            private void InitSoundsPlayables()
            {
                canPlayPlayerDetectedAudio = _trafficAI.playerDetectedSoundAudioSource != null && (_trafficAI.playerDetectedSoundClips.Length > 0 || _trafficAI.playerDetectedSoundAudioSource.clip != null);
                canPlayPlayerDetectedAnimator = _trafficAI.playerDetectedAnimator != null && _trafficAI.playerDetectedStates.Length > 0;
                canPlayPlayerDetectedAnimation = _trafficAI.playerDetectedAnimationController != null && _trafficAI.playerDetectedAnimations.Length > 0;
            }

            public override void OnFixedUpdateMainThread()
            {
                DetectPlayers();
                UpdateDistance();
                var lookAheadDistance = _trafficAI.MAXLookAheadDistanceFullStop * _trafficAI.playerSensorLengthMultiplier + _trafficAI.LengthMargin;
                var brake = GetBrake(lookAheadDistance);
                _trafficAI.AddExternalBrake(brake);
            }
            public override void OnFixedUpdate() { }

            private void DetectPlayers()
            {
                timer -= Time.deltaTime;
                if (timer > 0)
                {
                    return;
                }

                if (_trafficAI.ReservedPoints.Count == 0){return;}
                
                timer = 0.5f;
                Vector3? lastPoint = _trafficAI._frontPointPosition;
                _players.Clear();
                var width = _trafficAI.CarWidth * _trafficAI.playerSensorWidthMultiplier;
                
                lock (_trafficAI._reservedPointsLockObject)
                {
                    foreach (var point in _trafficAI.ReservedPoints)
                    {
                        _qty = Physics.OverlapCapsuleNonAlloc(lastPoint.Value, point.Point.point,
                            width, results, _trafficAI.playerLayer);

                        lastPoint = point.Point.point;

                        if (_qty == 0)
                        {
                            continue;
                        }

                        ProcessResults();
                    }
                }
                _trafficAI.FullStop = _players.Count > 0;
                PlayAllDetectionEvents();
            }

            private void ProcessResults()
            {
                for (var i = 0; i < _qty; i++)
                {
                    if (results[i].CompareTag(_trafficAI.playerTag) == false)
                    {
                        continue;
                    }

                    if (_players.Contains(results[i]) == false)
                    {
                        _players.Add(results[i]);
                    }
                }
            }

            private void UpdateDistance()
            {
                SqrDistance = float.MaxValue;
                var position = _trafficAI.FrontPoint.position;

                for (int i = 0; i < _players.Count; i++)
                {
                    var tempDistance = (position - _players[i].ClosestPointOnBounds(position)).sqrMagnitude;

                    if (tempDistance >= SqrDistance)
                    {
                        continue;
                    }

                    SqrDistance = tempDistance;
                }
            }

            private float GetBrake(float lookAheadDistance)
            {
                if (!_trafficAI.FullStop)
                {
                    return 0f;
                }
                return SqrDistance < lookAheadDistance.Sq() ? 1f : 0f;
            }

            #region Player Detection Events
            private void PlayAllDetectionEvents()
            {
                if (_trafficAI.FullStop == false){return;}
                
                PlayPlayerDetectedAnimation();
                PlayPlayerDetectedAnimator();
                PlayPlayerDetectedAudio();
                _trafficAI.onDetectedPlayer.Invoke();
            }

            private void PlayPlayerDetectedAudio()
            {
                if (canPlayPlayerDetectedAudio == false || Time.time - playerDetectedSoundTimming <= playerDetectedSoundNext)
                {
                    return;
                }

                playerDetectedSoundTimming = Time.time;
                playerDetectedSoundNext = Random.Range(_trafficAI.minplayerDetectedSoundTime, _trafficAI.maxplayerDetectedSoundTime);

                if (_trafficAI.playerDetectedSoundClips.Length > 0)
                {
                    var audioClip = _trafficAI.playerDetectedSoundClips[Random.Range(0, _trafficAI.playerDetectedSoundClips.Length)];
                    _trafficAI.playerDetectedSoundAudioSource.PlayOneShot(audioClip);
                    return;
                }

                if (ReferenceEquals(_trafficAI.playerDetectedSoundAudioSource.clip, null) == false)
                {
                    _trafficAI.playerDetectedSoundAudioSource.PlayOneShot(_trafficAI.playerDetectedSoundAudioSource.clip);
                }
            }

            private void PlayPlayerDetectedAnimator()
            {
                if (canPlayPlayerDetectedAnimator == false || Time.time - playerDetectedAnimatorTimming <= playerDetectedAnimatorNext) {return;}
                
                playerDetectedAnimatorNext = Random.Range(_trafficAI.minplayerDetectedAnimatorTime, _trafficAI.maxplayerDetectedAnimatorTime);
                playerDetectedAnimatorTimming = Time.time;
                _trafficAI.playerDetectedAnimator.Play(_trafficAI.playerDetectedStates[Random.Range(0, _trafficAI.playerDetectedStates.Length)]);
            }

            private void PlayPlayerDetectedAnimation()
            {
                if (canPlayPlayerDetectedAnimation == false || (Time.time - playerDetectedAnimationTimming <= playerDetectedAnimationNext)) {return;}
                
                playerDetectedAnimationNext = Random.Range(_trafficAI.minplayerDetectedAnimationTime, _trafficAI.maxplayerDetectedAnimationTime);
                playerDetectedAnimationTimming = Time.time;
                _trafficAI.playerDetectedAnimationController.Play(_trafficAI.playerDetectedAnimations[Random.Range(0, _trafficAI.playerDetectedAnimations.Length)]);
            }
            #endregion
        }
    }
}