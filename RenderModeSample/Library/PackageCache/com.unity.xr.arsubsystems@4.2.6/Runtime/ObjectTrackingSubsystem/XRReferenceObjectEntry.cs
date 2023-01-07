namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A provider-specific reference object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A reference object represents a 3D scan of a real object that can
    /// be recognized in the environment. Each <see cref="XRReferenceObject"/>
    /// contains a list of provider-specific <see cref="XRReferenceObjectEntry"/>.
    /// Each provider (implementation of <see cref="XRObjectTrackingSubsystem"/>)
    /// should derive a new type from this type.
    /// </para><para>
    /// Each <see cref="XRReferenceObjectEntry"/> is generally an asset on disk
    /// in a format specific to that provider.
    /// </para>
    /// </remarks>
    /// <seealso cref="XRReferenceObject"/>
    /// <seealso cref="XRReferenceObjectLibrary"/>
    [HelpURL(HelpUrls.Api + "UnityEngine.XR.ARSubsystems.XRReferenceObjectEntry.html")]
    public abstract class XRReferenceObjectEntry : ScriptableObject
    {
        /// <summary>
        /// Invoked when an <see cref="XRReferenceObject"/> is added to an <see cref="XRReferenceObjectLibrary"/>.
        /// </summary>
        /// <remarks>
        /// Override this method if your <see cref="XRReferenceObjectEntry"/> needs to perform logic when a novel
        /// <see cref="XRReferenceObject"/> is added to an <see cref="XRReferenceObjectLibrary"/>.
        ///
        /// The default implementation takes no action and has no effect.
        /// </remarks>
        /// <param name="library">The library to which <param name="referenceObject"> is being added.</param></param>
        /// <param name="referenceObject">The reference object being added to <paramref name="library"/>.</param>
        protected internal virtual void OnAddToLibrary(XRReferenceObjectLibrary library, XRReferenceObject referenceObject) { }
    }
}
