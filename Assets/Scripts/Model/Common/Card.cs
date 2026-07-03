using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Model.Common {
    public class Card 
    {
        private int _value;
        private Enums.Suit _suit;
        private bool _movable;
        private bool _revealed;

        public Card(Enums.Suit suit, int value)
        {
            _suit = suit;
            _value = value;
            _revealed = false;
        }

        public void Show(bool show)
        {
            _revealed = show;
        }

        public void MakeMovable(bool movable)
        {
            _movable = movable;
        }

        public Enums.Suit Suit => _suit;
        public int Value => _value;
        public bool Revealed => _revealed;
        public bool Movable => _movable;
    }
}