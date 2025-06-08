using Poker.Core.Interfaces;
using Poker.Core.Models;

namespace Poker.Core.Agents
{
    public class AllIn : IPlayerStrategy
    {
        /// <summary>
        /// The GIGA CHAD's strategy:
        /// - Always ALL IN BABYYYY, no matter the situation. 
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {
            // 1) Be a GIGA CHAD
            return new PlayerAction(PlayType.AllIn);
        }
    }
}