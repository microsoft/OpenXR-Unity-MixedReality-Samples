using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if URP_PRESENT
using UnityEngine.Rendering.Universal;
#endif

#if HDRP_PRESENT
using UnityEngine.Rendering.HighDefinition;
#endif

#if ENABLE_VR || ENABLE_AR
using UnityEngine.SpatialTracking;
#if XRI_PRESENT
#else

namespace UnityEditor.XR.LegacyInputHelpers
{

    internal static class MenuUtils
    {
        static readonly string kMainCamera = "MainCamera";
        static readonly Vector3 kDefaultCameraPosition = new Vector3(0.0f, 1.0f, -10.0f);
        const float kDefaultCameraNearClip = 0.01f;

        static bool CreateSimpleXRRig(Camera xrCamera, out GameObject gameObj)
        {
            var xrRigGO = ObjectFactory.CreateGameObject("XRRig");
            var cameraOffsetGO = ObjectFactory.CreateGameObject("Camera Offset");
            
            
            Undo.SetTransformParent(cameraOffsetGO.transform, xrRigGO.transform, "Parent Camera Offset to XR Rig");
            Pose camPose = new Pose();
            // we only want to move the rig to the camera position if one is passed in.
            bool camExistsAndNeedsMoving = false;
            if (xrCamera == null)
            {
                var xrCameraGO = ObjectFactory.CreateGameObject("Main Camera", typeof(Camera));
                xrCamera = xrCameraGO.GetComponent<Camera>();
            }
            else
            {               
                camPose.position = xrCamera.transform.position;
                // if initial camera position, move to the floor
                if(camPose.position == kDefaultCameraPosition)
                {
                    camPose.position.y = 0.0f;
                }
                camPose.rotation = xrCamera.transform.rotation;
                camExistsAndNeedsMoving = true;
            }
            Undo.SetTransformParent(xrCamera.transform, cameraOffsetGO.transform, "Parent Camera to Camera Offset");

            // Override the near clip to better handle controllers near the face
            xrCamera.nearClipPlane = kDefaultCameraNearClip;
            
            xrCamera.transform.localPosition = Vector3.zero;
            xrCamera.transform.localRotation = Quaternion.identity;
            xrCamera.tag = kMainCamera;

            if (camExistsAndNeedsMoving)
            {
                xrRigGO.transform.position = camPose.position;
                xrRigGO.transform.rotation = camPose.rotation;
            }

            TrackedPoseDriver trackedPoseDriver = xrCamera.gameObject.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver == null)
            {
                trackedPoseDriver = Undo.AddComponent<TrackedPoseDriver>(xrCamera.gameObject);
            }
            trackedPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);
            trackedPoseDriver.UseRelativeTransform = false;

            var coh = xrRigGO.AddComponent<CameraOffset>();
            coh.cameraFloorOffsetObject = cameraOffsetGO;

#if UNITY_2019_3_OR_NEWER
            coh.TrackingOriginMode = UnityEngine.XR.TrackingOriginModeFlags.Device;
#else
            coh.trackingSpace = UnityEngine.XR.TrackingSpaceType.Stationary;
#endif

            gameObj = xrRigGO;
            Selection.activeGameObject = xrRigGO;                

            return true;
        }

        static Camera VerifyWeCanUpgrade()
        {
            Debug.Log("Determining if we can automatically upgrade this scene to use an XR Rig");

            // rules are
            // only upgrade an empty scene with a directional light and a camera at the root node.
            var xrCameraList = Object.FindObjectsOfType<Camera>();
            Debug.Log("Checking number of cameras in the scene");
            if (xrCameraList.Length > 1)
            {
                // if the camera exists, and isn't at the root node. bail. 
                Debug.LogError("You have more than one camera in your scene. We are unable to automatically convert your scene. Please see the documentation on how to upgrade your scene.");
                return null;
            }
            else if (xrCameraList.Length == 0)
            {
                Debug.LogError("You have no cameras in your scene. We are unable to automatically convert your scene. Please see the documentation on how to upgrade your scene.");
                return null;
            }


            var xrCamera = xrCameraList.Length > 0 ? xrCameraList[0] : null;
            if (xrCamera != null)
            {
                Debug.Log("Checking Main Camera is at the root of the hierarchy");
                if (!(xrCamera.tag == kMainCamera && xrCamera.transform != null && xrCamera.transform.parent == null))
                {
                    // if the camera exists, and isn't at the root node. bail. 
                    Debug.LogError("Your Main Camera is not at the root of your hierarchy. We are unable to automatically convert your scene. Please see the documentation on how to upgrade your scene.");
                    return null;
                }

                Debug.Log("Checking camera components");
                List<Component> componentList = new List<Component>(xrCamera.gameObject.GetComponents(typeof(MonoBehaviour)));
                if (componentList.Count != 0)
                {

                    bool hdrp = false;
                    bool urp = false;
                    bool legacy = false;

#if HDRP_PRESENT
                    hdrp = true;
#endif
#if URP_PRESENT
                    urp = true;
#endif
                    legacy = !hdrp && !urp;

#pragma warning disable CS0219 // Variable is assigned but its value is never used
                    bool hdrpCameraOk = false;
                    bool urpCameraOk = false;
                    bool hdrpComponentOk = true;
                    bool urpComponentOk = true;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

                    // HDRP section.                    
#if HDRP_PRESENT
#if HDR_PRESENT_10_0
                    hdrpCameraOk = xrCamera.IsHDCamera();
                    hdrpComponentOk = (componentList.Count <= 2 && componentList.Find(x => x.GetType() == typeof(HDAdditionalCameraData)));
#elif HDRP_PRESENT_10_1_OR_NEWER
                    hdrpCameraOk = UnityEngine.Rendering.HighDefinition.ComponentUtility.IsHDCamera(xrCamera);
                    hdrpComponentOk = (componentList.Count <= 2 && componentList.Find(x => x.GetType() == typeof(HDAdditionalCameraData)));
#else
                    hdrpCameraOk = true;
                    hdrpComponentOk = (componentList.Count <= 2 && componentList.Find(x => x.GetType() == typeof(HDAdditionalCameraData)));
#endif
#endif

                    // URP Section   
#if URP_PRESENT
#if URP_PRESENT_10_0
                    urpCameraOk = xrCamera.IsUniversalCamera();
                    urpComponentOk = (componentList.Count <= 2 && componentList.Find(x => x.GetType() == typeof(UniversalAdditionalCameraData)));
#elif URP_PRESENT_10_1_OR_NEWER
                    urpCameraOk = UnityEngine.Rendering.Universal.ComponentUtility.IsUniversalCamera(xrCamera);
                    urpComponentOk = (componentList.Count <= 2 && componentList.Find(x => x.GetType() == typeof(UniversalAdditionalCameraData)));
#else
                    urpCameraOk = true;                    
                    urpComponentOk = (componentList.Count <= 2 && componentList.Find(x => x.GetType() == typeof(UniversalAdditionalCameraData)));
#endif
#endif
                    if (!((hdrp && hdrpCameraOk && hdrpComponentOk) || (urp && urpCameraOk && urpComponentOk)  || (legacy)))
                    {
                        Debug.LogError("Your Main Camera has additional components that we do not recognize. We are unable to automatically convert your scene. Please see the documentation on how to upgrade your scene.");
                        return null;
                    }

                }
                return xrCamera;
            }

            return null;
        }


        [MenuItem("GameObject/XR/Convert Main Camera To XR Rig", false, 10)]
        static void UpgradeToXRRig()
        {
            var xrCamera = VerifyWeCanUpgrade();
            if (xrCamera != null)
            {
                GameObject vrCameraRig;
                CreateSimpleXRRig(xrCamera, out vrCameraRig);
            }
        }
    }
}

#endif
#endif