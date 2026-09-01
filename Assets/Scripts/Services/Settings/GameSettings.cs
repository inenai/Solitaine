using System;
using Services;
using UnityEngine;

namespace Common
{
    [CreateAssetMenu(fileName = "UIConfigs", menuName = "Solitaine/Create UI config asset")]

    public class Settings : ScriptableObject
    {
        [Header("Text Colors")]
        [SerializeField] Color Txt_Color_Klondike = new Color(50f / 255f, 113f / 255f, 185f / 255f);
        [SerializeField] Color Txt_Color_Sawayama = new Color(120f / 255f, 55f / 255f, 64f / 255f);
        [SerializeField] Color Txt_Color_FreeCell = new Color(59f / 255f, 163f / 255f, 132f / 255f);
        [SerializeField] Color Txt_Color_Spider = new Color(255f / 255f, 190f / 255f, 0f / 255f);

        [Header("Image Colors")]
        [SerializeField] Color Img_Color_Klondike = new Color(0f / 255f, 63f / 255f, 135f / 255f);
        [SerializeField] Color Img_Color_Sawayama = new Color(70f / 255f, 5f / 255f, 14f / 255f);
        [SerializeField] Color Img_Color_FreeCell = new Color(9f / 255f, 113f / 255f, 82f / 255f);
        [SerializeField] Color Img_Color_Spider = new Color(183f / 255f, 140f / 255f, 16f / 255f);

        [Header("Wins Colors")]
        [SerializeField] Color Wins_1 = new Color(1f, 1f, 1f);
        [SerializeField] Color Wins_2 = new Color(0.7f, 0.4f, 0.23f);
        [SerializeField] Color Wins_3 = new Color(0.67f, 0.67f, 0.67f);
        [SerializeField] Color Wins_4 = new Color(1f, 0.83f, 0f);
        [SerializeField] Color Wins_5 = new Color(0f, 1f, 0.67f);
        [SerializeField] Color Wins_6 = new Color(1f, 0f, 0.66f);
        [SerializeField] Gradient Wins_Gradient = new Gradient();

        [Header("Rules EN")]
        [SerializeField] string Rules_Klondike = @"· Stack cards by alternating colors in descending order.
· Only Ks can occupy empty spaces.
· Stack cards by suit in ascending order in the four foundations from A to K to win.";
        [SerializeField] string Rules_Sawayama =
@"· Stack cards by alternating colors in descending order.
· Any card can occupy empty spaces.
· After drawing all cards from the stock, you can use the remaining empty space to place one card.
· Stack cards by suit in ascending order in the four foundations from A to K to win.";
        [SerializeField] string Rules_FreeCell =
@"· Stack cards by alternating colors in descending order.
· Any card can occupy empty spaces.
· Are there enough free spaces to move a stack card by card? If not, you can't move that stack!
· Stack cards by suit in ascending order in the four foundations from A to K to win.";
        [SerializeField] string Rules_Spider =
@"· Stack cards by descending order, regardless of suit.
· To move stacks, they must only have one suit!
· Any card can occupy empty spaces.
· To draw more cards from the stock, there cannot be empty spaces.
· Form eight ordered stacks of a single suit from K to A to win.";

        [Header("Controls EN")]
        [SerializeField] string Touch_Controls_Desc = @"· Smart auto-move: Double tap
· Peek: Hold tap";
        [SerializeField] string Key_Mouse_Controls_Desc = @"· Drag: Left-Click / Space
· Cancel drag: Right click
· Smart auto-move: Double-click / C
· Draw: D
· Restart: R
· Undo: Z
· Redo: X
· Peek: Hold right-click / V";

        [Header("Rules ES")]
        [SerializeField] string Rules_Klondike_ES =
@"· Apila cartas en orden descendiente, alternando colores.
· Solo las K pueden ocupar espacios vacíos.
· Apila las cartas por palo en orden ascendente de la A a la K en las bases para ganar.";
        [SerializeField] string Rules_Sawayama_ES =
@"· Apila cartas en orden descendiente, alternando colores.
· Puedes colocar cualquier carta en los espacios vacíos.
· Tras dar todas las cartas del mazo, puedes usar su espacio libre para colocar una carta.
· Apila las cartas por palo en orden ascendente de la A a la K en las bases para ganar.";
        [SerializeField] string Rules_FreeCell_ES =
@"· Apila las cartas en orden descendiente, alternando colores.
· Puedes colocar cualquier carta en los espacios vacíos.
· ¿Hay lugar suficiente para mover una pila carta por carta? ¡Si no, no puedes mover esa pila!
· Apila las cartas por palo en orden ascendente de la A a la K en las bases para ganar.";
        [SerializeField] string Rules_Spider_ES =
@"· Apila las cartas en orden descendiente, independientemente del palo.
· ¡Para mover pilas, deben ser de un solo palo!
· Puedes colocar cualquier carta en los espacios vacíos.
· Para dar más cartas del mazo, no debe haber espacios vacíos.
· Forma ocho pilas ordenadas de K a A de un solo palo para ganar.";

        [Header("Controls ES")]
        [SerializeField] string Touch_Controls_Desc_ES =
@"· Auto-movimiento inteligente: Toca una carta dos veces
· Espiar: Mantén el dedo sobre una carta";
        [SerializeField] string Key_Mouse_Controls_Desc_ES =
@"· Arrastrar: Click izq. / Barra espaciadora
· Cancelar arrastre: Click derecho
· Auto-movimiento inteligente: Doble-click / C
· Repartir: D
· Nuevo juego: R
· Deshacer: Z
· Rehacer: X
· Espiar: Mantener click der. / V";

        public Color GetUIImageColor(SolitaireKind kind)
        {
            switch (kind)
            {
                case SolitaireKind.KLONDIKE:
                    return Img_Color_Klondike;
                case SolitaireKind.SAWAYAMA:
                    return Img_Color_Sawayama;
                case SolitaireKind.FREECELL:
                    return Img_Color_FreeCell;
                case SolitaireKind.SPIDER:
                    return Img_Color_Spider;
                default:
                    return new Color(100f / 255f, 100f / 255f, 100f / 255f);
            }
        }

        public Color GetUITextColor(SolitaireKind kind)
        {
            switch (kind)
            {
                case SolitaireKind.KLONDIKE:
                    return Txt_Color_Klondike;
                case SolitaireKind.SAWAYAMA:
                    return Txt_Color_Sawayama;
                case SolitaireKind.FREECELL:
                    return Txt_Color_FreeCell;
                case SolitaireKind.SPIDER:
                    return Txt_Color_Spider;
                default:
                    return new Color(200f / 255f, 200f / 255f, 200f / 255f);
            }
        }

        public Color GetWinsColor(int wins)
        {
            if (wins < 500) return Wins_Gradient.Evaluate((float)wins / 500);
            else return Wins_Gradient.Evaluate(1);
            // if (wins < 10) return Wins_1;
            // if (wins < 50) return Wins_2;
            // if (wins < 200) return Wins_3;
            // if (wins < 1000) return Wins_4;
            // if (wins < 5000) return Wins_5;
            // return Wins_6;
        }

        internal string GetRules(SolitaireKind kind)
        {
            switch (God.Settings.CurrentLanguage)
            {
                case Language.ENGLISH:
                    switch (kind)
                    {
                        case SolitaireKind.KLONDIKE:
                            return Rules_Klondike;
                        case SolitaireKind.SAWAYAMA:
                            return Rules_Sawayama;
                        case SolitaireKind.FREECELL:
                            return Rules_FreeCell;
                        case SolitaireKind.SPIDER:
                            return Rules_Spider;
                        default:
                            throw new Exception($"Solitaire not yet fully supported by UI: {kind} language: {Language.ENGLISH}");
                    }

                case Language.SPANISH:
                    switch (kind)
                    {
                        case SolitaireKind.KLONDIKE:
                            return Rules_Klondike_ES;
                        case SolitaireKind.SAWAYAMA:
                            return Rules_Sawayama_ES;
                        case SolitaireKind.FREECELL:
                            return Rules_FreeCell_ES;
                        case SolitaireKind.SPIDER:
                            return Rules_Spider_ES;
                        default:
                            throw new Exception($"Solitaire not yet fully supported by UI: {kind}language: {Language.SPANISH}");
                    }
                default:
                    throw new Exception($"Language not recognized: {God.Settings.CurrentLanguage}");
            }
        }

        internal string GetKeyboardMouseCtrlsDesc()
        {
            switch (God.Settings.CurrentLanguage)
            {
                case Language.ENGLISH:
                    return Key_Mouse_Controls_Desc;
                case Language.SPANISH:
                    return Key_Mouse_Controls_Desc_ES;
            }
            return Key_Mouse_Controls_Desc;
        }

        internal string GetTouchCtrlsDesc()
        {
            switch (God.Settings.CurrentLanguage)
            {
                case Language.ENGLISH:
                    return Touch_Controls_Desc;
                case Language.SPANISH:
                    return Touch_Controls_Desc_ES;
            }
            return Touch_Controls_Desc;
        }
    }
}