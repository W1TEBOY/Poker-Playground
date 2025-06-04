using Poker.Core.Interfaces;
using Poker.Core.Models;

namespace Poker.Core.Agents
{
    public class FoldAgent : IPlayerStrategy
    {
        /// <summary>
        /// The coward's strategy:
        /// - Always folds, no matter the situation. 
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {
            // 1) Be a coward
            return new PlayerAction(PlayType.Fold);
        }
    }
}