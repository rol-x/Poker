﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    /// <summary>
    /// Responsible for poker rules and the flow of the game.
    /// </summary>
    static class PokerGame
    {
        static private List<Player> players;
        static private Dealer dealer;
        static private bool isRoundOver;
        static private bool usedReplacement;
        static private int moneyPool;
        static private int currentBid;
        static private int turnCount;
        static private int entryFee = 50;
        static private bool ghostMode = false;

        /// <summary>
        /// Initializes a new poker game for four players.
        /// </summary>
        static public void StartNewGame()
        {
            dealer = new Dealer();
            players = new List<Player>() { new Player("Edwin"), new Player("Marie"), new Player("Stella") };
            introduceThePlayer();
            shufflePlayersOrder();
            playGame();
        }
        
        /// <summary>
        /// Play poker rounds and remove bankrupt players, until one is victorious.
        /// </summary>
        static private void playGame()
        {
            while (players.Where(player => player.GetMoney() >= 0).Count() > 1)
            {
                dealer.GetNewDeck();
                playRound();
                for (int i = 0; i < players.Count; i++)
                    if (players[i].GetMoney() <= 0)
                        players.Remove(players[i]);
                entryFee += 50;
                if (userLost() && !ghostMode)
                    chooseAfterLife();
            }
            finishTheGame();
        }

        /// <summary> 
        /// Round control loop, dealing cards and taking bets.
        /// </summary>
        static private void playRound()
        {
            startNewRound();
            while (!isRoundOver)
            {
                resetTheBid();
                displayTable();

                // -1 acts as an flag indicating the player didn't make any move yet.
                var playersBids = new Dictionary<Player, int>();
                foreach (var player in players.Where(player => player.IsPlaying()))
                    playersBids.Add(player, -1);

                int playerIndex = 0;
                if (turnCount != 0)
                    do
                    {
                        // If the player folded, skip to the next player.
                        if (!players[playerIndex].IsPlaying())
                        {
                            playerIndex = (playerIndex + 1) % players.Count;
                            continue;
                        }

                        // If the players acts for the first time in turn, reset the inact flag.
                        if (playersBids[players[playerIndex]] == -1)
                            playersBids[players[playerIndex]] = 0;

                        // Choose to fold, raise or call.
                        if (players[playerIndex].IsUser())
                            makeMoveUser(players[playerIndex]);
                        else
                            makeMoveComputer(players[playerIndex]);

                        // Update player's contribution to the money pool.
                        playersBids[players[playerIndex]] = players[playerIndex].BetValue;

                        // Players who recently folded or went bankrupt, need not to be taken into account during bidding anymore.
                        if (!players[playerIndex].IsPlaying() && playersBids.ContainsKey(players[playerIndex]))
                            playersBids.Remove(players[playerIndex]);
                        if (players[playerIndex].IsBankrupt && playersBids.ContainsKey(players[playerIndex]))
                            playersBids.Remove(players[playerIndex]);

                        updateBidDisplay();

                        // Cycle through all players.
                        playerIndex = (playerIndex + 1) % players.Count;
                        if (playerIndex == 0)
                            displayTable();

                    } while (!areBidsEqual(playersBids));

                checkEndOfRound();

                if (isRoundOver && !usedReplacement)
                    replacementStage();

                if (!isRoundOver && !usedReplacement)
                    dealCard();
            }
            displayTableFinish();
            finishTheRound();
        }

        /// <summary> 
        /// Reset game's and players' round-level stats.
        /// </summary>
        static private void startNewRound()
        {
            moneyPool = 0;
            turnCount = 0;
            usedReplacement = false;
            isRoundOver = false;

            foreach (var player in players)
            {
                player.DiscardHand();
                player.Play();
                if (!player.IsUser() && !ghostMode)
                    player.HideCards();
            }
        }

        /// <summary>
        /// Get the player's name and place them at the table.
        /// </summary>
        static private void introduceThePlayer()
        {
            Console.WriteLine("Welcome to the table! What is your name?");
            string playerName = Console.ReadLine().Trim(' ');
            var player = new Player(playerName);
            player.SetUser();
            player.ShowCards();
            players.Add(player);
            Console.WriteLine();
        }

        /// <summary> 
        /// Randomize player's order at the table.
        /// </summary>
        static private void shufflePlayersOrder()
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
        /// Display players' names, quantity of cards and money; current bid and money pool.
        /// </summary>
        static private void displayTable()
        {
            Console.Clear();
            Console.WriteLine($"Money pool: ${moneyPool}\t\tCurrent bid: ${currentBid}\n");
            foreach (var player in players)
            {
                player.UpdateRanks();
                player.DisplayHand();
                Console.WriteLine($"Bet: ${player.BetValue}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Display all players' cards, ranks, money and money pool.
        /// </summary>
        static private void displayTableFinish()
        {
            unhidePlayersCards();
            Console.Clear();
            Console.WriteLine($"Money pool: ${moneyPool}\n");
            foreach (var player in players)
            {
                player.UpdateRanks();
                player.DisplayHand();
                player.DisplayRanks();
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Dynamically update first console line containing current bid size and money pool value.
        /// </summary>
        static private void updateBidDisplay()
        {
            var cursorLeft = Console.CursorLeft;
            var cursorTop = Console.CursorTop;

            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Money pool: ${moneyPool}\t\tCurrent bid: ${currentBid}\n\n");

            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.ReadKey(true);
        }

        /// <summary>
        /// Choose to raise the bid, call or fold.
        /// </summary>
        static private void makeMoveUser(Player player)
        {
            // The player is going bankrupt.
            if (player.GetMoney() <= currentBid)
            {
                Console.WriteLine($"Bet ${player.GetMoney()}.");
                Console.ReadKey(true);
                ConsoleEditor.ClearLastLines(1);
                Console.WriteLine($"{player.Name} bets ${player.GetMoney()}.");
                moneyPool += player.PayEntryFee(player.GetMoney());
            }

            // The player is making a normal move.
            else
            {
                int bidRaise = 0;
                bool repeat = true;
                // No one raised the bid.
                if (currentBid == 0)
                {
                    while (repeat)
                    {
                        Console.WriteLine("[B] Bet\n[C] Check\n[F] Fold\n");
                        var key = Console.ReadKey(true).Key;
                        ConsoleEditor.ClearLastLines(4);
                        switch (key)
                        {
                            // A bet is a raise, when the bid is 0.
                            case ConsoleKey.B:
                                bidRaise = player.Raise(currentBid);
                                currentBid += bidRaise;
                                moneyPool += currentBid;
                                repeat = false;
                                break;
                            // A check is a call, when the bid is 0.
                            case ConsoleKey.C:
                                player.Call(currentBid);
                                repeat = false;
                                break;
                            case ConsoleKey.F:
                                player.Fold();
                                repeat = false;
                                break;
                        }
                    }
                }
                // The bid is already raised.
                else
                {
                    while (repeat)
                    {
                        Console.WriteLine("[C] Call\n[R] Raise\n[F] Fold\n");
                        var key = Console.ReadKey(true).Key;
                        ConsoleEditor.ClearLastLines(4);
                        switch (key)
                        {
                            case ConsoleKey.C:
                                player.Call(currentBid);
                                moneyPool += currentBid;
                                repeat = false;
                                break;
                            case ConsoleKey.R:
                                bidRaise = player.Raise(currentBid);
                                currentBid += bidRaise;
                                moneyPool += currentBid;
                                repeat = false;
                                break;
                            case ConsoleKey.F:
                                player.Fold();
                                repeat = false;
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Perform a raise, call or fold, based on non-user player's situation in game.
        /// </summary>
        /// <returns></returns>
        static private void makeMoveComputer(Player player)
        {
            // The player is going bankrupt.
            if (player.GetMoney() <= currentBid)
            {
                Console.WriteLine($"{player.Name} bets ${player.GetMoney()}.");
                moneyPool += player.PayEntryFee(player.GetMoney());
            }

            // The player is making a normal move.
            else
            {
                int bidRaise = 0;
                if (new Random().NextDouble() <= foldProbability(player, currentBid))
                    player.Fold();
                else if (new Random().NextDouble() < raiseProbability(player, currentBid))
                    bidRaise = player.Raise(currentBid);
                else
                    player.Call(currentBid);

                currentBid += bidRaise;
                moneyPool += currentBid;
            }
        }

        /// <summary>
        /// Set all bids to zero.
        /// </summary>
        static private void resetTheBid()
        {
            currentBid = 0;
            foreach (var player in players)
            {
                player.DidRaise = false;
                player.BetValue = 0;
            }
        }

        /// <summary>
        /// Each player still in game is dealt a card. Advances turn by one.
        /// </summary>
        static private void dealCard()
        {
            if (turnCount == 0)
            {
                Console.WriteLine("Pay the entry fee to draw the first card.");
                Console.ReadKey(true);
                foreach (var player in players)
                    moneyPool += player.PayEntryFee(entryFee);
            }
            else
            {
                Console.WriteLine("Draw the next card.");
                Console.ReadKey(true);
            }
            dealer.DealCard(players);
            turnCount++;
        }

        /// <summary>
        /// Stage where players can replace up to four of their cards.
        /// </summary>
        static private void replacementStage()
        {
            Console.WriteLine("\nProceed to card replacement stage.");
            Console.ReadKey();
            foreach (var player in players.Where(player => player.IsPlaying() || player.IsBankrupt))
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

        /// <summary>
        /// Compute the probability of folding for non-user players.
        /// </summary>
        /// <returns>Returns the probability of folding the hand.</returns>
        static private double foldProbability(Player player, int currentBid)
        {
            double probability;
            // Players with at least a pair of cards will rarely fold. Unless scared by the bid.
            if (!player.GetRank().ContainsKey(Rank.HighCard))
            {
                int playerRank = (int)player.GetRank().First().Key + 3;
                probability = (0.1 / playerRank) + (currentBid / player.GetMoney() / 2);
            }
            else
            {
                // Players with 2 or 3 as a first card are most likely to fold.
                // The probability of folding decreases with the amount of cards.
                int highCardValue = (int)player.GetRank()[Rank.HighCard][0].Value + 1;
                probability = Math.Pow(Math.Log(14 - highCardValue, 14), 15 + 10 * player.Aggressiveness);
            }

            return probability;
        }

        /// <summary>
        /// Compute the probability of raising for non-user players.
        /// </summary>
        /// <returns>Returns the probability of folding the hand.</returns>
        static private double raiseProbability(Player player, int currentBid)
        {
            double probability;
            double highCardValueCoefficient = (double)player.GetRank().First().Value.Last().Value / 100;
            double playerRankCoefficient = (double)player.GetRank().First().Key / 20;

            // Players with no rank will rarely raise. Unless they bluff; or count on their high card.
            // The players with higher ranks are more likely to raise. Bluffing included.
            //
            // 10% base probability, 5 % for each rank level, 1% for each highest card value level, +-10% from bluffing.
            // Minimum (2♠) => 15% + 0% + 0% - 10% = 5%
            // Maximum (2♠) => 15% + 0% + 0% + 10% = 25%
            // Minimum (7♦ 7♥ 7♣) => 15% + 15% + 5% - 10% = 25%
            // Maximum (7♦ 7♥ 7♣) => 15% + 15% + 5% + 10% = 45%
            // Minimum (10♠ J♠ Q♠ K♠ A♠) => 15% + 40% + 12% - 10% = 57%
            // Maximum (10♠ J♠ Q♠ K♠ A♠) => 15% + 40% + 12% + 10% = 77%

            probability = 0.15 + playerRankCoefficient + highCardValueCoefficient + (0.1 - new Random().NextDouble() * 0.2);

            // If the player already raised, the chances are decimated.
            return player.DidRaise ? 0.1 * probability : probability;
        }

        /// <summary>
        /// Return if all the players bid the same amount.
        /// </summary>
        static private bool areBidsEqual(Dictionary<Player, int> bids)
        {
            return bids.Where(pair => pair.Value != currentBid).Count() == 0 ? true : false;
        }

        /// <summary>
        ///  Check whether the round should end (one player left or showdown).
        /// </summary>
        static private void checkEndOfRound()
        {
            // Players who didn't fold.
            var eligiblePlayers = players.Where(player => player.IsPlaying() || player.IsBankrupt);

            // Only one player left at the table.
            if (eligiblePlayers.Count() == 1)
            {
                isRoundOver = true;
                usedReplacement = true;
            }
            // All playing players have 5 cards
            if (eligiblePlayers.Where(player => player.GetHand().Count() == 5).Count() == eligiblePlayers.Count())
                isRoundOver = true;
        }

        /// <summary>
        /// Perform a showdown between players' hands to determine the round winner.
        /// </summary>
        static private Player determineTheWinner()
        {
            #region ComparisonCommentary

            // Terminology:
            // bestRank : Best rank chosen from all hands, e.g. having Full House, One Pair and Two Pairs, Full House is the bestRank.
            // bestRankPlayer : All players with the bestRank, e.g. three players having (7 7 4 4), (Q Q 3 3), (A A 2 2).
            // highestCard : The highest leading card in given bestRank, e.g. King in (K K K 9 9) or 7 in (7 7 4 4).
            // winner : The player who currently possesses highestCard in bestRank, e.g. (A A 2 2) from two lines above.
            // player.GetRank().First().Value : Cards that form the player's rank.
            // First() and Last() : Here, these correspond to the lowest and highest cards, respectively.
            //
            // Approach:
            // The idea is to cycle through players' ranks and choose the highest rank in the game.
            // If more than one player possesses this rank, first player is assumed to be the winner
            // and all players are compared to the winner. First, highest cards from same ranks are
            // compared. If these are the same, as in (9♣ 9♦ 2♥ 2♦) vs (9♥ 9♠ 3♣ 3♦), either the
            // lower cards from the rank are compared, here 3 vs 2, or the highest remaining cards
            // are compared, as in (K K Q Q 3) vs (K K Q Q 4).

            #endregion

            // Players eligible for winning.
            var eligiblePlayers = players.Where(player => player.IsPlaying() || player.IsBankrupt).ToList();

            // If all other players folded, the remaining player is the winner.
            if (eligiblePlayers.Count() == 1)
                return eligiblePlayers.First();

            // Display all players' hands.
            foreach (var player in eligiblePlayers)
                player.ShowCards();

            // Find the best rank among players.
            var bestRank = eligiblePlayers.First().GetRank().First();
            for (int i = 1; i < eligiblePlayers.Count; i++)
                if (eligiblePlayers[i].GetRank().First().Key > bestRank.Key)
                    bestRank = eligiblePlayers[i].GetRank().First();

            // Group players with the determined best rank.
            var bestRankPlayers = eligiblePlayers.Where(player => player.GetRank().ContainsKey(bestRank.Key));

            // The only player with the best rank is the winner.
            if (bestRankPlayers.Count() == 1)
                return bestRankPlayers.First();

            // All players with the best rank take part in hands comparison.
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
                            for (int i = restOfWinnerCards.Count - 1; i >= 0; i--)
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

        /// <summary>
        /// Make all players' cards visible.
        /// </summary>
        static private void unhidePlayersCards()
        {
            foreach (var player in players.Where(player => player.IsPlaying()))
                player.ShowCards();
        }

        /// <summary>
        /// Returns whether the user lost all money and is playing no more.
        /// </summary>
        static private bool userLost()
        {
            foreach (var player in players)
                if (player.IsUser())
                    return false;
            return true;
        }

        /// <summary>
        /// Let the user who lost choose a form of continuing the gameplay.
        /// </summary>
        static private void chooseAfterLife()
        {
            Console.Clear();
            Console.WriteLine("POKER HEAVEN\n");
            Console.WriteLine("It seems like you lost all your money. What would you wish to do?");
            Console.WriteLine("[R] Reincarnate into one of the players");
            Console.WriteLine("[G] Become a ghostly spectator (spooky)");
            Console.WriteLine("[V] Vanish straight into non-existance");

            ConsoleKeyInfo choice;
            do
            {
                choice = Console.ReadKey(true);
                switch (choice.Key)
                {
                    case ConsoleKey.R:
                        int playerNumber = new Random().Next() % players.Count;
                        players[playerNumber].SetUser();
                        players[playerNumber].ShowCards();
                        break;
                    case ConsoleKey.G:
                        ghostMode = true;
                        break;
                    case ConsoleKey.V:
                        Environment.Exit(0);
                        break;
                }
            } while (choice.Key != ConsoleKey.R && choice.Key != ConsoleKey.G && choice.Key != ConsoleKey.V);
        }

        /// <summary>
        /// Display the winner and hand them the money from given round.
        /// </summary>
        static private void finishTheRound()
        {
            var winner = determineTheWinner();
            
            Console.WriteLine($"{winner.Name} wins this round!");
            winner.Win(moneyPool);
            Console.ReadKey();
        }

        /// <summary>
        /// Finish the game, when only one player is left with the money.
        /// </summary>
        static private void finishTheGame()
        {
            Console.Clear();
            Console.WriteLine($"{players.First().Name} is the winner! Congratulations!");
            Console.ReadKey(true);
        }
    }
}
