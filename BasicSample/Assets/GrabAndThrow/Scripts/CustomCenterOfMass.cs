using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
	[RequireComponent(typeof(Rigidbody))]
	/// <summary>
	/// Defines a custom center of mass for a given GameObject's Rigidbody
	/// </summary>
	public class CustomCenterOfMass : MonoBehaviour
	{
		[SerializeField]
		private Vector3 centerOfMass = Vector3.zero;

		void Start() => GetComponent<Rigidbody>().centerOfMass = centerOfMass;
	}
}
