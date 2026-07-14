using Common;
using UnityEngine;

public abstract class GameView : MonoBehaviour
{
    protected IGameController _controller;
    protected DeckView _deckView;

    public DeckView Deck => _deckView;
    public IGameController Controller => _controller;

    public virtual void Init(IGameController controller)
    {
        _controller = controller;
        _deckView = new DeckView(this);
        OnInit();
    }

    protected abstract void OnInit();
}
