using System.Threading.Tasks;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// Contract for objects able to register a set of <see cref="IServiceComponent"/>
    /// to a given <see cref="CoreRegistry"/>.
    /// </summary>
    public interface IInitializablePackage
    {
        /// <summary>
        /// Start the process of registering all <see cref="IServiceComponent"/>
        /// provided by this package to the given <paramref name="registry"/>.
        /// </summary>
        /// <param name="registry">
        /// The <see cref="IServiceComponent"/> container to use.
        /// It provides the available <see cref="IServiceComponent"/> and this package
        /// will register the provided <see cref="IServiceComponent"/> to it.
        /// </param>
        /// <returns>
        /// Return a handle to the asynchronous initialization process.
        /// </returns>
        Task Initialize(CoreRegistry registry);
    }
}
