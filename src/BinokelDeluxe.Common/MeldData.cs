using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// DOCUMENTED

namespace BinokelDeluxe.Common
{
    /// <summary>
    /// Stores information about a single meld, consisting of several cards.
    /// If a card is part of multiple melds, it will be part of several SingleMeld objects.
    /// </summary>
    public class SingleMeld
    {
        /// <summary>
        /// The cards related to this meld.
        /// </summary>
        public List<Card> Cards { get; set; } = new List<Card>();
        /// <summary>
        /// The points for this meld.
        /// </summary>
        public int Points { get; set; } = 0;
    }

    /// <summary>
    /// Stores data about all melds of a single player.
    /// </summary>
    public class MeldData
    {
        /// <summary>
        /// The melds of a single player. May contain the same card multiple times (e.g. Pair + Binokel).
        /// </summary>
        public List<SingleMeld> Melds { get; set; } = new List<SingleMeld>();

        /// <summary>
        /// Retrieves a distinct list of melded cards ordered by suit and then descending type (Ace to Seven).
        /// </summary>
        public IEnumerable<Card> Cards
        {
            get
            {
                return Melds.SelectMany(meld => meld.Cards)
                    .Distinct()
                    .OrderBy(card => card.Suit)
                    .ThenByDescending(card => card.Type);
            }
        }

        /// <summary>
        /// Retrieves the accumulated points of all melds.
        /// </summary>
        public int Points
        {
            get
            {
                return Melds.Sum(meld => meld.Points);
            }
        }
    }
}
