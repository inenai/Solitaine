using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using UnityEngine;
using Utils;

namespace Klondike
{
    public class WasteView : CardPileView
    {
        /// <summary>
        /// <para> index => gameobject</para>
        /// <para>0 => 1st card, leftmost</para>
        /// <para>1 => 2nd card, middle</para>
        /// <para>2 => 3rd card, rightmost</para>
        /// </summary>

        [SerializeField] private Transform[] _cardPositions;
        protected List<CardView> _cardViews;

        protected override void OnInit() { }

        public override Task Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition)
        {
            Debug.Log("WasteView refreshing...");
            if (cards.Count == 0)
            {
                Debug.Log("WasteView refreshed.");
                return Task.CompletedTask;
            }

            _cardViews = StackCardsInPosition(cards, _cardPositions[0].position, transform, "Card_W_");

            if (_cardViews.Count == 2)
            {
                _cardViews[^1].transform.position = new Vector3(_cardPositions[1].position.x, _cardPositions[1].position.y, _cardViews[^1].transform.position.z);
            }

            if (_cardViews.Count > 2)
            {
                _cardViews[^1].transform.position = new Vector3(_cardPositions[2].position.x, _cardPositions[2].position.y, _cardViews[^1].transform.position.z);
                _cardViews[^2].transform.position = new Vector3(_cardPositions[1].position.x, _cardPositions[1].position.y, _cardViews[^2].transform.position.z);
            }

            Debug.Log("WasteView refreshed.");
            return Task.CompletedTask;
        }
    }
}