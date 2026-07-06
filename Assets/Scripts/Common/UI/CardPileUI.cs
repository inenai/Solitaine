using System;
using UnityEngine;

namespace Common
{
    public abstract class CardPileUI : MonoBehaviour
    {
        [SerializeField] protected PileKind _pileKind;
        protected int _index = -1;
        protected IGameController _controller;
        public PileKind PileKind => _pileKind;
        public int Index => _index;

        public void Init(IGameController controller, int index = -1, Action onDone = null)
        {
            _controller = controller;
            _index = index;
        }
    }
}