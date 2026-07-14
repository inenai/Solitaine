using Common;
using UnityEngine;

public class CardTextureGetter : MonoBehaviour
{
    [SerializeField] Sprite[] hearts;
    [SerializeField] Sprite[] diamonds;
    [SerializeField] Sprite[] spades;
    [SerializeField] Sprite[] clubs;

    [SerializeField] Sprite[] backs;

    private static CardTextureGetter _instance;
    public static CardTextureGetter Instance => _instance;

    void Awake()
    {
        SetInstance(this);
    }

    public static void SetInstance(CardTextureGetter instance)
    {
        _instance = instance;
    }

    public Sprite GetCardFace(CardSuit suit, int value)
    {
        switch (suit)
        {
            case CardSuit.HEARTS:
                return hearts[value - 1];
            case CardSuit.DIAMONDS:
                return diamonds[value - 1];
            case CardSuit.SPADES:
                return spades[value - 1];
            case CardSuit.CLUBS:
                return clubs[value - 1];
            default:
                throw new System.Exception($"Invalid card suit: {suit}");
        }
    }

    public Sprite GetRedDeck()
    {
        if (backs.Length < 3) throw new System.Exception("Card backs not loaded");
        return backs[0];
    }
    public Sprite GetGreenDeck()
    {
        if (backs.Length < 3) throw new System.Exception("Card backs not loaded");
        return backs[1];
    }

    public Sprite GetBlueDeck()
    {
        if (backs.Length < 3) throw new System.Exception("Card backs not loaded");
        return backs[2];
    }
}
