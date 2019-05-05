// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// This interface can be used to send triggers to the internal state machine.
    /// See https://github.com/Timmeey86/binokel-deluxe/blob/statemachine/doc/modelio/img/08_01_SingleGameStateMachine.png for the used triggers and related transitions.
    /// </summary>
    public interface ISingleGameTriggerSink
    {
        /// <summary>
        /// Sends a trigger to the trigger sink.
        /// </summary>
        /// <param name="trigger">The trigger to be sent.</param>
        void SendTrigger(Common.GameTrigger trigger);
    }
}
