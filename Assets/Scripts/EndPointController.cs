using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPointController : MonoBehaviour
{
	public static event Action OnPlayerWin;

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.CompareTag("Player"))
		{
			OnPlayerWin?.Invoke();
			
			other.gameObject.GetComponent<PlayerController>().Disable();
		}
	}
}
