using Poker.Core.Interfaces;
using Poker.Core.Models;
using System;

namespace Poker.Core.Agents
{
    public class FlipACoinAgent : IPlayerStrategy
    {
        private static readonly Random _rng = new();

        // 50/50 between folding and going all in
        public PlayerAction Act( ActRequest state )
        {
            return new PlayerAction(_rng.Next(2) == 0 ? PlayType.Fold : PlayType.AllIn);
        }
    }

}
