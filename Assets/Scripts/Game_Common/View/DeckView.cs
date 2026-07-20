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

    public void Load(List<Card> cards, Transform parent, Action onDone)
    {
        Debug.Log($"Load. Creating {cards.Count} cards...");
        _coroutinesRunning = 0;
        float zOffset = CardUtils.CardStackZOffset;
        foreach (Card card in cards)
        {
            _coroutinesRunning++;
            CreateCardUI(parent, card, zOffset, () =>
            {
                _coroutinesRunning--;
            });
            zOffset += CardUtils.CardStackZOffset;
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
        if (_deckView == null)
            throw new System.Exception($"DeckView is null!");
        if (card == null)
            throw new System.Exception($"Asked for a card view that is null!");

        if (!_deckView.ContainsKey(card))
            throw new System.Exception($"Asked for a card view that is not available in DeckView: {card}");
        return _deckView[card];
    }

    private void CreateCardUI(Transform transform, Card card, float zOffset, Action onDone)
    {
        //Debug.Log($"Create card {card}...");
        AssetManager.InstantiateAsync(CardUtils.CardPrefabAddressSprite, transform, (gameObject) =>
        {
            InitCardGameObject(gameObject, card, zOffset);
            onDone?.Invoke();
            //Debug.Log($"Create card {card} done.");
        }, (errorMessage) => {
            Debug.LogError(errorMessage);
            onDone?.Invoke();
        });
    }

    private void InitCardGameObject(GameObject gameObject, Card card, float zOffset)
    {
        gameObject.name = $"Card_{card}";
        gameObject.transform.localPosition = new Vector3(0f, 0f, zOffset);
        CardView cardView = gameObject.GetComponent<CardView>();
        cardView.Init(_gameView, card);
        if (_cardHeight == default)
        {
            _cardHeight = cardView.GetComponent<Collider2D>().bounds.size.y;
        }
        _deckView.Add(card, cardView);
    }
}
