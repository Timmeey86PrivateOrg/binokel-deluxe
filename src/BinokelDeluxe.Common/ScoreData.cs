using System;
using System.Collections.Generic;
using System.Text;

// DOCUMENTED

namespace BinokelDeluxe.Common
{
    /// <summary>
    /// The type of the score.
    /// </summary>
    public enum ScoreType
    {
        PlayerScore,
        TeamScore
    }

    /// <summary>
    /// Contains the score of a single player or team (in case of cross-Binokel).
    /// This is a struct, so you need to pass it as a ref object if you want to modify it.
    /// </summary>
    public struct ScoreData
    {
        /// <summary>
        /// The type of the score.
        /// </summary>
        public ScoreType ScoreType { get; set; }
        /// <summary>
        /// The points of the bidding phase in case the player or team won that phase.
        /// </summary>
        public short BiddingPoints { get; set; }
        /// <summary>
        /// The amount of points which were melded.
        /// </summary>
        public short MeldedPoints { get; set; }
        /// <summary>
        /// The amount of points received for winning tricks.
        /// </summary>
        public short TrickPoints { get; set; }
        /// <summary>
        /// Extra points which were received, e.g. because a player went out, or a Bettel or Durch was won or lost.
        /// This value may be negative. 
        /// </summary>
        public short ExtraPoints { get; set; }
        /// <summary>
        /// The amount of points for the current round. Note that melded points are only counted if at least one trick was won.
        /// This value may be negative. 
        /// </summary>
        public short RoundScore { get; set; }
        /// <summary>
        /// The total score of all rounds played so far.
        /// This value may be negative. 
        /// </summary>
        public short TotalScore { get; set; }
    }
}
