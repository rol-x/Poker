using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class Deck
    {
        private List<Card> cards;

        public Deck()
        {
            cards = new List<Card>();
            for (short color = 1; color <= 4; color++)
                for (short value = 1; value <= 13; value++)
                    cards.Add(new Card(color, value));
        }
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
        public Card GetNextCard()
        {
            var nextCard = cards.First();
            cards.Remove(nextCard);
            return nextCard;
        }
    }
}
