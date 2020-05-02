using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox
{
    /// <summary>
    /// Standard poker ranking, which matches cards from hand into groups like pair, full house or straight flush.
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
        private Dictionary<Rank, List<Card>> rank;
        private List<Card> hand;
        public int money;
        private bool isUser;
        private bool isPlaying;
        private bool areCardsHidden;

        public double Aggressiveness { get; }
        public string Name { get; }
        public bool DidRaise { get; set; }
        public int BetValue { get; set; }
        public bool IsBankrupt
        {
            get
            {
                if (money > 0)
                    return false;
                else
                {
                    isPlaying = false;
                    return true;
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Player(String name)
        {
            Name = name;
            money = 10000;
            hand = new List<Card>();
            Aggressiveness = 0.5 + 0.4 * (new Random().NextDouble() - 0.5);
            rank = new Dictionary<Rank, List<Card>>();
            isUser = false;
            areCardsHidden = true;
            DidRaise = false;
            BetValue = 0;
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
        /// Player equates their contribution to the money pool with the current bid.
        /// </summary>
        /// <param name="currentBid"></param>
        public void Call(int currentBid)
        {
            int myBidRaise = currentBid - BetValue;
            money -= myBidRaise;
            BetValue += myBidRaise;
            if (currentBid == 0)
                Console.WriteLine($"{Name} checks.");
            else
                Console.WriteLine($"{Name} calls with ${myBidRaise}.");
        }

        /// <summary>
        /// Player chooses the amount of money they want to bet in the current turn. 
        /// </summary>
        /// <returns>Returns the amount by which the players raises the bid.</returns>
        public int Raise(int currentBid)
        {
            // What is the value of the new bid.
            int newBid;

            // The user takes turn.
            if (isUser)
            {
                // First bet should be at least $100. Next bid should be a least $10 more.
                int lowerBound = Math.Max(currentBid - (currentBid % 10) + 10, 100);
                Console.WriteLine("How much do you want to bet?");
                do
                {
                    var parseSuccess = Int32.TryParse(Console.ReadLine(), out newBid);
                    if (newBid > money)
                        Console.WriteLine("You lack funds to bid so high!");
                    else if (newBid < lowerBound)
                        Console.WriteLine($"You have to bid at least ${lowerBound}.");
                    else if (newBid % 10 != 0)
                        Console.WriteLine("The lowest denomination is $10.");
                    if (!parseSuccess || newBid > money || newBid < lowerBound || newBid % 10 != 0)
                    {
                        Console.ReadKey(true);
                        ConsoleEditor.ClearLastLines(2);
                    }
                } while (newBid > money || newBid < lowerBound || newBid % 10 != 0);
                ConsoleEditor.ClearLastLines(2);
            }

            // Non-user player takes turn.
            else
            {
                double playerRankCoefficient = ((double)rank.First().Key + 1) / 20.0;
                double highCardValueCoefficient = ((double)hand.Last().Value + 1) / 100.0;

                newBid = Math.Min(currentBid + 300, (int)Math.Ceiling(currentBid *
                    (1 + playerRankCoefficient + highCardValueCoefficient + (0.1 - new Random().NextDouble() * 0.2))));

                //0.15 + playerRankCoefficient + highCardValueCoefficient + (0.1 - new Random().NextDouble() * 0.2);

                // If non-user is the first bidder, take 100 as a base.
                if (currentBid == 0)
                    newBid = (int)(100 *
                        (1 + playerRankCoefficient + highCardValueCoefficient + (0.1 - new Random().NextDouble() * 0.3)));

                // Trim the last digit.
                newBid -= newBid % 10;
            }

            // How much does the player increase their bid.
            int myBidRaise = newBid - BetValue;

            // Take the chips into hand to throw them nonchalantly on the table.
            money -= myBidRaise;
            BetValue += myBidRaise;

            Console.WriteLine($"{Name} bets ${newBid}.");
            DidRaise = true;

            // Return the difference this player made to the current bid.
            return newBid - currentBid;
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
            Console.Clear();
            if (!isPlaying && !IsBankrupt)
                return 0;
            if (isUser)
            {
                int marker = 0;
                var selections = new Dictionary<int, bool>();
                for (int i = 0; i < hand.Count + 1; i++)
                    selections.Add(i, false);
                while (selections[hand.Count] == false)
                {
                    Console.Clear();
                    Console.WriteLine("Which cards would you like to replace? (Select up to 4 cards)");
                    Console.WriteLine();
                    for (int i = 0; i < hand.Count; i++)
                    {
                        if (selections[i])
                            Console.Write("→");
                        Console.Write($"\t{hand[i].CardSymbol()} ");
                        if (marker == i)
                            Console.Write("<");
                        Console.WriteLine();
                    }
                    if (selections.Where(pair => pair.Value == true).Count() == 0)
                        Console.Write("\tNone ");
                    else
                        Console.Write("\tReplace ");
                    if (marker == hand.Count)
                        Console.Write("<");
                    Console.WriteLine();

                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (marker > 0)
                                marker--;
                            break;
                        case ConsoleKey.DownArrow:
                            if (marker < hand.Count)
                                marker++;
                            break;
                        case ConsoleKey.Enter:
                            if (selections.Where(pair => pair.Value == true).Count() < 5)
                                selections[marker] = !selections[marker];
                            break;
                    }
                }
                do
                {
                    var key = selections.Where(pair => pair.Value == true && pair.Key != 5).Last().Key;
                    hand.RemoveAt(key);
                    selections.Remove(key);
                } while (selections.Where(pair => pair.Value == true).Count() > 1);
                return 5 - hand.Count;
            }
            else
            {
                var uselessCards = hand.Where(card => rank.Where(pair => pair.Value.Contains(card)).Count() == 0);
                hand.RemoveAll(card => uselessCards.Contains(card));
                return 5 - hand.Count;
            }
        }

        /// <summary>
        /// Add to player's money.
        /// </summary>
        public void Win(int moneyPrize)
        {
            money += moneyPrize;
        }

        /// <summary>
        /// Return the amount of money player possesses.
        /// </summary>
        public int GetMoney()
        {
            return money;
        }

        /// <summary>
        /// The player pays the entry fee in order to play.
        /// </summary>
        public int PayEntryFee(int fee)
        {
            money -= fee;
            return fee;
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
        /// Show player's cards to other players.
        /// </summary>
        public void ShowCards()
        {
            areCardsHidden = false;
        }

        /// <summary>
        /// Conceal the player's cards to other players.
        /// </summary>
        public void HideCards()
        {
            areCardsHidden = true;
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
            rank.Clear();
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
                    rank.Add(Rank.Flush, hand);
            }
            // Four of a kind, three of a kind, one pair and two pairs are self-explanatory.
            foreach (KeyValuePair<Value, int> valueCount in valueDictionary)
            {
                switch (valueCount.Value)
                {
                    case 4:
                        rank.Add(Rank.FourOfAKind, hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                        break;
                    case 3:
                        rank.Add(Rank.ThreeOfAKind, hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                        break;
                    case 2:
                        if (!rank.ContainsKey(Rank.OnePair))
                            rank.Add(Rank.OnePair, hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                        else
                        {
                            // We marge two OnePair ranks into TwoPairs and remove the earlier written OnePair.
                            List<Card> twoPairs = rank[Rank.OnePair];
                            twoPairs.AddRange(hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                            rank.Add(Rank.TwoPairs, twoPairs);
                            rank.Remove(Rank.OnePair);
                        }
                        break;
                }
            }
            // Full house is one pair and three of a kind.
            if (rank.ContainsKey(Rank.OnePair) && rank.ContainsKey(Rank.ThreeOfAKind))
            {
                rank.Add(Rank.FullHouse, hand);
                rank.Remove(Rank.ThreeOfAKind);
                rank.Remove(Rank.OnePair);
            }
            // Straight is a chain of 5 succesive cards (e.g. Four, Five, Six, Seven, Eight)
            bool areCardsConsecutive = true;
            for (int i = 0; i < hand.Count - 1; i++)
                if (hand[i].Value + 1 != hand[i + 1].Value)
                    areCardsConsecutive = false;
            if (areCardsConsecutive && hand.Count == 5)
                rank.Add(Rank.Straight, hand);
            // Straight flush is a straight and a flush.
            if (rank.ContainsKey(Rank.Straight) && rank.ContainsKey(Rank.Flush))
            {
                rank.Add(Rank.StraightFlush, hand);
                rank.Remove(Rank.Straight);
                rank.Remove(Rank.Flush);
            }
            // High card is the card of highest value in case of no other ranks.
            if (rank.Count == 0 && hand.Count != 0)
                rank.Add(Rank.HighCard, hand.GetRange(hand.Count - 1, 1));
        }

        /// <summary>
        /// Returns the rank present in player's hand.
        /// </summary>
        public Dictionary<Rank, List<Card>> GetRank()
        {
            return rank;
        }

        /// <summary>
        /// Display all cards in the player's hand, as card symbols.
        /// </summary>
        public void DisplayHand()
        {
            if (isPlaying)
                Console.WriteLine($"{Name}\t\t${money}");
            else if (IsBankrupt)
                Console.WriteLine($"{Name} (bankrupt)\t${money}");
            else
                Console.WriteLine($"{Name} (fold)\t${money}");
            if (hand.Count == 0)
                Console.WriteLine("No cards drawn yet.");
            else
                foreach (var card in hand)
                    Console.Write(card.CardSymbol() + " ");
            Console.WriteLine();
        }

        /// <summary>
        /// List all ranks in the player's hand, with cards that form them.
        /// </summary>
        public void DisplayRanks()
        {
            foreach (var rank in rank)
            {
                Console.Write($"{rank.Key}: ");
                foreach (var card in rank.Value)
                    Console.Write(card.CardSymbol() + " ");
            }
            if (rank.Count != 0)
                Console.WriteLine();
        }
    }
}
