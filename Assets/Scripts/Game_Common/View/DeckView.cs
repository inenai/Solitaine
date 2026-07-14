using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using UnityEngine;
using Utils;

public class DeckView
{
    public float CardHeight => _cardHeight;

    private GameView _gameView;
    private Dictionary<Card, CardView> _deckView;
    private float _cardHeight;

    public DeckView(GameView view)
    {
        _deckView = new();
        _gameView = view;
    }

    public async Task Load(List<Card> cards)
    {
        Debug.Log($"Load. Creating {cards.Count} cards...");
        List<Task> tasks = new();
        foreach (Card card in cards)
        {
            tasks.Add(CreateCardUI(_gameView.transform, card));
        }
        await Task.WhenAll(tasks);
    }

    public CardView GetCardView(Card card)
    {
        if (!_deckView.ContainsKey(card))
            throw new System.Exception($"Asked for a card view that is not available in DeckView: {card}");
        return _deckView[card];
    }

    private async Task CreateCardUI(Transform transform, Card card)
    {
        //Debug.Log($"Create card {card}...");
        GameObject go = await AssetManager.InstantiateAsync(CardUtils.CardPrefabAddressSprite, transform);
        //Debug.Log($"Create card {card} done.");
        CardView cardView = go.GetComponent<CardView>();
        cardView.Init(_gameView);
        if (_cardHeight == 0f)
            _cardHeight = cardView.GetComponent<Collider2D>().bounds.size.y;
        cardView.LoadCardData(card);
        _deckView.Add(card, cardView);
        go.name = $"Card_{card}";
    }
}
