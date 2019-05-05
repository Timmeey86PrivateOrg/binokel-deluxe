// DOCUMENTED

namespace BinokelDeluxe.Common
{
    using System;

    /// <summary>
    /// This class can be used to forward a Card object through the event mechanism.
    /// </summary>
    public class CardEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardEventArgs"/> class.
        /// </summary>
        /// <param name="card">The card to be made available for event listeners.</param>
        public CardEventArgs(Card card)
        {
            this.Card = card;
        }

        /// <summary>
        /// Gets the card for which this event was fired.
        /// </summary>
        public Card Card { get; private set; }
    }
}
