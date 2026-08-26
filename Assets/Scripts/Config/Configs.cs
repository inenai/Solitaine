using System;
using UnityEngine;

namespace Common
{
    [CreateAssetMenu(fileName = "UIConfigs", menuName = "Solitaine/Create UI config asset")]

    public class Configs : ScriptableObject
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

        [Header("Rules")]
        [SerializeField] string Rules_Klondike = "· Stack cards by alternating colors in descending order.\n· Only Ks can occupy empty spaces.\n· Stack cards by suit in ascending order in the four foundations from A to K to win.";
        [SerializeField] string Rules_Sawayama = "· Stack cards by alternating colors in descending order.\n· Any card can occupy empty spaces.\n· After drawing all cards from the stock, you can use the remaining empty space to place one card.\n· Stack cards by suit in ascending order in the four foundations from A to K to win.";
        [SerializeField] string Rules_FreeCell = "· Stack cards by alternating colors in descending order.\n· Any card can occupy empty spaces.\n· Are there enough free spaces to move a stack card by card? If not, you can't move that stack!\n· Stack cards by suit in ascending order in the four foundations from A to K to win.";
        [SerializeField] string Rules_Spider = "· Stack cards by descending order, regardless of suit.\n· To move stacks, they must only have one suit!\n· Any card can occupy empty spaces.\n· To draw more cards from the stock, there cannot be empty spaces.\n· Form eight ordered stacks of a single suit from K to A to win.";

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
            if (wins < 10) return Wins_1;
            if (wins < 50) return Wins_2;
            if (wins < 200) return Wins_3;
            if (wins < 1000) return Wins_4;
            if (wins < 5000) return Wins_5;
            return Wins_6;
        }
        internal string GetRules(SolitaireKind kind)
        {
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
                    throw new Exception($"Solitaire not yet fully supported by UI: {kind}");
            }
        }
    }
}