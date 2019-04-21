using System;
using System.Collections.Generic;
using System.Text;

// DOCUMENTED

namespace BinokelDeluxe.Core
{
    /// <summary>
    /// Stores information required for recreating and replaying a single game.
    /// </summary>
    public class GameStateStack
    {
        /// <summary>
        /// Gets or sets a list of delta changes which occurred after the initial state. This is public due to XML Serialization only and should not be accessed directly. This is intended for XML Serializing only.
        /// </summary>
        public IList<GameStateChangeInfo> DeltaChanges { get; set; } = new List<GameStateChangeInfo>();

        /// <summary>
        /// Information required for creating an identical game.
        /// </summary>
        public GameCreationInfo CreationInfo { get; set; }
        /// <summary>
        /// Pushes a state entry to the stack.
        /// </summary>
        /// <param name="entry">The most recent entry.</param>
        public void Push(GameStateChangeInfo entry)
        {
            DeltaChanges.Add(entry);
        }

        /// <summary>
        /// Retrieves a state entry from the stack and removes it.
        /// </summary>
        /// <returns>The entry which used to be on top of the stack.</returns>
        public GameStateChangeInfo Pop()
        {
            GameStateChangeInfo entry = null;
            if (DeltaChanges.Count != 0 )
            {
                entry = DeltaChanges[DeltaChanges.Count - 1];
                DeltaChanges.RemoveAt(DeltaChanges.Count - 1);
            }
            return entry;
        }

        /// <summary>
        /// Retrieves the top entry without removing it.
        /// </summary>
        /// <returns>The entry which is currently on top of the stack.</returns>
        public GameStateChangeInfo Peek()
        {
            if(DeltaChanges.Count == 0)
            {
                return null;
            }
            else
            {
                return DeltaChanges[DeltaChanges.Count - 1];
            }
        }
    }
}
