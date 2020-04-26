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
        public void Play()
        {
            startNewRound();
            while (!isRoundOver)
            {
                Console.Clear();
                displayTable();
                foreach (var player in players)
                    if (player.IsPlaying())
                        makeMove(player);
                checkEndOfRound();

                if (isRoundOver && !usedReplacement)
                    foreach (var player in players)
                    {
                        int howManyToReplace = player.ReplaceCards();
                        dealer.DealReplacement(player, howManyToReplace);
                        usedReplacement = true;
                    }
                if (!isRoundOver)
                    dealer.DealCard(players);
            }

            determineTheWinner();
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
                var handValue = getHandValue(player);
                if (handValue == 0)
                    currentBid = player.PlaceBid(currentBid, false);
                else
                {
                    #region formulaCommentary

                    // The following formula maps [32, 568243] hand-value interval into probability space logarithmically.
                    // Probability of folding for scores S(k) correspoding to the highest card h are shown below.
                    //
                    // h    S(h)        P(Fold)
                    // 2    32          81.4%
                    // 3    243         40.6%
                    // 4    1024        21.9%
                    // 5    3125        12.2%
                    // 6    7776        6.9%
                    // 7    16807       3.8%
                    // 8    32768       2%
                    // 9    59049       1%
                    // 10   100000      0.5%
                    // J    161051      0.2%
                    // Q    248832      0.05%
                    // K    371293    < 0.01%
                    // A    537824    < 0.0001%
                    // max  568243      0%

                    // The probability of folding with 2 as a high card, depending on aggressiveness:
                    // agg.     P(Fold | 2) 
                    // 0.0	    100%
                    // 0.1      97.3%
                    // 0.2      94.1%
                    // 0.3      90.5%
                    // 0.4      86.3%
                    // 0.5      81.4%
                    // 0.6      75.8%
                    // 0.7      69.0%
                    // 0.8      60.5%
                    // 0.9      48.8%
                    // 1.0      12.2%

                    #endregion
                    double foldProbability = Math.Pow(Math.Log(568243 / handValue, 568243 / (32 - 31.99 * player.Aggressiveness)), (2.5 + player.Aggressiveness));
                    if (new Random().NextDouble() < foldProbability)
                        player.Fold();
                    else
                        player.PlaceBid(currentBid, false);
                }
            }
            Console.ReadKey(true);
        }

        /// <summary>
        /// Calculate hand value based on single card values, when no rank is present.
        /// </summary>
        /// <returns>Returns an integer from 32 to 568243, and 0.</returns>
        private int getHandValue(Player player)
        {
            // Sorted hand can be treated as number in base-14 system.
            // Examples:
            // 6 K A    = 6^3 + 13^4 + 14^5
            // 8 9 10   = 8^3 + 9^4 + 10^5
            // 2 9 J    = 2^3 + 9^4 + 11^5
            // 4 5 A    = 4^3 + 5^4 + 14^5
            // K A      = 13^4 + 14^5
            // 9 Q      = 9^4 + 12^5
            // 5 7      = 5^4 + 7^5
            // A        = 14^5

            double value = 0;
            if (player.GetRanks().ContainsKey(Rank.HighCard))
                for (int i = player.GetHand().Count - 1; i >= 0; i--)
                    value += Math.Pow((int)player.GetHand()[i].Value, i + 1);
            return (int)value;
        }

        /// <summary>
        ///  Check whether the round should end (one player left or showdown).
        /// </summary>
        private void checkEndOfRound()
        {
            if (players.Where(player => player.IsPlaying() == true).Count() == 1)
                isRoundOver = true;
            if (players[0].GetHand().Count == 5 && usedReplacement == true)
                isRoundOver = true;
        }

        /// <summary>
        /// Perform a showdown between players' hands to determine the round winner.
        /// </summary>
        private void determineTheWinner()
        {
            // TODO
        }
    }
}
