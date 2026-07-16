
using Common;

public interface IGameState
{
    public abstract void LogState();
    public abstract void OnWin();
    public abstract PileData GetCardPileOwnerData(Card card);
}
