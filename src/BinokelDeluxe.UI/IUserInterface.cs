using System;
using System.Collections.Generic;
using System.Text;

namespace BinokelDeluxe.UI
{ 
    /// <summary>
    /// Defines the interface for accessig the User Interface.
    /// Note: If UI updates are performed in a different thread (recommended), interface implementations should block the calling thread until the UI finished its work.
    /// This is vital since code in the calling thread will assume the UI is idle whenever a method returns.
    /// </summary>
    public interface IUserInterface
    {
        /// <summary>
        /// Implementers should create or update the table to reflect the new dealer position.
        /// Additionally, all players need to be displayed in a state where they did not bid yet.
        /// </summary>
        /// <param name="dealerPosition">The position of the dealer, where 0 is the position of the device user.</param>
        void PrepareTable(int dealerPosition);
        /// <summary>
        /// Implementers should play a dealing animation, with all cards covered.
        /// </summary>
        /// <param name="dealerPosition">The position of the dealer, where 0 is the position of the device user.</param>
        /// <param name="cardsByPlayers">A two-dimensional collection of cards where the outer dimension is the position of the player on the table (where
        /// position 0 = user) and the inner dimension is the collection of cards (10 or 12, dependent on settings) the player received.
        /// The UI should list these cards in the order provided by the IEnumerable.</param>
        /// <param name="numberOfCardsInDabb">The amount of cards which shall be displayed in the dabb.</param>
        void PlayDealingAnimation(
            int dealerPosition,
            IEnumerable<IEnumerable<Common.Card>> cardsByPlayers,
            int numberOfCardsInDabb
            );
        /// <summary>
        /// Implementers should uncover the cards of the user for himself.
        /// The relevant cards were provided in PlayDealingAnimation which is guaranteed to be called before this method.
        /// </summary>
        void UncoverCardsForUser();
        /// <summary>
        /// Implementers should ask the user to either make the first bid or pass. This may or may not be called for the player, dependent on their position and the actions
        /// performed by anyone being allowed to decide before them.
        /// The UI shall not switch to the next player yet.
        /// </summary>
        /// <param name="initialBidAmount">The amount which must be placed as first bid. This is usually a predefined amount which can not be changed by the player.</param>
        /// <returns>BidPlaced or Passed.</returns>
        Common.GameTrigger LetUserPlaceFirstBidOrPass(int initialBidAmount);
        /// <summary>
        /// Implementers should let the user know that the player at the given position placed the given bid.
        /// </summary>
        /// <param name="playerPosition">The position of the player who made a bid.</param>
        /// <param name="bidAmount">The amount which was bid by that player.</param>
        void DisplayAIBid(int playerPosition, int bidAmount);
        /// <summary>
        /// Implementers should ask the user to either counter the current bid or pass.
        /// </summary>
        /// <param name="nextBidAmount">The new amount which can be placed by the player. This is usually a fixed amount of points above the current bid.</param>
        /// <returns>BidPlaced or Passed.</returns>
        Common.GameTrigger LetUserDoCounterBidOrPass(int nextBidAmount);
        /// <summary>
        /// Implementers should let the user know that the given player (which might be the user itself) passed and will not be able to bid again in the current phase.
        /// It is probably a good idea to keep this information displayed (in a non-annoying way) until the end of the bidding phase.
        /// </summary>
        /// <param name="playerPosition">The player who passed.</param>
        void DisplayPlayerAsPassed(int playerPosition);
        /// <summary>
        /// Implementers should uncover the dabb for all players.
        /// </summary>
        /// <param name="cardsInDabb">The cards which are in the dabb.</param>
        void UncoverDabb(IEnumerable<Common.Card> cardsInDabb);
        /// <summary>
        /// Implementers should let the user exchange cards with the Dabb.
        /// After the exchange, the same amount of cards which were in the dabb will be discarded.
        /// Implementers also need to offer the user the choice to either start the game, go out, start a Bettel or start a Durch.
        /// In the first two cases, the user also needs to select a trump.
        /// </summary>
        /// <param name="discardedCards">Implementers should store the discarded cards in here. The algorithm will automatically know which cards were taken out of the dabb.</param>
        /// <param name="trumpSuit">Implementers need to store the suit which was selected as trump, or null in case of a Bettel or Durch.</param>
        /// <returns>TrumpSelected, GoingOut, BettelAnnounced or DurchAnnounced.</returns>
        Common.GameTrigger LetUserExchangeCardsWithDabb(out IEnumerable<Common.Card> discardedCards, out Common.CardSuit? trumpSuit);
    }
}
