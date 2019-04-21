// DOCUMENTED

using System;

namespace BinokelDeluxe.Common
{
    /// <summary>
    /// The suit of a card.
    /// </summary>
    public enum CardSuit
    {
        Acorns,
        Leaves,
        Hearts,
        Bells
    }

    /// <summary>
    /// The type of a card.
    /// </summary>
    public enum CardType
    {
        Seven,
        Unter,
        Ober,
        King,
        Ten,
        Ace,
    }

    /// <summary>
    /// Represents a unique card in the game.
    /// </summary>
    public sealed class Card : IEquatable<Card>
    {
        public CardSuit Suit { get; set; }
        public CardType Type { get; set; }
        public short DeckNumber { get; set; }

        public bool Equals(Card other)
        {
            if (other == null)
            {
                return false;
            }

            return Enum.Equals(Suit, other.Suit) && Enum.Equals(Type, other.Type) && DeckNumber == other.DeckNumber;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Card);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 29 + Suit.GetHashCode();
                hash = hash * 29 + Type.GetHashCode();
                hash = hash * 29 + DeckNumber.GetHashCode();
                return hash;
            }
        }
    }
}
