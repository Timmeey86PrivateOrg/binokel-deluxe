// DOCUMENTED

namespace BinokelDeluxe.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Stores information required for recreating and replaying a single game.
    /// </summary>
    public sealed class GameStateStack : IEquatable<GameStateStack>
    {
        /// <summary>
        /// Gets or sets a list of delta changes which occurred after the initial state. This is public due to XML Serialization only and should not be accessed directly. This is intended for XML Serializing only.
        /// </summary>
        public List<GameStateChangeInfo> DeltaChanges { get; set; } = new List<GameStateChangeInfo>();

        /// <summary>
        /// Gets or sets information required for creating an identical game.
        /// </summary>
        public GameCreationInfo CreationInfo { get; set; }

        /// <summary>
        /// Pushes a state entry to the stack.
        /// </summary>
        /// <param name="entry">The most recent entry.</param>
        public void Push(GameStateChangeInfo entry)
        {
            this.DeltaChanges.Add(entry);
        }

        /// <summary>
        /// Retrieves a state entry from the stack and removes it.
        /// </summary>
        /// <returns>The entry which used to be on top of the stack.</returns>
        public GameStateChangeInfo Pop()
        {
            GameStateChangeInfo entry = null;
            if (this.DeltaChanges.Count != 0)
            {
                entry = this.DeltaChanges[this.DeltaChanges.Count - 1];
                this.DeltaChanges.RemoveAt(this.DeltaChanges.Count - 1);
            }

            return entry;
        }

        /// <summary>
        /// Retrieves the top entry without removing it.
        /// </summary>
        /// <returns>The entry which is currently on top of the stack.</returns>
        public GameStateChangeInfo Peek()
        {
            if (this.DeltaChanges.Count == 0)
            {
                return null;
            }
            else
            {
                return this.DeltaChanges[this.DeltaChanges.Count - 1];
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as GameStateStack);
        }

        /// <summary>
        /// Checks whether or not other is equal to this. Two Game State Stack objects are considered equal if
        /// - Both are not null and
        /// - Both have no creation info, or both creation infos are equal (the content, not the references) and
        /// - Both have no delta changes, or both have the same delta changes in the same order.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>True if other is equal to this.</returns>
        public bool Equals(GameStateStack other)
        {
            if (other == null)
            {
                return false;
            }

            return
                Common.ValueComparer<GameCreationInfo>.Equals(this.CreationInfo, other.CreationInfo) &&
                Common.ListComparer<GameStateChangeInfo>.Equals(this.DeltaChanges, other.DeltaChanges);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                int hash = 17;
                hash = (hash * 29) + Common.ValueComparer<GameCreationInfo>.GetHashCode(this.CreationInfo);
                hash = (hash * 29) + Common.ListComparer<GameStateChangeInfo>.GetHashCode(this.DeltaChanges);
                return hash;
            }
        }
    }
}
