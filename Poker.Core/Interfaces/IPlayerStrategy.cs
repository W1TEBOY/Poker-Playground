using Poker.Core.Models;

namespace Poker.Core.Interfaces
{
    public interface IPlayerStrategy
    {
        /// <summary>
        /// Given the current table state, decide what Action to take.
        /// </summary>
        PlayerAction Act( ActRequest state );
    }
}