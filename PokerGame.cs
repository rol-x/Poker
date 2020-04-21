using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class PokerGame
    {
        private List<Player> players;
        private Dealer dealer;
        private bool isGameRunning;
        private int moneyPool;
        private int currentBid;

        // Constructor.
        public PokerGame()
        {
            isGameRunning = true;
            players = new List<Player>() { new Player("Edwin"), new Player("Marie"), new Player("Stella") };
            dealer = new Dealer();
        }

        // Setting the game up; main control loop.
        public void Play()
        {
            moneyPool = 0;
            currentBid = 0;

            introduceThePlayer();
            shufflePlayersOrder();
            foreach (var player in players)
                player.Play();

            while (isGameRunning)
            {
                Console.Clear();
                displayTable();
                foreach (var player in players)
                    if (player.IsPlaying())
                        makeMove(player);
                // Only up to 5 cards.
                dealer.DealCard(players);
            }
        }

        // Displays all players' hands, ranks and money; current bid and money pool.
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

        // Get the player's name and place them at the table.
        private void introduceThePlayer()
        {
            Console.WriteLine("Welcome to the table! What is your name?");
            string playerName = Console.ReadLine();
            var player = new Player(playerName);
            player.SetUser();
            players.Add(player);
            Console.WriteLine();
        }

        // Randomize player's order at the table.
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

        // Choose the appropriate action for current state of the game.
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
    }
}
