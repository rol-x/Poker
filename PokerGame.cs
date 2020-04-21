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
            players = new List<Player>(){ new Player("Edwin"), new Player("Marie"), new Player("Stephanie")};
            dealer = new Dealer();
        }

        // Setting the game up; main control loop.
        public void Play()
        {
            introduceThePlayer();
            shufflePlayersOrder();
            moneyPool = 0;
            currentBid = 0;
            while (isGameRunning)
            {
                Console.Clear();
                displayPlayersHands();
                foreach (var player in players)
                    makeMove(player);
            }
        }

        // Get the player's name and place them at the table.
        private void introduceThePlayer()
        {
            Console.WriteLine("Welcome to the table! What is your name?");
            string playerName = Console.ReadLine();
            var player = new Player(playerName);
            player.SetPlayable();
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

        // Output to the console card and ranks in each player's hand.
        private void displayPlayersHands()
        {
            foreach (var player in players)
            {
                player.UpdateRanks();
                player.DisplayHand();
                player.DisplayRanks();
            }
        }

        // Choose the appropriate action for current state of the game.
        private void makeMove(Player player)
        {
            // Bet - raise current bid from to some amount
            if (currentBid == 0)
            {
                Console.WriteLine("[B] Bet\n[C] Check\n[F] Fold\n");
                switch (Console.ReadKey().Key)
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
            else
            {
                Console.WriteLine("[C] Call\n[R] Raise\n[F] Fold\n");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.C:
                        currentBid = player.PlaceBid(currentBid, true);
                        moneyPool += currentBid;
                        break;
                    case ConsoleKey.R:
                        currentBid = player.PlaceBid(currentBid, false);
                        break;
                    case ConsoleKey.F:
                        player.Fold();
                        break;
                }
            }
        }
    }
}
