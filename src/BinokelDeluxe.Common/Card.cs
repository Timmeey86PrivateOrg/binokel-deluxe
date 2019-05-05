// DOCUMENTED

namespace BinokelDeluxe.Common
{
    using System;

    /// <summary>
    /// The suit of a card.
    /// </summary>
    public enum CardSuit
    {
        /// <summary>
        /// The "Acorns" suit.
        /// </summary>
        Acorns,

        /// <summary>
        /// The "Leaves" suit.
        /// </summary>
        Leaves,

        /// <summary>
        /// The "Hearts" suit.
        /// </summary>
        Hearts,

        /// <summary>
        /// The "Bells" suit.
        /// </summary>
        Bells,
    }

    /// <summary>
    /// The type of a card.
    /// </summary>
    public enum CardType
    {
        /// <summary>
        /// A card with the number seven.
        /// </summary>
        Seven,

        /// <summary>
        /// A card represented by an "U" letter.
        /// </summary>
        Unter,

        /// <summary>
        /// A card represented by an "O" letter.
        /// </summary>
        Ober,

        /// <summary>
        /// A card represented by a "K" letter.
        /// </summary>
        King,

        /// <summary>
        /// A card with the number ten.
        /// </summary>
        Ten,

        /// <summary>
        /// A card represented by an "A" letter.
        /// </summary>
        Ace,
    }

    /// <summary>
    /// Represents a unique card in the game.
    /// </summary>
    public sealed class Card : IEquatable<Card>
    {
        /// <summary>
        /// Gets or sets the suit of the card.
        /// </summary>
        public CardSuit Suit { get; set; }

        /// <summary>
        /// Gets or sets the type of the card.
        /// </summary>
        public CardType Type { get; set; }

        /// <summary>
        /// Gets or sets the number of the deck (0 or 1).
        /// </summary>
        public short DeckNumber { get; set; }

        /// <inheritdoc/>
        public bool Equals(Card other)
        {
            if (other == null)
            {
                return false;
            }

            return Equals(this.Suit, other.Suit) && Equals(this.Type, other.Type) && this.DeckNumber == other.DeckNumber;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Card);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                int hash = 17;
                hash = (hash * 29) + this.Suit.GetHashCode();
                hash = (hash * 29) + this.Type.GetHashCode();
                hash = (hash * 29) + this.DeckNumber.GetHashCode();
                return hash;
            }
        }
    }
}
