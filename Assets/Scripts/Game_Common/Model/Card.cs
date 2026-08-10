using UnityEngine;
using Utils;

namespace Common {

    public class Card
    {
        private int _value;
        private CardSuit _suit;
        private bool _free;
        private bool _revealed;

        public Card(CardSuit suit, int value)
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
            _free = free;
            //Debug.Log($"{action} card {this}");
        }

        public CardSuit Suit => _suit;
        public int Value => _value;
        public bool Revealed => _revealed;
        public bool Free => _free;

        public override string ToString()
        {
            string suitTxt = CardUtils.GetSuitStr(_suit);
            string lockedPref = _free ? "" : "[";
            string lockedSuf = _free ? "" : "]";
            string revealedTxt = "";
            if (Revealed)
            {
                revealedTxt = "*";
            }
            return $"{lockedPref}{CardUtils.CardValue(this)}{suitTxt}{revealedTxt}{lockedSuf}";
        }
    }
}