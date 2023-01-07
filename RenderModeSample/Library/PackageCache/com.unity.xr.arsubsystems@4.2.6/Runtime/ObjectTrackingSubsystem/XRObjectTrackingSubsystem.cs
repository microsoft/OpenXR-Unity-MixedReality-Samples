using System;
using Unity.Collections;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Base class for object tracking subsystems.
    /// </summary>
    /// <remarks>
    /// This subsystem allows real objects to be recognized in the environment.
    /// You must first specify a library of "reference objects" to search for.
    /// These reference objects are typically in a format specific to a particular
    /// implementation; see the documentation for the implementing subsystem for
    /// further instructions.
    /// </remarks>
    public class XRObjectTrackingSubsystem
        : TrackingSubsystem<XRTrackedObject, XRObjectTrackingSubsystem, XRObjectTrackingSubsystemDescriptor, XRObjectTrackingSubsystem.Provider>
    {
        /// <summary>
        /// For AR implementors: implement this class to provide support for object tracking.
        /// </summary>
        public abstract class Provider : SubsystemProvider<XRObjectTrackingSubsystem>
        {
            /// <summary>
            /// Get the changes to <see cref="XRTrackedObject"/>s (added, updated, and removed) 
            /// since the last call to this method. This is typically invoked once per frame.
            /// </summary>
            /// <param name="template">A 'template' <see cref="XRTrackedObject"/>. <see cref="XRTrackedObject"/>
            /// might have fields added to it in the future; this template allows you to fill
            /// the arrays of added, updated, and removed with default values before copying in
            /// data from your own memory buffer.</param>
            /// <param name="allocator">The allocator to use for the added, updated, and removed arrays.</param>
            /// <returns>A new <see cref="TrackableChanges{T}"/> containing the changes since the last
            /// call to this method, allocated with <paramref name="allocator"/>.</returns>
            public abstract TrackableChanges<XRTrackedObject> GetChanges(XRTrackedObject template, Allocator allocator);

            /// <summary>
            /// The library that contains the reference objects for which to scan.
            /// If this is not <c>null</c>, the provider should begin scanning for the
            /// objects in the library. If <c>null</c>, the provider should stop
            /// scanning for objects.
            /// </summary>
            public virtual XRReferenceObjectLibrary library
            {
                set {}
            }
        }

        /// <summary>
        /// Constructs an object tracking subsystem. Do not invoked directly; call <c>Create</c> on the <see cref="XRObjectTrackingSubsystemDescriptor"/> instead.
        /// </summary>
        public XRObjectTrackingSubsystem() { }

        /// <summary>
        /// Starts scanning for the reference objects in <see cref="library"/>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if <see cref="library"/> is <c>null</c>.</exception>
        protected sealed override void OnStart()
        {
            if (m_Library == null)
                throw new InvalidOperationException("Cannot start object tracking without an object library.");

            provider.library = m_Library;

            base.OnStart();
        }

        /// <summary>
        /// The library of reference objects for which to scan. This must be set to non-null
        /// before calling <see cref="OnStart"/>.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if you set the library to <c>null</c> while the subsystem is running.</exception>
        public XRReferenceObjectLibrary library
        {
            get => m_Library;
            set
            {
                if (m_Library == value)
                    return;

                if (running && value == null)
                    throw new ArgumentNullException("Cannot set library to null while subsystem is running.");

                m_Library = value;

                // If we are running, then we want to switch the current library
                if (running)
                    provider.library = m_Library;
            }
        }

        /// <summary>
        /// Stops scanning for objects.
        /// </summary>
        protected override sealed void OnStop()
        {
            provider.library = null;

            base.OnStop();
        }

        /// <summary>
        /// Get changes to <see cref="XRTrackedObject"/>s (added, updated, and removed) since
        /// the last call to this method. The caller owns the memory allocated with <paramref name="allocator"/>.
        /// </summary>
        /// <param name="allocator">The allocator to use for the returned arrays of changes.</param>
        /// <returns>A new <see cref="TrackableChanges{T}"/> allocated with <paramref name="allocator"/>.
        /// The caller owns the memory and is responsible for calling <c>Dispose</c> on it.</returns>
        public sealed override TrackableChanges<XRTrackedObject> GetChanges(Allocator allocator)
        {
            var changes = provider.GetChanges(XRTrackedObject.defaultValue, allocator);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            m_ValidationUtility.ValidateAndDisposeIfThrown(changes);
#endif
            return changes;
        }

        /// <summary>
        /// Registers an implementation of the <see cref="XRObjectTrackingSubsystem"/>.
        /// </summary>
        /// <typeparam name="T">The concrete type of the subsystem being registered.</typeparam>
        /// <param name="id">A unique string identifying the subsystem implementation.</param>
        /// <param name="capabilities">Describes the capabilities of the implementation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="id"/> is <c>null</c>.</exception>
        public static void Register<T>(string id, XRObjectTrackingSubsystemDescriptor.Capabilities capabilities)
            where T : XRObjectTrackingSubsystem.Provider
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            SubsystemDescriptorStore.RegisterDescriptor(new XRObjectTrackingSubsystemDescriptor(id, typeof(T), null, capabilities));
        }

        /// <summary>
        /// Registers a new implementation of the <see cref="XRObjectTrackingSubsystem"/>.
        /// Allows overriding a derived type of <c>XRObjectTrackingSubsystem</c>.
        /// </summary>
        /// <typeparam name="TProvider">The concrete type of the provider being registered.</typeparam>
        /// <typeparam name="TSubsystemOverride">The concrete type of the subsystem being registered.</typeparam>
        /// <param name="id">A unique string identifying the subsystem implementation.</param>
        /// <param name="capabilities">Describes the capabilities of the implementation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="id"/> is <c>null</c>.</exception>
        public static void Register<TProvider, TSubsystemOverride>(string id, XRObjectTrackingSubsystemDescriptor.Capabilities capabilities)
            where TProvider : XRObjectTrackingSubsystem.Provider
            where TSubsystemOverride : XRObjectTrackingSubsystem
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            SubsystemDescriptorStore.RegisterDescriptor(new XRObjectTrackingSubsystemDescriptor(id, typeof(TProvider), typeof(TSubsystemOverride), capabilities));
        }

        XRReferenceObjectLibrary m_Library;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ValidationUtility<XRTrackedObject> m_ValidationUtility =
            new ValidationUtility<XRTrackedObject>();
#endif
    }
}
