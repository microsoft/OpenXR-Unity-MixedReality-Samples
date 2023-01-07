using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.XR.Management.EditorTests")]
namespace UnityEngine.XR.Management.Tests
{
    internal class DummyLoader : XRLoader
    {
        public bool shouldFail = false;
        public int id;
        public GraphicsDeviceType supportedDeviceType = GraphicsDeviceType.Null;

        public DummyLoader()
        {
        }

        public override bool Initialize()
        {
            return !shouldFail;
        }

        public override T GetLoadedSubsystem<T>()
        {
            return default(T);
        }

        public override List<GraphicsDeviceType> GetSupportedGraphicsDeviceTypes(bool buildingPlayer)
        {
            if (supportedDeviceType == GraphicsDeviceType.Null)
            {
                return new List<GraphicsDeviceType>();
            }
            return new List<GraphicsDeviceType>() { supportedDeviceType };
        }

        protected bool Equals(DummyLoader other)
        {
            return base.Equals(other) && shouldFail == other.shouldFail && id == other.id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DummyLoader)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ shouldFail.GetHashCode();
                hashCode = (hashCode * 397) ^ id;
                return hashCode;
            }
        }
    }
}
