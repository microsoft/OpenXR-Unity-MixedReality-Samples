using System;
using Unity.Collections;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Defines an interface for interacting with human body functionality.
    /// </summary>
    public class XRHumanBodySubsystem
        : TrackingSubsystem<XRHumanBody, XRHumanBodySubsystem, XRHumanBodySubsystemDescriptor, XRHumanBodySubsystem.Provider>
    {
        /// <summary>
        /// Whether 2D human body pose estimation is requested.
        /// </summary>
        /// <value>
        /// <c>true</c> if 2D human body pose estimation is requested. Otherwise, <c>false</c>.
        /// </value>
        public bool pose2DRequested
        {
            get => provider.pose2DRequested;
            set => provider.pose2DRequested = value;
        }

        /// <summary>
        /// Whether 2D human body pose estimation is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if 2D human body pose estimation is enabled. Otherwise, <c>false</c>.
        /// </value>
        public bool pose2DEnabled => provider.pose2DEnabled;

        /// <summary>
        /// Whether 3D human body pose estimation is requested.
        /// </summary>
        /// <value>
        /// <c>true</c> if 3D human body pose estimation is requested. Otherwise, <c>false</c>.
        /// </value>
        public bool pose3DRequested
        {
            get => provider.pose3DRequested;
            set => provider.pose3DRequested = value;
        }

        /// <summary>
        /// Whether 3D human body pose estimation is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if 3D human body pose estimation is enabled. Otherwise, <c>false</c>.
        /// </value>
        public bool pose3DEnabled => provider.pose3DEnabled;

        /// <summary>
        /// Whether 3D human body scale estimation is requested.
        /// </summary>
        /// <value>
        /// <c>true</c> if 3D human body scale estimation is requested. Otherwise, <c>false</c>.
        /// </value>
        public bool pose3DScaleEstimationRequested
        {
            get => provider.pose3DScaleEstimationRequested;
            set => provider.pose3DScaleEstimationRequested = value;
        }

        /// <summary>
        /// Whether 3D human body scale estimation is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if 3D human body scale estimation is enabled. Otherwise, <c>false</c>.
        /// </value>
        public bool pose3DScaleEstimationEnabled => provider.pose3DScaleEstimationEnabled;

        /// <summary>
        /// Construct the subsystem by creating the functionality provider.
        /// </summary>
        public XRHumanBodySubsystem() { }

        /// <summary>
        /// Query the provider for the trackable changes.
        /// </summary>
        /// <param name="allocator">The memory allocator to use for allocating the arrays.</param>
        /// <returns>
        /// The trackable human body changes.
        /// </returns>
        public override TrackableChanges<XRHumanBody> GetChanges(Allocator allocator)
            => provider.GetChanges(XRHumanBody.defaultValue, allocator);

        /// <summary>
        /// Query the provider for the skeleton joints for the requested trackable identifier.
        /// </summary>
        /// <param name="trackableId">The human body trackable identifier for which to query.</param>
        /// <param name="allocator">The memory allocator to use for the returned arrays.</param>
        /// <param name="skeleton">The array of skeleton joints to update and returns.</param>
        public void GetSkeleton(TrackableId trackableId, Allocator allocator, ref NativeArray<XRHumanBodyJoint> skeleton)
            => provider.GetSkeleton(trackableId, allocator, ref skeleton);

        /// <summary>
        /// Gets the human body pose 2D joints for the current frame.
        /// </summary>
        /// <param name="allocator">The allocator to use for the returned array memory.</param>
        /// <returns>
        /// The array of body pose 2D joints.
        /// </returns>
        /// <remarks>
        /// The returned array can be empty if the system is not enabled for human body pose 2D or if the system
        /// does not detect a human in the camera image.
        /// </remarks>
        /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support human body
        /// pose 2D.</exception>
        public NativeArray<XRHumanBodyPose2DJoint> GetHumanBodyPose2DJoints(Allocator allocator)
            => provider.GetHumanBodyPose2DJoints(default(XRHumanBodyPose2DJoint), Screen.width, Screen.height,
                                                   Screen.orientation, allocator);

        /// <summary>
        /// Register the descriptor for the human body subsystem implementation.
        /// </summary>
        /// <param name="humanBodySubsystemCinfo">The human body subsystem implementation construction information.
        /// </param>
        /// <returns>
        /// <c>true</c> if the descriptor was registered. Otherwise, <c>false</c>.
        /// </returns>
        public static bool Register(XRHumanBodySubsystemCinfo humanBodySubsystemCinfo)
        {
            XRHumanBodySubsystemDescriptor humanBodySubsystemDescriptor = XRHumanBodySubsystemDescriptor.Create(humanBodySubsystemCinfo);
            SubsystemDescriptorStore.RegisterDescriptor(humanBodySubsystemDescriptor);
            return true;
        }

        /// <summary>
        /// The provider which will service the <see cref="XRHumanBodySubsystem"/>.
        /// </summary>
        public abstract class Provider : SubsystemProvider<XRHumanBodySubsystem>
        {
            /// <summary>
            /// Property to be implemented by the provider to set whether human body pose 2D estimation is requested.
            /// </summary>
            /// <returns>
            /// <c>true</c> if human body pose 2D estimation has been requested. Otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Thrown when setting the human body pose 2D estimation to
            /// <c>true</c> if the implementation does not support human body pose 2D estimation.</exception>
            public virtual bool pose2DRequested
            {
                get => false;
                set
                {
                    if (value)
                    {
                        throw new NotSupportedException("Setting human body pose 2D estimation to enabled is not "
                                                        + "supported by this implementation");
                    }
                }
            }

            /// <summary>
            /// Property to be implemented by the provider to get whether human body pose 2D estimation is enabled.
            /// </summary>
            public virtual bool pose2DEnabled => false;

            /// <summary>
            /// Property to be implemented by the provider to set whether human body pose 3D estimation is requested.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the human body pose 3D estimation has been requested. Otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Thrown when setting the human body pose 3D estimation to
            /// <c>true</c> if the implementation does not support human body pose 3D estimation.</exception>
            public virtual bool pose3DRequested
            {
                get => false;
                set
                {
                    if (value)
                    {
                        throw new NotSupportedException("Setting human body pose 3D estimation to enabled is not "
                                                        + "supported by this implementation");
                    }
                }
            }

            /// <summary>
            /// Method to be implemented by the provider to get whether human body pose 3D estimation is enabled.
            /// </summary>
            public virtual bool pose3DEnabled => false;

            /// <summary>
            /// Property to be implemented by the provider to get or set whether 3D human body scale estimation is requested.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the 3D human body scale estimation is set to the given value. Otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Thrown when setting the 3D human body scale estimation to
            /// <c>true</c> if the implementation does not support 3D human body scale estimation.</exception>
            public virtual bool pose3DScaleEstimationRequested
            {
                get => false;
                set
                {
                    if (value)
                    {
                        throw new NotSupportedException("Setting 3D human body scale estimation to enabled is not "
                                                        + "supported by this implementation");
                    }
                }
            }

            /// <summary>
            /// Property to be implemented by the provider to get whether 3D human body scale estimation is enabled.
            /// </summary>
            public virtual bool pose3DScaleEstimationEnabled => false;

            /// <summary>
            /// Method to be implemented by the provider to query for the set of human body changes.
            /// </summary>
            /// <param name="defaultHumanBody">The default human body.</param>
            /// <param name="allocator">The memory allocator to use for the returned trackable changes.</param>
            /// <returns>
            /// The set of human body changes.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Thrown for platforms that don't support human body pose
            /// estimation.</exception>
            public abstract TrackableChanges<XRHumanBody> GetChanges(XRHumanBody defaultHumanBody, Allocator allocator);

            /// <summary>
            /// Method to be implemented by the provider to get the skeleton joints for the requested trackable identifier.
            /// </summary>
            /// <param name="trackableId">The human body trackable identifier for which to query.</param>
            /// <param name="allocator">The memory allocator to use for the returned arrays.</param>
            /// <param name="skeleton">The array of skeleton joints to update and return.</param>
            /// <exception cref="System.NotSupportedException">Thrown for platforms that don't support human body pose 3D.
            /// </exception>
            public virtual void GetSkeleton(TrackableId trackableId, Allocator allocator, ref NativeArray<XRHumanBodyJoint> skeleton)
                => throw new NotSupportedException("Skeletons are not supported by this implementation.");

            /// <summary>
            /// Method to be implemented by the provider to get the human body pose 2D joints for the current frame.
            /// </summary>
            /// <param name="defaultHumanBodyPose2DJoint">The default value for the body pose 2D joint.</param>
            /// <param name="screenWidth">The width of the screen, in pixels.</param>
            /// <param name="screenHeight">The height of the screen, in pixels.</param>
            /// <param name="screenOrientation">The orientation of the device so that the joint positions can be
            /// adjusted as required.</param>
            /// <param name="allocator">The allocator to use for the returned array memory.</param>
            /// <returns>
            /// The array of body pose 2D joints.
            /// </returns>
            /// <remarks>
            /// The returned array can be empty if the system is not enabled for human body pose 2D or if the system
            /// does not detect a human in the camera image.
            /// </remarks>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support human body
            /// pose 2D.</exception>
            public virtual NativeArray<XRHumanBodyPose2DJoint> GetHumanBodyPose2DJoints(XRHumanBodyPose2DJoint defaultHumanBodyPose2DJoint,
                                                                                        int screenWidth,
                                                                                        int screenHeight,
                                                                                        ScreenOrientation screenOrientation,
                                                                                        Allocator allocator)
                => throw new NotSupportedException("Human body pose 2D is not supported by this implementation");
        }
    }
}
