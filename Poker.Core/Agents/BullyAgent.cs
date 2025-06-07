using Poker.Core.Interfaces;
using Poker.Core.Models;
using System.Linq;

namespace Poker.Core.Agents
{
    public class BullyAgent : IPlayerStrategy
    {
        /// <summary>
        /// The BULLY's strategy:
        /// - Always ALL IN if you have the most chips left, no matter the situation.
        /// - Else, Check or Fold
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {

            if ( state.YourStack > state.OtherActivePlayers.Values.Max(p => p.Chips) + state.ToCall )
            {
                return new PlayerAction(PlayType.AllIn);
            }

            // revert to cowardice
            if ( state.ToCall == 0 )
            {
                return new PlayerAction(PlayType.Check);
            }

            return new PlayerAction(PlayType.Fold);
        }
    }
}