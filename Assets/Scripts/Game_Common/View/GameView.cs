using Common;
using UnityEngine;

public abstract class GameView : MonoBehaviour
{
    [SerializeField] CardView[] cardViews;
    protected IGameController _controller;
    protected DeckView _deckView;

    public DeckView Deck => _deckView;
    public IGameController Controller => _controller;

    public virtual void Init(IGameController controller)
    {
        _controller = controller;
        _deckView = new DeckView(this,cardViews);
        OnInit();
    }

    protected abstract void OnInit();
    public abstract bool IsCardInATargetablePile(PileData cardOwnerData, out TargetCardPileView result);

    public Vector3 GetCardViewPosition(Card card)
    {
        return _deckView.GetCardView(card).transform.position;
    }
}
