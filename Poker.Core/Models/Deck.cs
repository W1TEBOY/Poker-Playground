namespace Poker.Core.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A standard 52-card deck.
    /// </summary>
    public class Deck : IEnumerable<Card>
    {
        private static readonly Random _globalRng = new Random();

        private readonly List<Card> _cards;
        private readonly Random _rng;

        /// <summary>
        /// How many cards remain in the deck.
        /// </summary>
        public int Count => _cards.Count;

        /// <summary>
        /// Create a new deck, automatically shuffled.
        /// </summary>
        /// <param name="rng">
        /// Optional RNG (for testing). Defaults to a shared global instance.
        /// </param>
        public Deck( Random rng = null )
        {
            _rng = rng ?? _globalRng;
            _cards = GenerateFreshDeck().ToList();
            _Shuffle();
        }

        /// <summary>
        /// Draw the top card. Throws if empty.
        /// </summary>
        public Card Draw( )
        {
            if ( _cards.Count == 0 )
                throw new InvalidOperationException("Deck is empty.");

            var lastIndex = _cards.Count - 1;
            var card = _cards[lastIndex];
            _cards.RemoveAt(lastIndex);
            return card;
        }

        /// <summary>
        /// Put the deck back to a full 52 cards (unshuffled).
        /// </summary>
        public void Reset( )
        {
            _cards.Clear();
            _cards.AddRange(GenerateFreshDeck());
            _Shuffle();
        }

        /// <summary>
        /// _Shuffle in place using Fisher-Yates.
        /// </summary>
        public void _Shuffle( )
        {
            for ( int i = _cards.Count - 1; i > 0; i-- )
            {
                int j = _rng.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        /// <summary>
        /// Peek at the top card without removing it.
        /// </summary>
        public Card Peek( )
        {
            if ( _cards.Count == 0 )
                throw new InvalidOperationException("Deck is empty.");
            return _cards[^1];
        }

        private static IEnumerable<Card> GenerateFreshDeck( )
        {
            return Enum.GetValues<Values>()
                       .SelectMany(v => Enum.GetValues<Suits>()
                                            .Select(s => new Card(v, s)));
        }

        /// <summary>
        /// Support foreach, LINQ, etc.
        /// </summary>
        public IEnumerator<Card> GetEnumerator( ) => _cards.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator( ) => GetEnumerator();

        public override string ToString( ) => string.Join(", ", _cards);
    }
}
