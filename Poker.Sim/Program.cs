﻿using Poker.Core;
using Poker.Core.Models;
using System.Diagnostics;

namespace Poker.ConsoleApp
{
    static class Program
    {
        static void Main( string[] args )
        {
            var engine = new PokerEngine();
            const int MAX_HANDS = 10000;
            int handNum = 0;

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("🎲  Poker simulation starting…\n");

            // Start the stopwatch before playing any hands
            var stopwatch = Stopwatch.StartNew();

            while ( engine.Players.Count > 1 && handNum < MAX_HANDS )
            {
                handNum++;
                Console.WriteLine($"── Hand #{handNum} ─────────────────────────────────");

                // play one hand, get the result
                var result = engine.PlayHand();

                // 1) show the board
                Console.Write("Board: ");
                PrintCards(result.Board);
                Console.WriteLine();

                // 2) show each player's hole cards + best 5-card hand
                foreach ( var (pl, hv) in result.ShowdownHands )
                {
                    Console.Write($"{pl.Name,-10} | Hole: ");
                    PrintCards(pl.Hand.Cards);
                    Console.Write("  → Best: ");
                    Console.Write($"{hv.Rank} ");
                    PrintCards(hv.Cards);
                    Console.WriteLine();
                }

                // 3) announce winner(s)
                var names = string.Join(", ", result.Winners.Select(w => w.Name));
                Console.WriteLine($"\n🎉 Winner{(result.Winners.Count > 1 ? "s" : "")}: {names}\n");

                // 4) show updated stacks
                PrintStacks(engine);
                Console.WriteLine();
            }

            // Stop the timer
            stopwatch.Stop();

            Console.WriteLine("⏹  Game over!\nFinal standings:");
            PrintStacks(engine);
            Console.WriteLine();

            // Calculate total and average times
            TimeSpan totalTime = stopwatch.Elapsed;
            double averageMsPerHand = handNum > 0
                ? totalTime.TotalMilliseconds / handNum
                : 0.0;

            // Pretty‐print the total time as hh:mm:ss.fff
            string prettyTotal = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}",
                totalTime.Hours,
                totalTime.Minutes,
                totalTime.Seconds,
                totalTime.Milliseconds);

            Console.WriteLine($"⏱  Total time: {prettyTotal}");
            Console.WriteLine($"⚖️  Average time per hand: {averageMsPerHand:F2} ms");
        }

        static void PrintCards( IEnumerable<Card> cards )
        {
            // e.g. render “Ah” for Ace of hearts, “10♠”, etc.
            foreach ( var c in cards )
            {
                char value = c.Value switch
                {
                    Values.Two => '2',
                    Values.Three => '3',
                    Values.Four => '4',
                    Values.Five => '5',
                    Values.Six => '6',
                    Values.Seven => '7',
                    Values.Eight => '8',
                    Values.Nine => '9',
                    Values.Ten => 'T',
                    Values.Jack => 'J',
                    Values.Queen => 'Q',
                    Values.King => 'K',
                    Values.Ace => 'A',
                    _ => '?'
                };
                char suit = c.Suit switch
                {
                    Suits.Hearts => '♥',
                    Suits.Diamonds => '♦',
                    Suits.Clubs => '♣',
                    Suits.Spades => '♠',
                    _ => '?'
                };
                Console.Write($"{value}{suit} ");
            }
        }

        static void PrintStacks( PokerEngine engine )
        {
            Console.WriteLine($"{"Player",-10} | Chips");
            Console.WriteLine(new string('─', 20));
            foreach ( var p in engine.Players.OrderByDescending(p => p.Chips) )
                Console.WriteLine($"{p.Name,-10} | {p.Chips}");
        }
    }
}
