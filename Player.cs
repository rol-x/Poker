using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox
{
    /// <summary>
    /// Standard poker ranking, matching cards from hand together into groups like pair, full house or straight flush.
    /// </summary>
    enum Rank
    {
        HighCard,
        OnePair,
        TwoPairs,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush
    }

    /// <summary>
    /// Represents a player at the table, controlled either by a computer or the user.
    /// </summary>
    class Player
    {
        private Dictionary<Rank, List<Card>> ranks;
        private List<Card> hand;
        private int money;
        private bool isUser;
        private bool isPlaying;
        private double aggressiveness;

        public string Name { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Player(String name)
        {
            Name = name;
            money = 10000;
            hand = new List<Card>();
            aggressiveness = 0.5 + 0.2 * (new Random().NextDouble() - 0.5);
            ranks = new Dictionary<Rank, List<Card>>();
            isUser = false;
        }
        
        /// <summary>
        /// Make this player the user. 
        /// </summary>
        public void SetUser()
        {
            isUser = true;
        }

        /// <summary>
        /// Returns if the player is the user. 
        /// </summary>
        public bool IsUser()
        {
            return isUser;
        }

        /// <summary>
        /// Add a single card to the hand.
        /// </summary>
        public void DrawCard(Card card)
        {
            hand.Add(card);
        }

        /// <summary>
        /// Player chooses the amount of money they want to bet in current round. 
        /// </summary>
        /// <returns>Returns the bid value chosen by player.</returns>
        public int PlaceBid(int currentBid, bool doesCall)
        {
            int bidSize;
            if (isUser)
            {
                if (doesCall)
                    bidSize = currentBid;
                else
                {
                    // First bet should be at least 100.
                    int lowerBound = Math.Max(currentBid, 100);
                    Console.WriteLine("How much money do you want to bet? ");
                    do
                    {
                        bidSize = Int32.Parse(Console.ReadLine());
                        if (bidSize > money)
                            Console.WriteLine("You lack funds to bid so high!");
                        if (bidSize < lowerBound)
                            Console.WriteLine($"You have to bid at least {lowerBound}.");
                    } while (bidSize > money || bidSize < lowerBound);
                }
            }
            else
            {
                // Non-playable character bidding logic.
                // First bet probability: fixed 30%.
                // First bet size: any number from 0 to 50, times 10.
                // Minimal bid: current bid
                // Maximal bid: current bid * (aggressiveness + 1)
                // Minimal bid probability: 1 - aggressiveness
                // Aggressiveness: 0 - always calls, 0.5 - calls and raises equally, 1 - only raises. Linear.
                //
                // Number(10 / 3) is normalizing[0, 0.3) interval into[0, 1).

                if (currentBid == 0)
                    currentBid = 10 * (int)(50 * (10 / 3) * Math.Max(0, new Random().NextDouble() - 0.7));

                bidSize = (int)Math.Max(currentBid, currentBid * (aggressiveness + new Random().NextDouble()));
            }

            money -= bidSize;

            if (doesCall)
                Console.WriteLine($"{Name} calls with {currentBid}");
            else if (bidSize == 0)
                Console.WriteLine($"{Name} checks.");
            else
                Console.WriteLine($"{Name} bets {bidSize}.");

            return bidSize;
        }

        /// <summary>
        /// The player decides to fold (i.e. surrender) his current hand, excluding him from the rest of the round.
        /// </summary>
        public void Fold()
        {
            isPlaying = false;
            Console.WriteLine($"{Name} folds.");
        }

        /// <summary>
        /// Choose to replace from one to four cards.
        /// </summary>
        /// <returns>Returns the number of cards to replace.</returns>
        public int ReplaceCards()
        {
            if (!isPlaying)
                return 0;
            if (isUser)
            {
                Console.WriteLine("Which cards would you like to replace?");
                Console.WriteLine("0 for none. 1, 2, 3, 4, 5 for each card.");
                var cardsToReplace = Console.ReadLine().Split(' ').ToArray();
                Console.WriteLine(cardsToReplace);
                // TODO
                return 0;
            }
            else
                return new Random().Next() % 5;
        }

        /// <summary>
        /// The player is included in the new round.
        /// </summary>
        public void Play()
        {
            isPlaying = true;
        }

        /// <summary>
        /// Return the information whether a player is still playing (didn't fold nor did they go bankrupt).
        /// </summary>
        public bool IsPlaying()
        {
            return isPlaying;
        }

        /// <summary>
        /// Return the cards in the player's hand.
        /// </summary>
        public List<Card> GetHand()
        {
            return hand;
        }

        /// <summary>
        /// Sort cards in a hand by their value.
        /// </summary>
        public void SortHand()
        {
            for (int i = 0; i < hand.Count - 1; i++)
                for (int j = i + 1; j < hand.Count; j++)
                    if (hand[i].Value > hand[j].Value)
                    {
                        var temp = hand[j];
                        hand[j] = hand[i];
                        hand[i] = temp;
                    }
        }

        /// <summary>
        /// Put all cards from hand away.
        /// </summary>
        public void DiscardHand()
        {
            hand.Clear();
        }

        /// <summary>
        /// Search cards in hand for ranks, like pair, three of a kind or flush.
        /// </summary>
        public void UpdateRanks()
        {
            ranks.Clear();
            SortHand();
            var colorDictionary = new Dictionary<Color, int>();
            var valueDictionary = new Dictionary<Value, int>();
            foreach (var card in hand)
            {
                if (!colorDictionary.ContainsKey(card.Color))
                    colorDictionary.Add(card.Color, 1);
                else
                    colorDictionary[card.Color]++;

                if (!valueDictionary.ContainsKey(card.Value))
                    valueDictionary.Add(card.Value, 1);
                else
                    valueDictionary[card.Value]++;
            }

            // Flush is five cards in one color.
            foreach (var colorCount in colorDictionary)
            {
                if (colorCount.Value == 5)
                    ranks.Add(Rank.Flush, hand);
            }
            // Four of a kind, three of a kind, one pair and two pairs are self-explanatory.
            foreach (KeyValuePair<Value, int> valueCount in valueDictionary)
            {
                switch (valueCount.Value)
                {
                    case 4:
                        ranks.Add(Rank.FourOfAKind, hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                        break;
                    case 3:
                        ranks.Add(Rank.ThreeOfAKind, hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                        break;
                    case 2:
                        if (!ranks.ContainsKey(Rank.OnePair))
                            ranks.Add(Rank.OnePair, hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                        else
                        {
                            // We marge two OnePair ranks into TwoPairs and remove the earlier written OnePair.
                            List<Card> twoPairs = ranks[Rank.OnePair];
                            twoPairs.AddRange(hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                            ranks.Add(Rank.TwoPairs, twoPairs);
                            ranks.Remove(Rank.OnePair);
                        }
                        break;
                }
            }
            // Full house is one pair and three of a kind.
            if (ranks.ContainsKey(Rank.OnePair) && ranks.ContainsKey(Rank.ThreeOfAKind))
            {
                ranks.Add(Rank.FullHouse, hand);
                ranks.Remove(Rank.ThreeOfAKind);
                ranks.Remove(Rank.OnePair);
            }
            // Straight is a chain of 5 succesive cards (e.g. Four, Five, Six, Seven, Eight)
            bool areCardsConsecutive = true;
            for (int i = 0; i < hand.Count - 1; i++)
                if (hand[i].Value + 1 != hand[i + 1].Value)
                    areCardsConsecutive = false;
            if (areCardsConsecutive && hand.Count == 5)
                ranks.Add(Rank.Straight, hand);
            // Straight flush is a straight and a flush.
            if (ranks.ContainsKey(Rank.Straight) && ranks.ContainsKey(Rank.Flush))
            {
                ranks.Add(Rank.StraightFlush, hand);
                ranks.Remove(Rank.Straight);
                ranks.Remove(Rank.Flush);
            }
            // High card is the card of highest value in case of no other ranks.
            if (ranks.Count == 0 && hand.Count != 0)
                ranks.Add(Rank.HighCard, hand.GetRange(hand.Count - 1, 1));
        }

        /// <summary>
        /// Returns ranks present in player's hand.
        /// </summary>
        public Dictionary<Rank, List<Card>> GetRanks()
        {
            return ranks;
        }

        /// <summary>
        /// Display all cards in the player's hand, as card symbols.
        /// </summary>
        public void DisplayHand()
        {
            if (isPlaying)
                Console.WriteLine($"{Name}\t\tCash: {money}");
            else
                Console.WriteLine($"{Name} (fold)\tCash: {money}");
            foreach (var card in hand)
                Console.Write(card.CardSymbol() + " ");
            Console.WriteLine();
        }

        /// <summary>
        /// List all ranks in the player's hand, with cards that form them.
        /// </summary>
        public void DisplayRanks()
        {
            foreach (var rank in ranks)
            {
                Console.Write($"{rank.Key}: ");
                foreach (var card in rank.Value)
                    Console.Write(card.CardSymbol() + " ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

    }
}
