// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    using System;

    /// <summary>
    /// The type of the Binokel Game. Currently, only three player and four player games are supported.
    /// </summary>
    public enum GameType
    {
        /// <summary>
        /// A game where three players play against each other.
        /// </summary>
        ThreePlayerGame,

        /// <summary>
        /// A game where four players play in teams of two against each other.
        /// </summary>
        FourPlayerCrossBinokelGame,
    }

    /// <summary>
    /// The way card values are counted.
    /// </summary>
    public enum CountingType
    {
        /// <summary>
        /// Ten points for aces, tens and kings, zero points for the remainder.
        /// </summary>
        TenPointsForAceToKing,

        /// <summary>
        /// Eleven points for aces, ten for tens, four for kings, 3 for obers, 2 for unters.
        /// </summary>
        DecreasingPointsForAceToUnter,
    }

    /// <summary>
    /// This class contains any setting which affects the game logic.
    /// </summary>
    public sealed class RuleSettings : Common.IConfigurable, IEquatable<RuleSettings>
    {
        /// <summary>
        /// Gets or sets the type of the game, i.e. whether three or four player binokel is being played.
        /// </summary>
        public GameType GameType { get; set; } = GameType.ThreePlayerGame;

        /// <summary>
        /// Gets or sets a value indicating whether the cards which display a seven shall be included.
        /// </summary>
        public bool SevensAreIncluded { get; set; } = true;

        /// <summary>
        /// Gets or sets the way the card values are being counted.
        /// </summary>
        public CountingType CountingType { get; set; } = CountingType.DecreasingPointsForAceToUnter;

        /// <summary>
        /// Gets or sets a value indicating whether the score for each player/team shall be rounded towards the next multiple of ten.
        /// </summary>
        public bool ScoresWillBeRounded { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether when one player goes out, all others will receive 10 points per player in addition to their meld.
        /// </summary>
        public bool ExtraPointsForOthersWhenGoingOut { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether extra points are awarded for melding the seven of trumps.
        /// </summary>
        public bool ExtraPointsForSevenOfTrumps { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether ten points are awarded for winning the last trick rather than the first trick.
        /// </summary>
        public bool ExtraPointsForLastTrickInsteadOfFirst { get; set; } = true;

        /// <summary>
        ///  Gets or sets a value indicating whether the seven of trumps may be discarded despite being melded.
        /// </summary>
        public bool SevenOfTrumpsCanBeMeldedAndDiscarded { get; set; } = true;

        /// <summary>
        ///  Gets or sets a value indicating whether a Bettel (attempt to lose all tricks, and gaining 1000 points in case of success) is allowed.
        /// </summary>
        public bool BettelsAreAllowed { get; set; } = false;

        /// <summary>
        ///  Gets or sets a value indicating whether when player B plays a trump because he cannot follow suit, player C is forced to play a higher trump
        ///  if he has one and also cannot follow suit. If false, player C still needs to play a trump but may play a lower trump than player B.
        /// </summary>
        public bool SecondTrumpMustAlsoWin { get; set; } = true;

        /// <summary>
        /// Checks whether this and other are equal.
        /// Two rule settings objects are equal if all their properties are equal.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>True if this and other are equal.</returns>
        public bool Equals(RuleSettings other)
        {
            if (other == null)
            {
                return false;
            }

            return
                Equals(this.GameType, other.GameType) &&
                this.SevensAreIncluded == other.SevensAreIncluded &&
                Equals(this.CountingType, other.CountingType) &&
                this.ScoresWillBeRounded == other.ScoresWillBeRounded &&
                this.ExtraPointsForLastTrickInsteadOfFirst == other.ExtraPointsForLastTrickInsteadOfFirst &&
                this.ExtraPointsForOthersWhenGoingOut == other.ExtraPointsForOthersWhenGoingOut &&
                this.ExtraPointsForSevenOfTrumps == other.ExtraPointsForSevenOfTrumps &&
                this.SevenOfTrumpsCanBeMeldedAndDiscarded == other.SevenOfTrumpsCanBeMeldedAndDiscarded &&
                this.BettelsAreAllowed == other.BettelsAreAllowed &&
                this.SecondTrumpMustAlsoWin == other.SecondTrumpMustAlsoWin;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as RuleSettings);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // overflow is fine, just wrap
            unchecked
            {
                var hash = 17;
                hash = (hash * 29) + this.GameType.GetHashCode();
                hash = (hash * 29) + this.SevensAreIncluded.GetHashCode();
                hash = (hash * 29) + this.CountingType.GetHashCode();
                hash = (hash * 29) + this.ScoresWillBeRounded.GetHashCode();
                hash = (hash * 29) + this.ExtraPointsForLastTrickInsteadOfFirst.GetHashCode();
                hash = (hash * 29) + this.ExtraPointsForOthersWhenGoingOut.GetHashCode();
                hash = (hash * 29) + this.ExtraPointsForSevenOfTrumps.GetHashCode();
                hash = (hash * 29) + this.SevenOfTrumpsCanBeMeldedAndDiscarded.GetHashCode();
                hash = (hash * 29) + this.BettelsAreAllowed.GetHashCode();
                hash = (hash * 29) + this.SecondTrumpMustAlsoWin.GetHashCode();
                return hash;
            }
        }
    }
}
