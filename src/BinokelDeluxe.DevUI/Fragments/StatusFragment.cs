using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinokelDeluxe.DevUI.Fragments
{
    /// <summary>
    /// This fragment is responsible for displaying the player status.
    /// </summary>
    internal class StatusFragment
    {
        public SpriteFont Font { get; set; }

        public enum PlayerStatus
        {
            Waiting,
            Dealing,
            Bidding,
            Passed
        }

        private List<PlayerStatus> _playerStatuses = null;
        private List<int> _playerBidAmounts = null;
        private List<Vector2> _playerTextPositions;
        private int _numberOfPlayers;
        private int _dealerPosition;

        public void DisplayWaitingStatus(int numberOfPlayers)
        {
            _numberOfPlayers = numberOfPlayers;
            _playerStatuses = Enumerable.Repeat(PlayerStatus.Waiting, numberOfPlayers).ToList();
            _playerBidAmounts = Enumerable.Repeat(0, numberOfPlayers).ToList();
            CalculatePlayerTextPositions();
        }

        public void StartDealing(int dealerPosition)
        {
            _dealerPosition = dealerPosition;
            _playerStatuses[dealerPosition] = PlayerStatus.Dealing;
        }

        public void FinishDealing()
        {
            _playerStatuses[_dealerPosition] = PlayerStatus.Waiting;
        }

        public void SetPlayerBidding(int playerPosition)
        {
            _playerStatuses[playerPosition] = PlayerStatus.Bidding;
        }

        public void SetPlayerPassed(int playerPosition)
        {
            _playerStatuses[playerPosition] = PlayerStatus.Passed;
        }

        public void SetPlayerBid(int playerPosition, int amount)
        {
            _playerStatuses[playerPosition] = PlayerStatus.Waiting;
            _playerBidAmounts[playerPosition] = amount;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_playerStatuses == null) return;

            for(int playerIndex = 0; playerIndex < _numberOfPlayers; playerIndex++)
            {
                spriteBatch.DrawString(Font, _playerStatuses[playerIndex].ToString(), _playerTextPositions[playerIndex], Color.Black, .0f, new Vector2(), 0.2f, SpriteEffects.None, 1.0f);
                if(_playerBidAmounts[playerIndex] > 0 )
                {
                    spriteBatch.DrawString(Font, _playerBidAmounts[playerIndex].ToString(), _playerTextPositions[playerIndex] + new Vector2(.0f, 20f), Color.Black, .0f, new Vector2(), 0.2f, SpriteEffects.None, 1.0f);
                }
            }
        }
        private void CalculatePlayerTextPositions()
        {
            if (_numberOfPlayers == 3)
            {
                _playerTextPositions = new List<Vector2>()
                {
                    new Vector2(375, 420),
                    new Vector2(660, 180),
                    new Vector2(110, 180)
                };
            }
            else
            {
                _playerTextPositions = new List<Vector2>()
                {
                    new Vector2(375, 420),
                    new Vector2(660, 180),
                    new Vector2(380, 100),
                    new Vector2(100, 180)
                };
            }
        }
    }
}
