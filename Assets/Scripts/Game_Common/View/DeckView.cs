using System;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Utils;

public class DeckView
{
    public float CardHeight => _cardHeight;

    private GameView _gameView;
    private Dictionary<Card, CardView> _deckView;
    private float _cardHeight;
    // private int _coroutinesRunning;
    private CardView[] _cardViews;

    public DeckView(GameView view, CardView[] cardViews)
    {
        _deckView = new();
        _gameView = view;
        _cardViews = cardViews;
    }

    public void Load(List<Card> cards, Action onDone)
    {
        Debug.Log($"Load. Loading {cards.Count} cards...");
        if (cards.Count != _cardViews.Length)
        {
            throw new Exception("Scene has not enough pre-loaded cards!!!");
        }

        float zOffset = CardUtils.CardStackZOffset;
        for (int i = 0; i < cards.Count; i++)
        {
            InitCard(_cardViews[i], cards[i], zOffset);
            zOffset += CardUtils.CardStackZOffset;
        }
        onDone?.Invoke();
    }

    private void InitCard(CardView cardView, Card card, float zOffset)
    {
        cardView.gameObject.name = $"Card_{card}";
        cardView.transform.localPosition = new Vector3(0f, 0f, zOffset);
        cardView.Init(_gameView, card);
        if (_cardHeight == default)
        {
            _cardHeight = cardView.GetComponent<Collider2D>().bounds.size.y;
        }
        _deckView.Add(card, cardView);
    }

    public CardView GetCardView(Card card)
    {
        if (_deckView == null)
            throw new System.Exception($"DeckView is null!");
        if (card == null)
            throw new System.Exception($"Asked for a card view that is null!");

        if (!_deckView.ContainsKey(card))
            throw new System.Exception($"Asked for a card view that is not available in DeckView: {card}");
        return _deckView[card];
    }

    // private void CreateCardUI(Transform transform, Card card, float zOffset, Action onDone)
    // {
    //     //Debug.Log($"Create card {card}...");
    //     AssetManager.InstantiateAsync(CardUtils.CardPrefabAddressSprite, transform, (gameObject) =>
    //     {
    //         InitCardGameObject(gameObject, card, zOffset);
    //         onDone?.Invoke();
    //         //Debug.Log($"Create card {card} done.");
    //     }, (errorMessage) =>
    //     {
    //         Debug.LogError(errorMessage);
    //         onDone?.Invoke();
    //     });
    // }

    // private IEnumerator WaitForAllCoroutinesDone(Action onDone)
    // {
    //     while (_coroutinesRunning > 0) yield return null;
    //     onDone?.Invoke();
    // }

    // private void InitCardGameObject(GameObject gameObject, Card card, float zOffset)
    // {
    //     InitCard(gameObject.GetComponent<CardView>(), card, zOffset);
    // }
}
