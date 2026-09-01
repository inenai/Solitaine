using System;
using Services;
using UnityEngine;

namespace Common
{
    [CreateAssetMenu(fileName = "UIConfigs", menuName = "Solitaine/Create UI config asset")]

    public class Settings : ScriptableObject
    {
        [Header("Text Colors")]
        [SerializeField] Color Txt_Color_Klondike;
        [SerializeField] Color Txt_Color_Sawayama;
        [SerializeField] Color Txt_Color_FreeCell;
        [SerializeField] Color Txt_Color_Spider;
        [SerializeField] Color Txt_Color_Scorpion;

        [Header("Image Colors")]
        [SerializeField] Color Img_Color_Klondike;
        [SerializeField] Color Img_Color_Sawayama;
        [SerializeField] Color Img_Color_FreeCell;
        [SerializeField] Color Img_Color_Spider;
        [SerializeField] Color Img_Color_Scorpion;

        [Header("Wins Colors")]
        [SerializeField] Color Wins_1;
        [SerializeField] Color Wins_2;
        [SerializeField] Color Wins_3;
        [SerializeField] Color Wins_4;
        [SerializeField] Color Wins_5;
        [SerializeField] Color Wins_6;
        [SerializeField] Gradient Wins_Gradient;

        [Header("Rules EN")]
        [SerializeField] string Rules_Klondike;
        [SerializeField] string Rules_Sawayama;
        [SerializeField] string Rules_FreeCell;
        [SerializeField] string Rules_Spider;
        [SerializeField] string Rules_Scorpion;

        [Header("Controls EN")]
        [SerializeField] string Touch_Controls_Desc;
        [SerializeField] string Key_Mouse_Controls_Desc;

        [Header("Rules ES")]
        [SerializeField] string Rules_Klondike_ES;
        [SerializeField] string Rules_Sawayama_ES;
        [SerializeField] string Rules_FreeCell_ES;
        [SerializeField] string Rules_Spider_ES;
        [SerializeField] string Rules_Scorpion_ES;

        [Header("Controls ES")]
        [SerializeField] string Touch_Controls_Desc_ES;
        [SerializeField] string Key_Mouse_Controls_Desc_ES;

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
                case SolitaireKind.SCORPION:
                    return Img_Color_Scorpion;
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
                case SolitaireKind.SCORPION:
                    return Txt_Color_Scorpion;
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
                        case SolitaireKind.SCORPION:
                            return Rules_Scorpion;
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
                        case SolitaireKind.SCORPION:
                            return Rules_Scorpion_ES;
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