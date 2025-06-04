namespace Poker.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Hand
    {
        // Backing list—clients only see IReadOnlyList
        private readonly List<Card> _cards = new();

        // Expose as read-only
        public IReadOnlyList<Card> Cards => _cards;

        // Only two cards allowed in a hand
        public void Add( Card card )
        {
            if ( _cards.Count >= 2 )
                throw new InvalidOperationException("Hand is already full.");

            _cards.Add(card);
        }

        public void ResetCards( )
        {
            _cards.Clear();
        }

        public override string ToString( )
            => string.Join(", ", _cards.Select(c => c.ToString()));
    }
}