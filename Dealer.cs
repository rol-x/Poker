using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class Dealer
    {
        private Deck deck;

        public Dealer()
        {
            GetNewDeck();
        }
        public void DealCard(List<Player> players)
        {
            foreach (var player in players)
                if (player.IsPlaying())
                    player.DrawCard(deck.GetNextCard());
        }
        public void GetNewDeck()
        {
            deck = new Deck();
            deck.Shuffle();
        }
    }
}
