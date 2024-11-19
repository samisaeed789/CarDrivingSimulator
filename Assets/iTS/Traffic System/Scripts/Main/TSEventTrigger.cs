using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ITS.AI
{
	public abstract class TSEventTrigger : MonoBehaviour
	{

		[System.Serializable]
		public class TSPointReference
		{
			/// <summary>
			/// The lane.
			/// </summary>
			public int lane;

			/// <summary>
			/// The connector.
			/// </summary>
			public int connector = -1;

			/// <summary>
			/// The point.
			/// </summary>
			public int point;
		}

		[HideInInspector] public TSPointReference startingPoint;
		[HideInInspector] public TSPointReference eventEndingPoint;
		[HideInInspector] public bool spawnCarOnStartingPoint = false;
		[HideInInspector] public List<TSPointReference> carPredefinedPath = new List<TSPointReference>();
		[HideInInspector] public TSTrafficAI tAI;
		public float range = 25f;
		#region private members

		protected bool isTriggered = false;
		protected TSMainManager manager;

		#endregion


		public virtual void Awake()
		{
			manager = GameObject.FindObjectOfType<TSMainManager>();
		}


		public abstract void InitializeMe();


		public virtual void SetCar(TSTrafficAI car)
		{
			tAI = car;
			tAI.InitializeMe();
			if (spawnCarOnStartingPoint)
			{
				tAI.reservedForEventTrigger = true;
			}

			if (carPredefinedPath != null && carPredefinedPath.Count > 0)
			{
				tAI.AddNextTrackToPath(GetPath(), startingPoint.point);
				tAI.UnPause();
			}
		}

		protected List<TSTrafficAI.TSNextLaneSelection> GetPath()
		{
			List<TSTrafficAI.TSNextLaneSelection> newPath = new List<TSTrafficAI.TSNextLaneSelection>();
			for (int i = 0; i < carPredefinedPath.Count; i++)
			{
				var definedPath = carPredefinedPath[i];
				var tsBaseInfo = definedPath.connector == -1? (TSBaseInfo)manager.lanes[definedPath.lane]: manager.lanes[definedPath.lane].connectors[definedPath.connector];
				var path = new TSTrafficAI.TSNextLaneSelection(tsBaseInfo);
				newPath.Add(path);
			}

			return newPath;
		}

		protected void DisableCarAI()
		{
			tAI.Pause();;
		}

		protected void EnableCarAI()
		{
			tAI.UnPause();
		}


		public TSPoints Point(TSPointReference point)
		{
			if (point.connector == -1)
			{
				return manager.lanes[point.lane].points[point.point];
			}
			else
			{
				return manager.lanes[point.lane].connectors[point.connector].points[point.point];
			}
		}

	}
}