using UnityEngine;
using Common.Utils;


namespace Model.Common {
    public class Card
    {
        private int _value;
        private Enums.Suit _suit;
        private bool _free;
        private bool _revealed;

        public Card(Enums.Suit suit, int value)
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
            Debug.Log($"{action} card {Utils.CardToShortString(this)}");
            _free = free;
        }

        public Enums.Suit Suit => _suit;
        public int Value => _value;
        public bool Revealed => _revealed;
        public bool Free => _free;
    }
}