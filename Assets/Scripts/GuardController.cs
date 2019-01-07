using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardController : MonoBehaviour
{
	public static event Action OnGuardSpotPlayer;

	public Transform pathHolder;
	public float speed = 5;
	public float pauseTime = 0.3f;
	public float turnSpeed = 90;
	public float timeToSpotPlayer = 0.5f;

	float playerVisibleTimer;

	public Light spotlight;
	Color originalColor;

	public float viewDistance;
	float viewAngle;

	Transform player;
	public LayerMask viewMask;

	void Start()
	{
		viewAngle = spotlight.spotAngle;
		originalColor = spotlight.color;

		Vector3[] waypoints = new Vector3[pathHolder.childCount];
		for (int i = 0; i < waypoints.Length; i++)
		{
			waypoints[i] = pathHolder.GetChild(i).position;
			waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
		}

		StartCoroutine(FollowPath(waypoints));

		player = GameObject.FindGameObjectWithTag("Player").transform;
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
		if (SpotPlayer())
		{
			playerVisibleTimer += Time.deltaTime;
		}
		else
		{
			playerVisibleTimer -= Time.deltaTime;
		}

		playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
		spotlight.color = Color.Lerp(originalColor, Color.red, playerVisibleTimer / timeToSpotPlayer);

		if (playerVisibleTimer >= timeToSpotPlayer)
		{
			OnGuardSpotPlayer?.Invoke();
		}
	}

	private bool SpotPlayer()
	{
		if (player != null)
		{
			if (Vector3.Distance(transform.position, player.position) <= viewDistance)
			{
				var playerPosition = player.position;
				playerPosition.y = 0;

				var guardPosition = transform.position;
				guardPosition.y = 0;

				var direction = (playerPosition - guardPosition).normalized;

				if (Vector3.Angle(transform.forward, direction) <= viewAngle / 2)
				{
					if (!Physics.Linecast(transform.position, player.position, viewMask))
					{
						return true;
					}
				}
			}
		}

		return false;
	}

	IEnumerator FollowPath(Vector3[] waypoints)
	{
		transform.position = waypoints[0];

		int targetWaypointIndex = 1;
		Vector3 targetWaypoint = waypoints[targetWaypointIndex];
		transform.LookAt(targetWaypoint);

		while (true)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);

			if (transform.position == targetWaypoint)
			{
				targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
				targetWaypoint = waypoints[targetWaypointIndex];

				yield return new WaitForSeconds(pauseTime);
				yield return StartCoroutine(TurnToFace(targetWaypoint));
			}
			yield return null;
		}
	}

	IEnumerator TurnToFace(Vector3 lookTarget)
	{
		Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
		float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

		while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.1f)
		{
			float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
			transform.eulerAngles = Vector3.up * angle;

			yield return null;
		}
	}

	/// <summary>
	/// Callback to draw gizmos that are pickable and always drawn.
	/// </summary>
	void OnDrawGizmos()
	{
		Vector3 startPosition = pathHolder.GetChild(0).position;
		Vector3 previousPosition = startPosition;

		foreach (Transform waypoint in pathHolder)
		{
			Gizmos.DrawSphere(waypoint.position, 0.3f);
			Gizmos.DrawLine(previousPosition, waypoint.position);
			previousPosition = waypoint.position;
		}

		Gizmos.DrawLine(previousPosition, startPosition);

		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, transform.forward * viewDistance);

		if (player != null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(transform.position, player.position);
		}
	}
}