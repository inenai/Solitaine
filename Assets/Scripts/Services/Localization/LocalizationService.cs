using Common;

namespace Services {

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
        TITLE,
        GS_KL_TITLE,
        GS_KL_DRAW_AMOUNT,
        GS_KL_ONE_CARD,
        GS_KL_THREE_CARDS,
        GS_KL_RESTOCK,
        GS_KL_UNLIMITED,
        GS_KL_TWO_TIMES,
        GS_KL_AUTOMOVES,
        GS_KL_ENABLED,
        GS_SAVE_CLOSE,
        GS_SAVE_RESTART,
        GS_SAVE_START,
        GS_SP_TITLE,
        GS_SP_SUITS,
        GS_SP_ONE,
        GS_SP_TWO,
        GS_SP_FOUR,
        GS_SC_TITLE,
        GS_SC_VARIANT,
        LOC_LOAD_SAVED_CONFIRM,
        LOC_LOAD_LOAD_SAVED,
        LOC_LOAD_START_NEW,
    }

    public class LocalizationService : Service
    {

        public override void Init()
        {

        }

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
                        case LocalizationKey.TITLE:
                            return "Project Solitaine";
                        case LocalizationKey.GS_KL_TITLE:
                            return "Klondike Settings";
                        case LocalizationKey.GS_KL_DRAW_AMOUNT:
                            return "Draw amount:";
                        case LocalizationKey.GS_KL_ONE_CARD:
                            return "One card";
                        case LocalizationKey.GS_KL_THREE_CARDS:
                            return "Three cards";
                        case LocalizationKey.GS_KL_RESTOCK:
                            return "Restock deck:";
                        case LocalizationKey.GS_KL_UNLIMITED:
                            return "Unlimited";
                        case LocalizationKey.GS_KL_TWO_TIMES:
                            return "Twice";
                        case LocalizationKey.GS_KL_AUTOMOVES:
                            return "Auto-Moves";
                        case LocalizationKey.GS_KL_ENABLED:
                            return "Enabled";
                        case LocalizationKey.GS_SAVE_CLOSE:
                            return "Save & Close";
                        case LocalizationKey.GS_SAVE_RESTART:
                            return "Save & Restart";
                        case LocalizationKey.GS_SAVE_START:
                            return "Save & Start";
                        case LocalizationKey.GS_SP_TITLE:
                            return "Spider Settings";
                        case LocalizationKey.GS_SP_SUITS:
                            return "Suits";
                        case LocalizationKey.GS_SP_ONE:
                            return "One";
                        case LocalizationKey.GS_SP_TWO:
                            return "Two";
                        case LocalizationKey.GS_SP_FOUR:
                            return "Four";
                        case LocalizationKey.GS_SC_TITLE:
                            return "Scorpion Settings";
                        case LocalizationKey.GS_SC_VARIANT:
                            return "Variant:";
                        case LocalizationKey.LOC_LOAD_SAVED_CONFIRM:
                            return "There is a game in progress. Load game?";
                        case LocalizationKey.LOC_LOAD_LOAD_SAVED:
                            return "Load game";
                        case LocalizationKey.LOC_LOAD_START_NEW:
                            return "New game";
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
                            return "Config.";
                        case LocalizationKey.LOC_CONTROLS:
                            return "Controles";
                        case LocalizationKey.LANGUAGE:
                            return "Español";
                        case LocalizationKey.TITLE:
                            return "Proyecto Solitaine";
                        case LocalizationKey.GS_KL_TITLE:
                            return "Configurar Klondike";
                        case LocalizationKey.GS_KL_DRAW_AMOUNT:
                            return "Repartir";
                        case LocalizationKey.GS_KL_ONE_CARD:
                            return "Una carta";
                        case LocalizationKey.GS_KL_THREE_CARDS:
                            return "Tres cartas";
                        case LocalizationKey.GS_KL_RESTOCK:
                            return "Reponer mazo:";
                        case LocalizationKey.GS_KL_UNLIMITED:
                            return "Ilimitado";
                        case LocalizationKey.GS_KL_TWO_TIMES:
                            return "Dos veces";
                        case LocalizationKey.GS_KL_AUTOMOVES:
                            return "Auto-movimientos";
                        case LocalizationKey.GS_KL_ENABLED:
                            return "Habilitados";
                        case LocalizationKey.GS_SAVE_CLOSE:
                            return "Guardar y Cerrar";
                        case LocalizationKey.GS_SAVE_RESTART:
                            return "Guardar y empezar de nuevo";
                        case LocalizationKey.GS_SAVE_START:
                            return "Guardar y empezar";
                        case LocalizationKey.GS_SP_TITLE:
                            return "Configurar Spider";
                        case LocalizationKey.GS_SP_SUITS:
                            return "Palos";
                        case LocalizationKey.GS_SP_ONE:
                            return "Uno";
                        case LocalizationKey.GS_SP_TWO:
                            return "Dos";
                        case LocalizationKey.GS_SP_FOUR:
                            return "Cuatro";
                        case LocalizationKey.GS_SC_TITLE:
                            return "Configurar Scorpion";
                        case LocalizationKey.GS_SC_VARIANT:
                            return "Variante:";
                        case LocalizationKey.LOC_LOAD_SAVED_CONFIRM:
                            return "Hay una partida guardada. ¿Cargar partida?";
                        case LocalizationKey.LOC_LOAD_LOAD_SAVED:
                            return "Cargar partida";
                        case LocalizationKey.LOC_LOAD_START_NEW:
                            return "Nueva partida";
                    }
                    break;
            }
            throw new System.Exception($"Localization key {key} could not be resolved for language {lang}");
        }


    }
}