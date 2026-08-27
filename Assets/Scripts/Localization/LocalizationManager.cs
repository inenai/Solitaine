namespace Common {

    public enum LocalizationKey
    {
        LOC_EXIT_BTN,
        LOC_MAIN_MENU,
        LOC_NEW_GAME,
        LOC_WINS,
        LOC_RULES,
        LOC_GAME_SETTINGS,
        LOC_CONTROLS,
        LANGUAGE,
    }

    public static class LocalizationManager
    {

        internal static string GetLocalizedText(LocalizationKey key, Language lang)
        {
            switch (lang)
            {
                case Language.ENGLISH:
                    switch (key)
                    {
                        case LocalizationKey.LOC_EXIT_BTN:
                            return "Exit";
                        case LocalizationKey.LOC_MAIN_MENU:
                            return "Main Menu";
                        case LocalizationKey.LOC_NEW_GAME:
                            return "New Game";
                        case LocalizationKey.LOC_WINS:
                            return "Wins";
                        case LocalizationKey.LOC_RULES:
                            return "Rules";
                        case LocalizationKey.LOC_GAME_SETTINGS:
                            return "Game Settings";
                        case LocalizationKey.LOC_CONTROLS:
                            return "Controls";
                        case LocalizationKey.LANGUAGE:
                            return "English";
                    }
                    break;
                case Language.SPANISH:
                    switch (key)
                    {
                        case LocalizationKey.LOC_EXIT_BTN:
                            return "Salir";
                        case LocalizationKey.LOC_MAIN_MENU:
                            return "Menú principal";
                        case LocalizationKey.LOC_NEW_GAME:
                            return "Nuevo Juego";
                        case LocalizationKey.LOC_WINS:
                            return "Victorias";
                        case LocalizationKey.LOC_RULES:
                            return "Reglas";
                        case LocalizationKey.LOC_GAME_SETTINGS:
                            return "Configuración";
                        case LocalizationKey.LOC_CONTROLS:
                            return "Controles";
                        case LocalizationKey.LANGUAGE:
                            return "Español";
                    }
                    break;
            }
            throw new System.Exception($"Localization key {key} could not be resolved for language {lang}");
        }
    }
}