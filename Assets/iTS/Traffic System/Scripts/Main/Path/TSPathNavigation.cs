using System;
using System.Collections.Generic;
using UnityEngine;

namespace ITS.AI
{
    public class TSPathNavigation
    {
        public TSPathNavigation()
        {
            _path = new List<TSTrafficAI.TSNextLaneSelection>(20);
        }

        public TSPathNavigation(TSPathNavigation original)
        {
            _path = original._path;
        }
        
        private readonly List<TSTrafficAI.TSNextLaneSelection> _path;
        private int _pathIndex = 0;
        private int _currentPointIndex = 0;
        public int CurrentPointIndex => _currentPointIndex;
        public int Count => _path.Count;
        public TSPoints CurrentWaypoint => Waypoints[_currentPointIndex];
        public TSPoints[] Waypoints => CurrentLane.Points;
        public TSBaseInfo CurrentLane { get; set; }
        //public TSBaseInfo CurrentLane => _path[Mathf.Min(_pathIndex, _path.Count-1)].NextPath;
        public TSTrafficAI.TSNextLaneSelection CurrentPath => _path[_pathIndex];
        public Action NotAbleToMoveToNextPath;
        public Action MovedToNextPath;
        public TSTrafficAI.TSNextLaneSelection this[int index] => _path[index];

        public void RemoveAt(int index)
        {
            _path.RemoveAt(index);
        }

        public void MoveToNextPoint()
        {
            _currentPointIndex++;
            CheckIfShouldSwitchTrack();
        }

        public void MoveToPreviousPoint()
        {
            _currentPointIndex++;
            if (_currentPointIndex <= 0)
            {
                _currentPointIndex = 0;
            }
        }
        
        private void CheckIfShouldSwitchTrack()
        {
            if (_currentPointIndex < Waypoints.Length || SwitchToNextLane())
            {
                return;
            }

            _currentPointIndex--;
            NotAbleToMoveToNextPath?.Invoke();
        }

        private bool SwitchToNextLane()
        {
            _pathIndex++;
            if (_pathIndex >= _path.Count || _path[_pathIndex].IsNull)
            {
                _pathIndex--;
                return false;
            }

            _currentPointIndex = 0;
            CurrentLane = _path[_pathIndex].NextPath;
            
            MovedToNextPath?.Invoke();
            return true;
        }

        public void DecreasePathIndex()
        {
            _pathIndex--;
            if (_pathIndex < 0)
            {
                _pathIndex = 0;
            }
        }

        public void Clear()
        {
            _path.Clear();
            _pathIndex = 0;
        }

        public void AddPath(List<TSTrafficAI.TSNextLaneSelection> newPath)
        {
            _path.Clear();
            _path.AddRange(newPath);
            _pathIndex = 0;
        }

        public bool TryAddPath(float distance, TSLaneInfo.VehicleType vehicleType)
        {
            while (TotalDistance < distance || _path.Count < 2)
            {
                if (_path.Count == 0)
                {
                    var nextTrack = new TSTrafficAI.TSNextLaneSelection(CurrentLane);
                    _pathIndex = 0;
                    _path.Add(nextTrack);
                }
                else if (_path[_path.Count - 1].IsNull == false)
                {
                    var nextTrack = new TSTrafficAI.TSNextLaneSelection(_path[_path.Count - 1].NextPath.GetNext(vehicleType));
                    _path.Add(nextTrack);
                    
                    if (nextTrack.IsNull)
                    {
                        return false;
                    }
                }
                else
                {
                    break;
                }
            }

            return true;
        }
        
        public float totalDistance;

        public float TotalDistance
        {
            get
            {
                totalDistance = 0f;

                if (_path.Count == 0) { return 0;}
                for (int i = 0; i < _path.Count; i++)
                {
                    if (_path[i].IsNull){continue;}
                    totalDistance += _path[i].NextPath.totalDistance;
                }

                totalDistance -= CurrentLane.totalDistance * _currentPointIndex / CurrentLane.Points.Length;

                return totalDistance;
            }
        }

        public bool NoMorePath => _pathIndex >= _path.Count || _path[_path.Count-1].IsNull;

        public bool TryGetNextPath(out TSTrafficAI.TSNextLaneSelection? tsNextLaneSelection)
        {
            tsNextLaneSelection = null;
            var index = _pathIndex + 1;
            
            if (index >= _path.Count || _path[index].IsNull) return false;
            tsNextLaneSelection = _path[index];
            return true;

        }

        public void SetCurrentPointIndex(int newCurrentPointIndex)
        {
            _currentPointIndex = newCurrentPointIndex;
        }
    }
}