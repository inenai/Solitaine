using UnityEngine;

public static class KlondikeConfig
{
    const string DrawAmountKey = "KL_DRAW_AMOUNT";
    const string AllowRedrawKey = "KL_ALLOW_REDRAW";

    public static int DrawAmount
    {
        get
        {
            return 3; //TODO reset when settings available
            return PlayerPrefs.GetInt(DrawAmountKey, 1);
        }
        set
        {
            PlayerPrefs.SetInt(DrawAmountKey, value);
            PlayerPrefs.Save();
        }
    }
    public static bool AllowRedraw
    {
        get
        {
            return PlayerPrefs.GetInt(AllowRedrawKey, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(DrawAmountKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

}