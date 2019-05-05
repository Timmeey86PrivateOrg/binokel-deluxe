// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Thrown when there is no listener connected to an event.
    /// Since the state machine will not switch states without anyone sending a trigger, the application would be stuck in that case.
    /// </summary>
    [System.Serializable]
    public class UnconnectedEventException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnconnectedEventException"/> class.
        /// </summary>
        /// <param name="eventName">The name of the event which has no listeners.</param>
        public UnconnectedEventException(string eventName)
            : base(string.Format(
                "Nothing is connected to the {0} event. This would cause the application to be stuck. Connect something to this event and fire the right trigger at the end of it.",
                eventName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnconnectedEventException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected UnconnectedEventException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
