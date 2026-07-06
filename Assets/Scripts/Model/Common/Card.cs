using Common;
using UnityEngine;
using static Model.Common.Enums;

namespace Model.Common {

    public struct CardPileData
    {
        public int Index;
        public PileKind Kind;

        public CardPileData(PileKind kind, int index) : this()
        {
            Kind = kind;
            Index = index;
        }
    }

    public class Card
    {
        private int _value;
        private Suit _suit;
        private bool _free;
        private bool _revealed;

        public Card(Suit suit, int value)
        {
            _suit = suit;
            _value = value;
            _revealed = false;
            _free = false;
        }

        public void Show(bool show)
        {
            _revealed = show;
        }

        public void FreeCard(bool free)
        {
            string action = free ? "Freed" : "Locked";
            Debug.Log($"{action} card {this}");
            _free = free;
        }

        public Suit Suit => _suit;
        public int Value => _value;
        public bool Revealed => _revealed;
        public bool Free => _free;

        public override string ToString()
        {
            string suitTxt = Utils.GetSuitStr(_suit);

            string revealedTxt = "";
            if (Revealed)
            {
                revealedTxt = "*";
            }
            return $"{Utils.CardValue(this)}{suitTxt}{revealedTxt}";
        }
    }
}