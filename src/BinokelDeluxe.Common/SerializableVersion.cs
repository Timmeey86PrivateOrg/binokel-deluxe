namespace BinokelDeluxe.Common
{
    using System;

    /// <summary>
    /// Provides a serializable, comparable version.
    /// </summary>
    public sealed class SerializableVersion : IEquatable<SerializableVersion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableVersion"/> class.
        /// </summary>
        public SerializableVersion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        public SerializableVersion(int major, int minor)
        {
            this.Major = major;
            this.Minor = minor;
        }

        /// <summary>
        /// Gets or sets the major version number.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Gets or sets the minor version number.
        /// </summary>
        public int Minor { get; set; }

        /// <inheritdoc/>
        public bool Equals(SerializableVersion other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Major == other.Major && this.Minor == other.Minor;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as SerializableVersion);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // overflow is fine, just wrap
            unchecked
            {
                int hash = 17;
                hash = (hash * 29) + this.Major;
                hash = (hash * 29) + this.Minor;
                return hash;
            }
        }
    }
}
