using static Model.Common.Enums;

namespace UI.Common
{

    public interface IGameUIController
    {
        bool PilePressed(PileKind pileKind, int index);
    }
}