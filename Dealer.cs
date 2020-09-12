using System;
using System.Collections.Generic;

namespace Sandbox
{
    /// <summary>
    /// Handles card dealing, acts as a wrapper for the Deck class.
    /// </summary>
    class Dealer
    {
        private Deck deck;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Dealer()
        {
            GetNewDeck();
        }

        /// <summary>
        /// Deal one card from the deck to every player still in the game.
        /// </summary>
        public void DealCard(List<Player> players)
        {
            foreach (var player in players)
                if (player.IsPlaying())
                    player.DrawCard(deck.GetNextCard());
                else if (player.IsBankrupt)
                    player.DrawCard(deck.GetNextCard());
        }

        /// <summary>
        /// Deal replacement cards to a specific player.
        /// </summary>
        /// <param name="player">The player to deal the cards to.</param>
        /// <param name="count">Number of cards to deal.</param>
        public void DealReplacement(Player player, int count)
        {
            if (player.IsUser())
            {
                if (count == 1)
                    Console.Write("\nNew card: ");
                else
                    Console.Write("\nNew cards: ");
            }

            for (int i = 0; i < count; i++)
            {
                var newCard = deck.GetNextCard();
                if (player.IsUser())
                    Console.Write(newCard.CardSymbol() + " ");
                player.DrawCard(newCard);
            }

            if (player.IsUser())
                Console.ReadKey();
        }

        /// <summary>
        /// Equip brand new deck and shuffle the cards.
        /// </summary>
        public void GetNewDeck()
        {
            deck = new Deck();
            deck.Shuffle();
        }
    }
}
