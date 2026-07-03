using System.Collections.Generic;
using Model.Common;

namespace Klondike
{
    public class KlondikeState
    {
        public Stack<Card> StockPile;
        public Stack<Card> WastePile;
        public Stack<Card>[] Tableau;
        public Foundation[] Foundations;

        int _drawCount = 1;
        bool _allowRestock = true;

        public void InitState()
        {            
            InitFoundations();
            InitTableau();
            StockPile = new Stack<Card>();
            WastePile = new Stack<Card>();
           
            ApplyConfig();
        }

        private void InitFoundations()
        {
            Foundations = new Foundation[4];
            for (int i = 0; i < 4; i++)
            {
                Foundations[i] = new Foundation();
            }
        }

        private void InitTableau()
        {
            Tableau = new Stack<Card>[7];
            for (int i = 0; i < 7; i++)
            {
                Tableau[i] = new Stack<Card>();
            }
        }
        
        private void ApplyConfig()
        {
            _drawCount = KlondikeConfig.DrawAmount;
            _allowRestock = KlondikeConfig.AllowRedraw;
        }        

        public int DrawCount => _drawCount;
        public bool AllowRestock => _allowRestock;
    }
    
}
