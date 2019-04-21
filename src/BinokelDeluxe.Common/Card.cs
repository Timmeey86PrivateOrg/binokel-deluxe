// DOCUMENTED

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
    /// Represents a card in the game. There are two instances of each card in each game.
    /// </summary>
    public class Card
    {
        public CardSuit Suit { get; set; }
        public CardType Type { get; set; }
    }
}
