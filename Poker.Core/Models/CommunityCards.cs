namespace Poker.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CommunityCards
    {
        private readonly List<Card> _cards = new();

        public IReadOnlyList<Card> Cards => _cards;

        // Up to five community cards
        public void Add( Card card )
        {
            if ( _cards.Count >= 5 )
                throw new InvalidOperationException("Community cards are already full.");

            _cards.Add(card);
        }

        public override string ToString( )
            => string.Join(", ", _cards.Select(c => c.ToString()));
    }
}