using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    /// <summary>
    /// Responsible for poker rules and the flow of the game.
    /// </summary>
    class PokerGame
    {
        private List<Player> players;
        private Dealer dealer;
        private bool isRoundOver;
        private bool usedReplacement;
        private int moneyPool;
        private int currentBid;

        /// <summary> 
        /// Constructor.
        /// </summary>
        public PokerGame()
        {
            players = new List<Player>() { new Player("Edwin"), new Player("Marie"), new Player("Stella") };
            dealer = new Dealer();
            isRoundOver = false;
        }

        /// <summary> 
        /// Introduce the user and shuffle the order of players at the table.
        /// </summary>
        public void SitPlayers()
        {
            introduceThePlayer();
            shufflePlayersOrder();
        }

        /// <summary> 
        /// Main rounds control loop.
        /// </summary>
        public void PlayRound()
        {
            startNewRound();
            while (!isRoundOver)
            {
                Console.Clear();
                displayTable();

                foreach (var player in players)
                {
                    if (player.IsPlaying())
                        makeMove(player);
                }

                checkEndOfRound();

                if (isRoundOver && !usedReplacement)
                {
                    Console.WriteLine("\nProceed to card replacement stage.");
                    Console.ReadKey();
                    foreach (var player in players.Where(player => player.IsPlaying()))
                    {
                        Console.Clear();
                        int howManyToReplace = player.ReplaceCards();
                        dealer.DealReplacement(player, howManyToReplace);
                        usedReplacement = true;
                        isRoundOver = false;
                        displayTable();
                        Console.WriteLine($"{player.Name} exchanged {howManyToReplace} cards.");
                        Console.ReadKey();
                    }
                }
                if (!isRoundOver && !usedReplacement)
                {
                    Console.WriteLine("Deal next card.");
                    Console.ReadKey();
                    dealer.DealCard(players);
                }
            }
            var winner = determineTheWinner();
            Console.WriteLine($"{winner.Name} wins this round!");
            winner.Win(moneyPool);
            Console.ReadKey();
        }

        /// <summary> 
        /// Reset game's and players' round-level stats.
        /// </summary>
        private void startNewRound()
        {
            moneyPool = 0;
            currentBid = 0;
            usedReplacement = false;
            isRoundOver = false;
            foreach (var player in players)
            {
                player.DiscardHand();
                player.Play();
                player.HideCards();
            }
        }

        /// <summary>
        /// Get the player's name and place them at the table.
        /// </summary>
        private void introduceThePlayer()
        {
            Console.WriteLine("Welcome to the table! What is your name?");
            string playerName = Console.ReadLine();
            var player = new Player(playerName);
            player.SetUser();
            players.Add(player);
            Console.WriteLine();
        }

        /// <summary> 
        /// Randomize player's order at the table.
        /// </summary>
        private void shufflePlayersOrder()
        {
            var playersShuffled = new List<Player>();
            while (players.Count > 0)
            {
                var playerIndex = new Random().Next() % players.Count;
                playersShuffled.Add(players[playerIndex]);
                players.RemoveAt(playerIndex);
            }
            players = playersShuffled;
        }

        /// <summary>
        /// Display all players' hands, ranks and money; current bid and money pool.
        /// </summary>
        private void displayTable()
        {
            Console.WriteLine($"Money pool: ${moneyPool}\t\tCurrent bid: ${currentBid}\n\n");
            foreach (var player in players)
            {
                player.UpdateRanks();
                player.DisplayHand();
                player.DisplayRanks();
            }
        }

        /// <summary>
        /// Choose the appropriate action for current state of the game.
        /// </summary>
        private void makeMove(Player player)
        {
            // The player sees their options, in order to make a choice.
            if (player.IsUser())
            {
                // No one raised the bid.
                if (currentBid == 0)
                {
                    Console.WriteLine("[B] Bet\n[C] Check\n[F] Fold\n");
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.B:
                            currentBid = player.PlaceBid(currentBid, false);
                            moneyPool += currentBid;
                            break;
                        case ConsoleKey.C:
                            Console.WriteLine($"{player.Name} checks.");
                            break;
                        case ConsoleKey.F:
                            player.Fold();
                            break;
                    }
                }
                // The bid is already raised.
                else
                {
                    Console.WriteLine("[C] Call\n[R] Raise\n[F] Fold\n");
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.C:
                            currentBid = player.PlaceBid(currentBid, true);
                            moneyPool += currentBid;
                            break;
                        case ConsoleKey.R:
                            currentBid = player.PlaceBid(currentBid, false);
                            moneyPool += currentBid;
                            break;
                        case ConsoleKey.F:
                            player.Fold();
                            break;
                    }
                }
            }
            // Non-playable player takes a turn.
            else
            {
                // Players with at least a pair of cards won't ever fold.
                if (!player.GetRank().ContainsKey(Rank.HighCard))
                    currentBid = player.PlaceBid(currentBid, false);
                else
                {
                    // Players with 2 or 3 as a first card are most likely to fold.
                    // The probability of folding decreases with the amount of cards.
                    int highCardValue = (int)player.GetRank()[Rank.HighCard][0].Value + 1;
                    double foldProbability = Math.Pow(Math.Log(14 - highCardValue, 14), 10);
                    if (new Random().NextDouble() <= foldProbability / player.GetHand().Count)
                        player.Fold();
                    else
                        currentBid = player.PlaceBid(currentBid, false);
                }
                moneyPool += currentBid;
            }
            Console.ReadKey(true);
        }

        /// <summary>
        ///  Check whether the round should end (one player left or showdown).
        /// </summary>
        private void checkEndOfRound()
        {
            // Only one player left at the table.
            if (players.Where(player => player.IsPlaying()).Count() == 1)
            {
                isRoundOver = true;
                usedReplacement = true;
            }
            // The number of players with 5 cards is equal to the number of players, who are still playing.
            if (players.Where(player => player.GetHand().Count() == 5).Count() == players.Where(player => player.IsPlaying()).Count())
                isRoundOver = true;
        }

        /// <summary>
        /// Perform a showdown between players' hands to determine the round winner.
        /// </summary>
        private Player determineTheWinner()
        {
            #region ComparisonCommentary

            // Terminology:
            // bestRank : Best rank chosen from all hands, e.g. having Full House, One Pair and Two Pairs, Full House is the bestRank.
            // bestRankPlayer : All players with the bestRank, e.g. three players having (7 7 4 4), (Q Q 3 3), (A A 2 2).
            // highestCard : The highest leading card in given bestRank, e.g. King in (K K K 9 9) or 7 in (7 7 4 4).
            // winner : The player who currently possesses highestCard in bestRank, e.g. (A A 2 2) from two lines above.
            // player.GetRank().First().Value : Cards that form the player's rank.
            // First() and Last() : Here, those correspond to the smallest card in rank and highest card in rank, respectively.
            //
            // Approach:
            // The idea is to cycle through players' ranks and choose the highest rank in the game.
            // If more than one player possesses this rank, first player is assumed to be the winner
            // and all players are compared to the winner. First, highest cards from same ranks are
            // compared. If these are the same, as in (9♣ 9♦ 2♥ 2♦) vs (9♥ 9♠ 3♣ 3♦), either the
            // lower cards from the rank are compared, here 3 vs 2, or the highest remaining cards
            // are compared, as in (K K Q Q 3) vs (K K Q Q 4).

            #endregion

            foreach (var player in players)
                player.ShowCards();
            var bestRank = players.First().GetRank().First();
            for (int i = 1; i < players.Count; i++)
                if (players[i].GetRank().First().Key > bestRank.Key)
                    bestRank = players[i].GetRank().First();
            var bestRankPlayers = players.Where(player => player.GetRank().ContainsKey(bestRank.Key));

            if (bestRankPlayers.Count() == 1)
                return bestRankPlayers.First();

            // The last card in the rank is the one of the highest value due to hand sorting.
            var winner = bestRankPlayers.First();
            var highestCard = winner.GetRank().First().Value.Last();
            foreach (var player in bestRankPlayers)
            {
                switch (bestRank.Key)
                {
                    // Top card from these ranks hints at the winner.
                    // If top cards of those ranks are the same for some players, one next card is taken into account.
                    case Rank.HighCard:
                    case Rank.OnePair:
                        if (player.GetRank().First().Value.Last().Value > highestCard.Value)
                        {
                            winner = player;
                            highestCard = winner.GetRank().First().Value.Last();
                        }
                        else if (player.GetRank().First().Value.Last().Value == highestCard.Value)
                        {
                            // Compare the rest of the cards from the highest to lowest.
                            var restOfWinnerCards = winner.GetHand().Where(card => !bestRank.Value.Contains(card)).ToList();
                            var restOfPlayerCards = player.GetHand().Where(card => !bestRank.Value.Contains(card)).ToList();
                            for (int i = restOfPlayerCards.Count - 1; i >= 0; i--)
                            {
                                if (restOfPlayerCards[i].Value > restOfWinnerCards[i].Value)
                                {
                                    winner = player;
                                    highestCard = winner.GetRank().First().Value.Last();
                                    break;
                                }
                                else if (restOfPlayerCards[i].Value < restOfWinnerCards[i].Value)
                                    break;
                            }
                        }
                        break;
                    // Top card from these ranks detemine the winner unambiguously, if one deck is used in the game.
                    case Rank.ThreeOfAKind:
                    case Rank.FourOfAKind:
                    case Rank.Straight:
                    case Rank.StraightFlush:
                    case Rank.FullHouse:
                        if (player.GetRank().First().Value.Last().Value > highestCard.Value)
                        {
                            winner = player;
                            highestCard = winner.GetRank().First().Value.Last();
                        }
                        break;
                    // If the strong pairs are the same, compare the weaker pairs.
                    // If the weak pairs are the same, compare the remaining single cards.
                    case Rank.TwoPairs:
                        if (player.GetRank().First().Value.Last().Value > highestCard.Value)
                        {
                            winner = player;
                            highestCard = winner.GetRank().First().Value.Last();
                        }
                        else if (player.GetRank().First().Value.Last().Value == highestCard.Value)
                        {
                            if (player.GetRank().First().Value.First().Value > winner.GetRank().First().Value.First().Value)
                            {
                                winner = player;
                                highestCard = winner.GetRank().First().Value.Last();
                            }
                            else if (player.GetRank().First().Value.First().Value == winner.GetRank().First().Value.First().Value)
                            {
                                var remainingWinnerCard = winner.GetHand().Where(card => !bestRank.Value.Contains(card)).First();
                                var remainingPlayerCard = player.GetHand().Where(card => !bestRank.Value.Contains(card)).First();
                                if (remainingPlayerCard.Value > remainingWinnerCard.Value)
                                {
                                    winner = player;
                                    highestCard = winner.GetRank().First().Value.Last();
                                }
                            }
                        }
                        break;
                    // Top cards are compared. If they are the same, second top are compared, and so on.
                    case Rank.Flush:
                        var playerFlush = player.GetRank().First().Value;
                        var winnerFlush = winner.GetRank().First().Value;
                        for (int i = playerFlush.Count - 1; i >= 0; i--)
                        {
                            if (playerFlush[i].Value > winnerFlush[i].Value)
                            {
                                winner = player;
                                highestCard = winner.GetRank().First().Value.Last();
                                break;
                            }
                            else if (playerFlush[i].Value < winnerFlush[i].Value)
                                break;
                        }
                        break;
                }
            }
            return winner;
        }
    }
}
