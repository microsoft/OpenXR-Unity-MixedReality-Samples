namespace Unity.Services.Core.Environments.Internal
{
    /// <inheritdoc />
    class Environments : IEnvironments
    {
        public string Current { get; internal set; }
    }
}
