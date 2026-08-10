using Common;

namespace Sawayama
{
    public class SawayamaState : GameState
    {
        public SawayamaState(int foundations, int tableaus, int freeCells, bool stock, bool waste) : base(foundations, tableaus, freeCells, stock, waste)
        {
        }

        protected override void ApplyConfig()
        {
        }
    }
}