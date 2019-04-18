using System;
using System.Collections.Generic;
using System.Text;

namespace BinokelDeluxe.GameLogic
{
    /// <summary>
    /// This class is the public interface for interacting with the internal state machine. The only access provided is through events and triggers.
    /// This allows keeping the whole state management an implementation detail which others will not depend on.
    /// </summary>
    public class SingleGameStateBridge
    {
        private readonly SingleGameStateMachine _stateMachine = new SingleGameStateMachine();

        /// <summary>
        /// Prepares a new game using the given settings. This will create a new event sender and a new trigger sink.
        /// </summary>
        /// <param name="ruleSettings">The rule settings to be used.</param>
        /// <param name="dealerNumber">The number of the dealer player, starting from 0.</param>
        public void PrepareNewGame(RuleSettings ruleSettings, int dealerNumber)
        {
            _stateMachine.RefreshStateMachine(ruleSettings, dealerNumber);
        }

        /// <summary>
        /// Retrieves an object which will send events as described in the ISingleGameEventSender interface.
        /// </summary>
        /// <returns>The event sender.</returns>
        public ISingleGameEventSender GetEventSender() { return _stateMachine; }

        /// <summary>
        /// Retrieves an object which will process triggers as described in the ISingleGameTriggerSink interface.
        /// Be sure to call PrepareNewGame before calling this.
        /// </summary>
        /// <returns>The trigger sink.</returns>
        public ISingleGameTriggerSink GetTriggerSink() { return _stateMachine; }
    }
}
