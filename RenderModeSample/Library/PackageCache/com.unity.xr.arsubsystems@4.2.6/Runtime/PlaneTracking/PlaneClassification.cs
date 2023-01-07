namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the alignment of a plane (for example, whether it is horizontal or vertical).
    /// </summary>
    /// <seealso cref="BoundedPlane.classification"/>
    public enum PlaneClassification
    {
        /// <summary>
        /// The plane does not match any available classification.
        /// </summary>
        None = 0,

        /// <summary>
        /// The plane is horizontal with an upward facing normal (for example, a floor).
        /// </summary>
        Wall,

        /// <summary>
        /// The plane is classified as the floor.
        /// </summary>
        Floor,

        /// <summary>
        /// The plane is classified as the ceiling.
        /// </summary>
        Ceiling,

        /// <summary>
        /// The plane is classified as a table.
        /// </summary>
        Table,

        /// <summary>
        /// The plane is classified as a seat.
        /// </summary>
        Seat,

        /// <summary>
        /// The plane is classified as a door.
        /// </summary>
        Door,

        /// <summary>
        /// The plane is classified as a window.
        /// </summary>
        Window
    }
}
