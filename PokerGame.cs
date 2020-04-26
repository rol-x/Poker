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
            Console.WriteLine($"Money pool: {moneyPool}\t\tCurrent bid: {currentBid}\n\n");
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
                // Player has only high card; 20% fold chance.
                if (player.GetRanks().ContainsKey(Rank.HighCard))
                {
                    if (new Random().NextDouble() > 0.80)
                        player.Fold();
                    else
                        currentBid = player.PlaceBid(currentBid, false);
                }
                // Otherwise, player has 5% fold chance.
                else
                {
                    if (new Random().NextDouble() > 0.95)
                        player.Fold();
                    else
                        currentBid = player.PlaceBid(currentBid, false);
                }
            }
            Console.ReadKey(true);
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
