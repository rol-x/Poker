using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    /// <summary>
    /// Simulates a deck of 52 cards.
    /// </summary>
    class Deck
    {
        private List<Card> cards;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Deck()
        {
            cards = new List<Card>();
            for (short color = 1; color <= 4; color++)
                for (short value = 1; value <= 13; value++)
                    cards.Add(new Card(color, value));
        }

        /// <summary>
        /// Reorder the cards randomly.
        /// </summary>
        public void Shuffle()
        {
            var shuffledCards = new List<Card>();
            while (cards.Count > 0)
            {
                // Select a card at random and add it to the new list, then remove this card from the original list.
                var cardIndex = new Random().Next() % cards.Count;
                shuffledCards.Add(cards[cardIndex]);
                cards.RemoveAt(cardIndex);
            }
            cards = shuffledCards;
        }

        /// <summary>
        /// Take and return the top card from the deck.
        /// </summary>
        public Card GetNextCard()
        {
            var nextCard = cards.First();
            cards.Remove(nextCard);
            return nextCard;
        }
    }
}
