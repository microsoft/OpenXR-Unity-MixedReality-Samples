// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
	[RequireComponent(typeof(Rigidbody))]
	/// <summary>
	/// GameObjects with this component are considered valid for grab / throw interactions by GrabAndThrowManager.cs
	/// </summary>
	public class ThrowableObject : MonoBehaviour
	{
		[SerializeField]
		public Collider[] colliders;
	}
}