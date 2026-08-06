using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Klondike
{
    public class KlondikeState : GameState
    {

        public int DrawCount => _drawAmount;
        public int AvailableRestocks => _availableRestocks;
        public bool FoundationCardsFree => _foundationCardsFree;

        int _drawAmount = KlondikeSettings.DEFAULT_DRAW_AMOUNT;
        int _availableRestocks = KlondikeSettings.DEFAULT_RESTOCKS;
        bool _foundationCardsFree = KlondikeSettings.DEFAULT_FOUNDATION_CARDS_FREE;

        public KlondikeState(int foundations, int tableaus, int freeCells, bool stock, bool waste) : base(foundations, tableaus, freeCells, stock, waste)
        {
        }

        protected override void ApplyConfig()
        {
            _drawAmount = KlondikeSettings.DrawAmount;
            _availableRestocks = KlondikeSettings.AvailableRestocks;
            _foundationCardsFree = KlondikeSettings.FoundationCardsFree;
        }


        public override void OnRestock(bool undo = false)
        {
            if (_availableRestocks > 0)
            {
                if (undo) _availableRestocks++;
                else _availableRestocks--;
            }
        }
    }
}
