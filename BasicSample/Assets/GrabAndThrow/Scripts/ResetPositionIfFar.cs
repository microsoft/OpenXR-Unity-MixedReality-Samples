// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

public class ResetPositionIfFar : MonoBehaviour
{
	[SerializeField]
	public float MaxDistanceFromStartLocation = 5;

	private Vector3 m_initialPosition;
	private Quaternion m_initialRotation;

	void Start()
	{
		m_initialPosition = transform.position;
		m_initialRotation = transform.rotation;
	}

	void Update()
	{
		if (Vector3.Distance(m_initialPosition, transform.position) > MaxDistanceFromStartLocation)
		{
			transform.SetPositionAndRotation(m_initialPosition, m_initialRotation);
			Rigidbody rigidbody = GetComponent<Rigidbody>();
			if (rigidbody != null)
			{
				rigidbody.velocity = Vector3.zero;
				rigidbody.angularVelocity = Vector3.zero;
			}
		}
	}
}
