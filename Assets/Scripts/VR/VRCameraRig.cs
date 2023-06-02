using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCameraRig : MonoBehaviour
{
	[Tooltip("Virtual transform corresponding to the meatspace tracking origin. Devices are tracked relative to this.")]
	public Transform trackingOriginTransform;

	[Tooltip("List of possible transforms for the head/HMD, including the no-SteamVR fallback camera.")]
	public Transform[] hmdTransforms;

	//-------------------------------------------------
	// Singleton instance of the Player. Only one can exist at a time.
	//-------------------------------------------------
	private static VRCameraRig _instance;
	public static VRCameraRig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<VRCameraRig>();
			}
			return _instance;
		}
	}

	//-------------------------------------------------
	// Get the HMD transform. This might return the fallback camera transform if SteamVR is unavailable or disabled.
	//-------------------------------------------------
	public Transform hmdTransform
	{
		get
		{
			if (hmdTransforms != null)
			{
				for (int i = 0; i < hmdTransforms.Length; i++)
				{
					if (hmdTransforms[i].gameObject.activeInHierarchy)
						return hmdTransforms[i];
				}
			}
			return null;
		}
	}

	//-------------------------------------------------
	// Guess for the world-space position of the player's feet, directly beneath the HMD.
	//-------------------------------------------------
	public Vector3 feetPositionGuess
	{
		get
		{
			Transform hmd = hmdTransform;
			if (hmd)
			{
				return trackingOriginTransform.position + Vector3.ProjectOnPlane(hmd.position - trackingOriginTransform.position, trackingOriginTransform.up);
			}
			return trackingOriginTransform.position;
		}
	}

	//-------------------------------------------------
	private void Awake()
	{
		if (trackingOriginTransform == null)
		{
			trackingOriginTransform = this.transform;
		}

#if OPENVR_XR_API && UNITY_LEGACY_INPUT_HELPERS
			if (hmdTransforms != null)
			{
				foreach (var hmd in hmdTransforms)
				{
					if (hmd.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>() == null)
						hmd.gameObject.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
				}
			}
#endif
	}
}
