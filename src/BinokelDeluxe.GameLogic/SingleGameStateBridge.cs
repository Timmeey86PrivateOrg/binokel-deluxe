// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    /// <summary>
    /// This class is the public interface for interacting with the internal state machine. The only access provided is through events and triggers.
    /// This allows keeping the whole state management an implementation detail which others will not depend on.
    /// </summary>
    public class SingleGameStateBridge
    {
        private readonly SingleGameStateMachine stateMachine = new SingleGameStateMachine();

        /// <summary>
        /// Gets an object which will send events as described in the ISingleGameEventSource interface.
        /// </summary>
        /// <returns>The event source.</returns>
        public ISingleGameEventSource EventSource
        {
            get { return this.stateMachine; }
        }

        /// <summary>
        /// Gets an object which will process triggers as described in the ISingleGameTriggerSink interface.
        /// Be sure to call PrepareNewGame before calling this.
        /// </summary>
        /// <returns>The trigger sink.</returns>
        public ISingleGameTriggerSink TriggerSink
        {
            get { return this.stateMachine; }
        }

        /// <summary>
        /// Prepares a new game using the given settings. This will create a new event source and a new trigger sink.
        /// </summary>
        /// <param name="ruleSettings">The rule settings to be used.</param>
        /// <param name="dealerPosition">The position of the dealer on the table, where 0 is the human player (single player) or the host (multiplayer).</param>
        public void PrepareNewGame(RuleSettings ruleSettings, int dealerPosition)
        {
            this.stateMachine.RefreshStateMachine(ruleSettings, dealerPosition);
        }
    }
}
