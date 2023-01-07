namespace Unity.Services.Core.Editor.OrganizationHandler
{
    /// <summary>
    /// Helper class to get information of the current organization.
    /// </summary>
    public static class OrganizationProvider
    {
        static IOrganizationHandler s_OrganizationHandler = new OrganizationHandler();

        /// <summary>
        /// Current organization info.
        /// </summary>
        public static IOrganizationHandler Organization
        {
            get => s_OrganizationHandler;
            set
            {
                if (value != null)
                {
                    s_OrganizationHandler = value;
                }
            }
        }
    }
}
