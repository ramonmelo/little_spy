using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float speed = 7;
	public float turnSpeed = 9;

	public float smoothMoveTime = 0.1f;

	float smoothInputMagnitude;
	float smoothInputVelocity;
	float angle;

	Vector3 velocity;
	Rigidbody rigidbody;

	bool disabled;

	void Start()
	{
		rigidbody = GetComponent<Rigidbody>();

		GuardController.OnGuardSpotPlayer += Disable;
	}

	public void Disable()
	{
		disabled = true;
	}

	void Update()
	{
		var inputDirection = Vector3.zero;

		if (!disabled)
		{
			inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
		}

		var inputMagnitude = inputDirection.magnitude;

		smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothInputVelocity, smoothMoveTime);

		var targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
		angle = Mathf.LerpAngle(angle, targetAngle, turnSpeed * Time.deltaTime * inputMagnitude);

		velocity = transform.forward * speed * smoothInputMagnitude;
	}

	void FixedUpdate()
	{
		rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * angle));
		rigidbody.MovePosition(rigidbody.position + velocity * Time.deltaTime);
	}
	
	void OnDestroy()
	{
		GuardController.OnGuardSpotPlayer -= Disable;
	}
}