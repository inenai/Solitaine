using System;
using System.Collections;
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
    private int _coroutinesRunning;

    public DeckView(GameView view)
    {
        _deckView = new();
        _gameView = view;
    }

    public void Load(List<Card> cards, Action onDone)
    {
        Debug.Log($"Load. Creating {cards.Count} cards...");
        _coroutinesRunning = 0;
        foreach (Card card in cards)
        {
            _coroutinesRunning++;
            CreateCardUI(_gameView.transform, card, ()=>{
                _coroutinesRunning--;
            });
        }
        _gameView.StartCoroutine(WaitForAllCoroutinesDone(onDone));
    }

    private IEnumerator WaitForAllCoroutinesDone(Action onDone)
    {
        while (_coroutinesRunning > 0) yield return null;
        onDone?.Invoke();
    }

    public CardView GetCardView(Card card)
    {
        if (!_deckView.ContainsKey(card))
            throw new System.Exception($"Asked for a card view that is not available in DeckView: {card}");
        return _deckView[card];
    }

    private void CreateCardUI(Transform transform, Card card, Action onDone)
    {
        //Debug.Log($"Create card {card}...");
        AssetManager.InstantiateAsync(CardUtils.CardPrefabAddressSprite, transform, (gameObject) =>
        {
            InitCardGameObject(gameObject, card);
            onDone?.Invoke();
            //Debug.Log($"Create card {card} done.");

        }, (errorMessage) => {
            Debug.LogError(errorMessage);
            onDone?.Invoke();
        });
    }

    private void InitCardGameObject(GameObject gameObject, Card card)
    {
        gameObject.name = $"Card_{card}";
        CardView cardView = gameObject.GetComponent<CardView>();
        cardView.Init(_gameView, card);
        if (_cardHeight == default)
        {
            _cardHeight = cardView.GetComponent<Collider2D>().bounds.size.y;
        }
        _deckView.Add(card, cardView);
    }
}
