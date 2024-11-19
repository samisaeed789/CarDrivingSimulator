using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ITS.AI;

/// <summary>
/// TS traffic volume.  This class defines volumes with specific amount of total cars allowed.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TSTrafficVolume : MonoBehaviour 
{

	/// <summary>
	/// The max allowed cars for this volume.
	/// </summary>
	public int maxAllowedCars = 10;

	/// <summary>
	/// The cars on this section.
	/// </summary>
	public List<TSTrafficAI> carsOnThisSection = new List<TSTrafficAI>();

	private Collider _collider;
	private Bounds _colliderBounds;

	private void Awake()
	{
		_collider = GetComponent<Collider>();
		_colliderBounds = _collider.bounds;
	}

	/// <summary>
	/// Raises the trigger exit event.
	/// </summary>
	/// <param name="car">Car.</param>
	void OnTriggerExit(Collider car)
	{
		if (car.gameObject.layer == LayerMask.NameToLayer("Traffic Cars"))
		{
			carsOnThisSection.Remove(car.GetComponent<TSTrafficAI>());
		}
	}

	public bool AllowedToSpawnAtPoint()
	{
		return carsOnThisSection.Count < maxAllowedCars;
	}

	public bool Contains(Vector3 point)
	{
		return _colliderBounds.Contains(point);
	}
}
