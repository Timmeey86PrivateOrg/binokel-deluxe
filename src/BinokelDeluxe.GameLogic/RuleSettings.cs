using System;
using System.Collections.Generic;
using System.Text;

namespace BinokelDeluxe.GameLogic
{
    public enum GameType
    {
        /// <summary>
        /// A game where three players play against each other.
        /// </summary>
        ThreePlayerGame,
        /// <summary>
        /// A game where four players play in teams of two against each other.
        /// </summary>
        FourPlayerCrossBinokelGame
    }

    public enum CountingType
    {
        /// <summary>
        /// Ten points for aces, tens and kings, zero points for the remainder.
        /// </summary>
        TenPointsForAceToKing,
        /// <summary>
        /// Eleven points for aces, ten for tens, four for kings, 3 for obers, 2 for unters.
        /// </summary>
        DecreasingPointsForAceToUnter
    }

    /// <summary>
    /// This class contains any setting which affects the game logic.
    /// </summary>
    public class RuleSettings : Shared.IConfigurable
    {
        /// <summary>
        /// Defines whether three or four player binokel is being played.
        /// </summary>
        public GameType GameType { private set; get; }

        /// <summary>
        /// True if the cards which display a seven shall be included.
        /// </summary>
        public bool SevensAreIncluded { private set; get; }

        /// <summary>
        /// Defines the score of each card.
        /// </summary>
        public CountingType CountingType { private set; get; }

        /// <summary>
        /// True if the score for each player/team shall be rounded towards the next multiple of ten.
        /// </summary>
        public bool ScoresWillBeRounded { private set; get; }

        /// <summary>
        /// True if, when one player goes out, all others will receive 10 points per player in addition to their meld.
        /// </summary>
        public bool ExtraPointsForOthersWhenGoingOut { private set; get; }
        
        /// <summary>
        /// True if the seven of trumps can be melded for ten points.
        /// </summary>
        public bool ExtraPointsForSevenOfTrumps { private set; get; }

        /// <summary>
        /// True if the ten points are awarded for winning the last trick, false if ten points are awarded for winning the first trick.
        /// </summary>
        public bool ExtraPointsForLastTrickInsteadOfFirst { private set; get; }

        /// <summary>
        /// True if the seven of trumps may be discarded despite being melded.
        /// </summary>
        public bool SevenOfTrumpsCanBeMeldedAndDiscarded { private set; get; }

        /// <summary>
        /// True if a Bettel (attempt to lose all tricks, and gaining 1000 points in case of success) is allowed.
        /// </summary>
        public bool BettelsAreAllowed { private set; get; }

        /// <summary>
        /// True if when player B plays a trump because he cannot follow suit, player C is forced to play a higher trump if he has one and also cannot follow suit.
        /// If false, player C still needs to play a trump but may play a lower trump than player B.
        /// </summary>
        public bool SecondTrumpMustAlsoWin { private set; get; }
    }
}
