using Model.Common;
using static Model.Common.Enums;

namespace UI.Common
{
    public interface IGameController
    {
        bool CardDoubleClicked(Card card);
        bool PileClicked(PileKind pileKind, int index);
    }
}