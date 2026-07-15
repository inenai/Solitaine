using Common;

namespace Klondike
{
    public class KlondikeUI : GameUI
    {
        protected override void OnInit()
        {
            UpdateWins();
        }

        public override void OnGameWon()
        {
            UpdateWins();
            base.OnGameWon();
        }

        private void UpdateWins()
        {
            _winsTxt.text = $"Wins: {KlondikeSettings.WinCount}";
        }
    }
}