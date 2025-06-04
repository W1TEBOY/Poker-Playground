namespace Poker.Core.Models
{
    using Poker.Core.Interfaces;
    using System;

    public class Player
    {
        // --- Properties ---
        public Guid Id
        {
            get;
        }
        public string Name
        {
            get;
        }
        public Position Position
        {
            get; set;
        }
        public Hand Hand
        {
            get; set;
        }
        public int Chips
        {
            get; private set;
        }
        private readonly IPlayerStrategy _strategy;

        // --- Constructor with optional Guid ---
        public Player( string name, int startChips, int playerCount, int currentPosition, IPlayerStrategy strategy, Guid? id = null )
        {
            _strategy = strategy;
            Chips = startChips;
            Hand = new Hand();
            Id = id ?? Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Position = new Position(playerCount, currentPosition);
        }

        // --- Betting method stub ---
        public int Bet(
            int betSize
        )
        {
            int bet = Math.Min(betSize, Chips);
            Chips -= bet;
            return bet;
        }

        public void AddWinnings( int potShare )
        {
            Chips += potShare;
        }

        public PlayerAction Act( ActRequest state )
        {
            return _strategy.Act(state);
        }
    }
}
