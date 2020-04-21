using System;

namespace Sandbox
{
    enum Color
    {
        Spades,
        Clubs,
        Hearts,
        Diamonds
    }
    enum Value
    {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    class Card
    {
        public Color Color { get; }
        public Value Value { get; }

        public Card(short color, short value)
        {
            switch (color)
            {
                case 1:
                    Color = Color.Spades;
                    break;
                case 2:
                    Color = Color.Clubs;
                    break;
                case 3:
                    Color = Color.Hearts;
                    break;
                case 4:
                    Color = Color.Diamonds;
                    break;
            }

            switch (value)
            {
                case 1:
                    Value = Value.Ace;
                    break;
                case 2:
                    Value = Value.Two;
                    break;
                case 3:
                    Value = Value.Three;
                    break;
                case 4:
                    Value = Value.Four;
                    break;
                case 5:
                    Value = Value.Five;
                    break;
                case 6:
                    Value = Value.Six;
                    break;
                case 7:
                    Value = Value.Seven;
                    break;
                case 8:
                    Value = Value.Eight;
                    break;
                case 9:
                    Value = Value.Nine;
                    break;
                case 10:
                    Value = Value.Ten;
                    break;
                case 11:
                    Value = Value.Jack;
                    break;
                case 12:
                    Value = Value.Queen;
                    break;
                case 13:
                    Value = Value.King;
                    break;
            }
        }

        public Card(Color color, Value value)
        {
            Color = color;
            Value = value;
        }

        public string CardSymbol()
        {
            string cardSymbol = "";
            switch (Value)
            {
                case Value.Ace:
                    cardSymbol = "A";
                    break;
                case Value.Two:
                    cardSymbol = "2";
                    break;
                case Value.Three:
                    cardSymbol = "3";
                    break;
                case Value.Four:
                    cardSymbol = "4";
                    break;
                case Value.Five:
                    cardSymbol = "5";
                    break;
                case Value.Six:
                    cardSymbol = "6";
                    break;
                case Value.Seven:
                    cardSymbol = "7";
                    break;
                case Value.Eight:
                    cardSymbol = "8";
                    break;
                case Value.Nine:
                    cardSymbol = "9";
                    break;
                case Value.Ten:
                    cardSymbol = "10";
                    break;
                case Value.Jack:
                    cardSymbol = "J";
                    break;
                case Value.Queen:
                    cardSymbol = "Q";
                    break;
                case Value.King:
                    cardSymbol = "K";
                    break;

            }
            switch (Color)
            {
                case Color.Spades:
                    cardSymbol += "♠";
                    break;
                case Color.Clubs:
                    cardSymbol += "♣";
                    break;
                case Color.Hearts:
                    cardSymbol += "♥";
                    break;
                case Color.Diamonds:
                    cardSymbol += "♦";
                    break;
            }
            return cardSymbol;
        }
    }
}
