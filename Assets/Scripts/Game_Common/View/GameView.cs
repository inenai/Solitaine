using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

public abstract class GameView : MonoBehaviour
{
    [SerializeField] CardView[] cardViews;
    protected GameController _controller;
    protected DeckView _deckView;

    public DeckView Deck => _deckView;
    public GameController Controller => _controller;

    public abstract bool IsCardInATargetablePile(PileData cardOwnerData, out TargetCardPileView result);

    #region RefreshViews
    public abstract void RefreshStock(CardPile stockCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone);
    public abstract void RefreshWaste(CardPile wasteCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone);
    public abstract void RefreshFoundations(List<CardPile> foundationsCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone);
    public abstract void RefreshTableaus(List<CardPile> tableausCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone);
    public abstract void RefreshFreeCells(List<CardPile> list, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone);
    #endregion

    public virtual void Reset() { }
    protected abstract void OnInit();

    public virtual void Init(GameController controller)
    {
        _controller = controller;
        _deckView = new DeckView(this, cardViews);
        OnInit();
    }

    public Vector3 GetCardViewPosition(Card card)
    {
        return _deckView.GetCardView(card).transform.position;
    }
}
