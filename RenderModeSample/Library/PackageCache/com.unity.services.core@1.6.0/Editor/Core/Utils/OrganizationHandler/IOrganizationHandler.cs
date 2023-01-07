namespace Unity.Services.Core.Editor.OrganizationHandler
{
    /// <summary>
    /// The interface to access the organization properties
    /// </summary>
    public interface IOrganizationHandler
    {
        /// <summary>
        /// Retrieves the OrganizationKey
        /// </summary>
        string Key { get; }
    }
}
