// DOCUMENTED

namespace BinokelDeluxe.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// Stores information about a single meld, consisting of several cards.
    /// If a card is part of multiple melds, it will be part of several SingleMeld objects.
    /// </summary>
    public class SingleMeld
    {
        /// <summary>
        /// Gets or sets the cards related to this meld.
        /// </summary>
        public List<Card> Cards { get; set; } = new List<Card>();

        /// <summary>
        /// Gets or sets the points of this meld.
        /// </summary>
        public int Points { get; set; } = 0;
    }
}
