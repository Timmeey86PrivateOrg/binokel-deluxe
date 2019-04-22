using System;

namespace BinokelDeluxe.Common
{
    /// <summary>
    /// Provides a serializable version.
    /// </summary>
    public sealed class SerializableVersion : IEquatable<SerializableVersion>
    {
        public int Major { get; set; }
        public int Minor { get; set; }

        public SerializableVersion()
        {

        }
        public SerializableVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public bool Equals(SerializableVersion other)
        {
            if (other == null)
            {
                return false;
            }

            return Major == other.Major && Minor == other.Minor;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SerializableVersion);
        }

        public override int GetHashCode()
        {
            unchecked // overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 29 + Major;
                hash = hash * 29 + Minor;
                return hash;
            }
        }
    }
}
