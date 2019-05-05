// DOCUMENTED

namespace BinokelDeluxe.Common
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Stores data about all melds of a single player.
    /// </summary>
    public class MeldData
    {
        /// <summary>
        /// Gets or sets the melds of a single player. May contain the same card multiple times (e.g. Pair + Binokel).
        /// </summary>
        public List<SingleMeld> Melds { get; set; } = new List<SingleMeld>();

        /// <summary>
        /// Gets a distinct list of melded cards ordered by suit and then descending type (Ace to Seven).
        /// </summary>
        public IEnumerable<Card> Cards
        {
            get
            {
                return this.Melds.SelectMany(meld => meld.Cards)
                    .Distinct()
                    .OrderBy(card => card.Suit)
                    .ThenByDescending(card => card.Type);
            }
        }

        /// <summary>
        /// Gets the accumulated points of all melds.
        /// </summary>
        public int Points
        {
            get
            {
                return this.Melds.Sum(meld => meld.Points);
            }
        }
    }
}
