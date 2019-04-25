using System;
using System.Collections.Generic;
using System.Text;

namespace BinokelDeluxe.UI
{ 
    /// <summary>
    /// The actions which are possible in the main menu.
    /// </summary>
    public enum MainMenuActions
    {
        StartGame,
        DisplayScoreboard,
        Quit
    }
    /// <summary>
    /// Defines the interface for accessig the User Interface.
    /// Note: If UI updates are performed in a different thread (recommended), interface implementations should block the calling thread until the UI finished its work.
    /// This is vital since code in the calling thread will assume the UI is idle whenever a method returns.
    /// Note: This interface is rather large. Feel free to implement an adapter which delegates work to several smaller worker classes.
    /// </summary>
    public interface IUserInterface
    {
        /// <summary>
        /// Implementers should display a main menu and return the selected action.
        /// </summary>
        /// <returns>The main menu.</returns>
        MainMenuActions DisplayMainMenu();
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
        /// <param name="numberOfCardsPerPlayer">The amount of cards for each player.</param>
        /// <param name="numberOfCardsInDabb">The amount of cards which shall be displayed in the dabb.</param>
        void PlayDealingAnimation(
            int dealerPosition,
            int numberOfCardsPerPlayer,
            int numberOfCardsInDabb
            );
        /// <summary>
        /// Implementers should uncover the cards of the user for himself.
        /// The cards should be displayed in the order provided by userCards, if possible.
        /// </summary>
        /// <param name="userCards">The cards of the user.</param>
        void UncoverCardsForUser(IEnumerable<Common.Card> userCards);
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
        /// The options should be removed immediately after selecting one.
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
        /// Implementers should uncover the dabb for all players and wait for confirmation of the user that the dabb was seen.
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
        /// <summary>
        /// Implementers should rearrange (or remove and re-initialize) the cards of the player in accordance with the order in rearrangedCards.
        /// </summary>
        /// <param name="rearrangedCards">The new order of cards of the human player.</param>
        void RearrangeCardsForUser(IEnumerable<Common.Card> rearrangedCards);
        /// <summary>
        /// Implementes should display the given melds. The enumerable contains one entry for each player, ordered by player position on the table
        /// (i.e. independent of who is the current dealer). The UI is free to decide whether a list of all cards, or the single melds as groups are displayed.
        /// </summary>
        /// <param name="meldsByPlayers">The melds made by each player.</param>
        void DisplayMelds(IEnumerable<Common.MeldData> meldsByPlayers);
        /// <summary>
        /// Implementers should display a scoreboard for the current round as a result of the player who won the bidding phase going out.
        /// The list of score data will either contain data for three players or two teams.
        /// </summary>
        /// <param name="playerOrTeamScores">The scores of the players or teams, where the first player (or team) is the user.</param>
        void DisplayGoingOutScore(IEnumerable<Common.ScoreData> playerOrTeamScores);
        /// <summary>
        /// Implementers should let the user know that the player at the given position will perform the next move.
        /// </summary>
        /// <param name="playerPosition">The position of the player on the table where 0 is the user.</param>
        void ActivatePlayer(int playerPosition);
        /// <summary>
        /// Implementers should let the user select a card to be played. The card should not be removed from the player's hand yet.
        /// </summary>
        /// <returns>The card which was selected by the user.</returns>
        Common.Card LetUserSelectCard();
        /// <summary>
        /// Implementers should let the user know they attempted to play an invalid card and should highlight the valid cards in a way.
        /// </summary>
        /// <param name="validCards">The cards which are allowed to be played.</param>
        void HandleInvalidMove(IEnumerable<Common.Card> validCards);
        /// <summary>
        /// Implementers should remove any currently displayed validity states from the UI.
        /// </summary>
        void RemoveValidityState();
        /// <summary>
        /// Implementers should show an animation that moves the given card from the given player to the middle, while uncovering it for all players.
        /// This is also sent for human players.
        /// </summary>
        /// <param name="playerPosition">The position of the player on the table where 0 is the user.</param>
        /// <param name="card">The card which shall be placed.</param>
        void PlaceCardInMiddle(int playerPosition, Common.Card card);
        /// <summary>
        /// Implementers should move the cards from the middle to the trick winner.
        /// </summary>
        /// <param name="playerPosition">The position of the player on the table where 0 is the user.</param>
        void MoveCardsToTrickWinner(int playerPosition);
        /// <summary>
        /// Implementers should display a scoreboard for the current game as a result of all cards being placed.
        /// </summary>
        /// <param name="playerOrTeamScores">The scores of the players or teams, where the first player (or team) is the user.</param>
        void DisplayGameScore(IEnumerable<Common.ScoreData> playerOrTeamScores);
    }
}
