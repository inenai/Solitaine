using System;
using Common;
using Scorpion;
using SQLite;
using Storage;
using UnityEngine;
using Utils;

namespace Services
{
    public class DatabaseService : Service
    {
        private SQLiteConnection _db;
        private string databaseFileName = $"WonGamesDb.db";

        private void Log(string message)
        {
            Logs.Log($"[DatabaseService] " + message);
        }

        public override void Init()
        {
            _db = new SQLiteConnection($"{Application.persistentDataPath}/{databaseFileName}");
            _db.CreateTable<WonGames>();
            _db.CreateTable<Settings_Klondike>();
            _db.CreateTable<Settings_Spider>();
            _db.CreateTable<Settings_Scorpion>();

            _db.CreateTable<SavedGame>();
            _db.CreateTable<GameCommandEntry>();
            _db.CreateTable<GameCommandActionEntry>();

            MigrateOldWinData();

            // int klondikeWins = GetTotalWins(SolitaireKind.KLONDIKE);
            // int sawayamaWins = GetTotalWins(SolitaireKind.SAWAYAMA);
            // int freeCellWins = GetTotalWins(SolitaireKind.FREECELL);
            // int spiderWins = GetTotalWins(SolitaireKind.SPIDER);
            // Logs.Log($"[SQL] Klondike Wins: {klondikeWins}");
            // Logs.Log($"[SQL] Sawayama Wins: {sawayamaWins}");
            // Logs.Log($"[SQL] FreeCell Wins: {freeCellWins}");
            // Logs.Log($"[SQL] Spider Wins: {spiderWins}");

            // int gamesWithStats = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM WonGames WHERE time_seconds IS NOT NULL AND time_seconds != ?", int.MaxValue);
            // Logs.Log($"[SQL] Games with statistics: {gamesWithStats}");
        }

        private void MigrateOldWinData()
        {
            string Kl_WinCountKey = "KL_WIN_COUNT";
            string Sw_WinCountKey = "SW_WIN_COUNT";
            string Fc_WinCountKey = "FC_WIN_COUNT";
            string Sp_WinCountKey = "SP_WIN_COUNT";

            if (PlayerPrefs.HasKey(Kl_WinCountKey))
            {
                int wins = PlayerPrefs.GetInt(Kl_WinCountKey);
                for (int i = 0; i < wins; i++)
                {
                    AddGameEntry_Klondike(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
                }
                PlayerPrefs.DeleteKey(Kl_WinCountKey);
            }

            if (PlayerPrefs.HasKey(Sw_WinCountKey))
            {
                int wins = PlayerPrefs.GetInt(Sw_WinCountKey);
                for (int i = 0; i < wins; i++)
                {
                    AddGameEntry(SolitaireKind.SAWAYAMA, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
                }
                PlayerPrefs.DeleteKey(Sw_WinCountKey);
            }

            if (PlayerPrefs.HasKey(Fc_WinCountKey))
            {
                int wins = PlayerPrefs.GetInt(Fc_WinCountKey);
                for (int i = 0; i < wins; i++)
                {
                    AddGameEntry(SolitaireKind.FREECELL, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
                }
                PlayerPrefs.DeleteKey(Fc_WinCountKey);
            }

            if (PlayerPrefs.HasKey(Sp_WinCountKey))
            {
                int wins = PlayerPrefs.GetInt(Sp_WinCountKey);
                for (int i = 0; i < wins; i++)
                {
                    AddGameEntry_Spider(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
                }
                PlayerPrefs.DeleteKey(Sp_WinCountKey);
            }
        }

        #region WRITE
        public WonGames AddGameEntry(SolitaireKind kind, int timeSeconds, int movesMade, int undos, int redos)
        {
            var game = new WonGames
            {
                SolitaireKind = kind,
                TimeSeconds = timeSeconds,
                MovesMade = movesMade,
                Undos = undos,
                Redos = redos
            };

            _db.Insert(game);
            return game;
        }

        public void AddGameEntry_Klondike(int timeSeconds, int movesMade, int undos, int redos, int drawAmount, int restocks)
        {
            var game = AddGameEntry(SolitaireKind.KLONDIKE, timeSeconds, movesMade, undos, redos);

            var settings_Klondike = new Settings_Klondike
            {
                GameId = game.GameId,
                DrawsAmount = drawAmount,
                Restocks = restocks
            };

            _db.Insert(settings_Klondike);
        }

        public void AddGameEntry_Spider(int timeSeconds, int movesMade, int undos, int redos, int suits)
        {
            var game = AddGameEntry(SolitaireKind.SPIDER, timeSeconds, movesMade, undos, redos);

            var settings_Spider = new Settings_Spider
            {
                GameId = game.GameId,
                Suits = suits
            };

            _db.Insert(settings_Spider);
        }

        public void AddGameEntry_Scorpion(int timeSpentSeconds, int movesCount, int usedUndos, int usedRedos, ScorpionVariant variant, int suits)
        {
            var game = AddGameEntry(SolitaireKind.SCORPION, timeSpentSeconds, movesCount, usedUndos, usedRedos);

            var settings_Scorpion = new Settings_Scorpion
            {
                GameId = game.GameId,
                Variant = variant,
                Suits = suits
            };

            _db.Insert(settings_Scorpion);
        }

        public void SaveGameProgress(SolitaireKind kind, int seconds, GameCommand[] doneMoves, int seed)
        {
            int[] commandsIDsList = new int[doneMoves.Length];
            for (int i = 0; i < doneMoves.Length; i++)
            {
                commandsIDsList[i] = AddGameCommand(doneMoves[i]);
            }

            var entry = new SavedGame
            {
                SolitaireKind = kind,
                TimeSeconds = seconds,
                CommandsList = CommonUtils.ToString(commandsIDsList),
                RandomSeed = seed,
            };

            _db.InsertOrReplace(entry);
            Log($"Saved game progress KIND: {kind} [{entry.RandomSeed}] MOVES: {commandsIDsList.Length}");
        }

        public void TryDeleteSavedGame(SolitaireKind kind)
        {
            var existingGame = GetSavedGameByKind(kind);
            if (existingGame != null)
            {
                DeleteSavedGameData(existingGame);
            }
        }

        private void DeleteSavedGameData(SavedGame game)
        {
            Log($"Deleting game progress KIND: {game.SolitaireKind} [{game.RandomSeed}]");
            if (game.CommandsList != null)
            {
                foreach (int commandId in game.CommandsList)
                {
                    var command = _db.Find<GameCommandEntry>(commandId);
                    if (command?.ActionsList != null)
                    {
                        foreach (int actionId in command.ActionsList)
                        {
                            _db.Delete<GameCommandActionEntry>(actionId);
                        }
                    }

                    _db.Delete<GameCommandEntry>(commandId);
                }
            }

            _db.Delete(game);
        }

        public int AddGameCommand(GameCommand command)
        {
            GameCommandAction[] actionArray = command.Actions.ToArray();
            int[] actionsIDsList = new int[actionArray.Length];

            for (int i = 0; i < actionArray.Length; i++)
            {
                actionsIDsList[i] = AddGameCommandAction(actionArray[i]);
            }

            var entry = new GameCommandEntry
            {
                ActionsList = CommonUtils.ToString(actionsIDsList),
            };

            _db.Insert(entry);
            return entry.CommandId;
        }

        public int AddGameCommandAction(GameCommandAction commandAction)
        {
            var entry = new GameCommandActionEntry
            {
                CommandActionKind = commandAction.Kind,
            };

            switch (commandAction.Kind)
            {
                case GameCommandActionKind.MOVE:
                    entry.SourcePile = ((GameCommandActionMove)commandAction).SourcePile;
                    entry.SourcePileIndex = ((GameCommandActionMove)commandAction).SourceIndex;
                    entry.TargetPile = ((GameCommandActionMove)commandAction).TargetPile;
                    entry.TargetPileIndex = ((GameCommandActionMove)commandAction).TargetIndex;
                    break;
                case GameCommandActionKind.REVEAL:
                    entry.RevealedAction = ((GameCommandActionReveal)commandAction).Revealed;
                    entry.CardSuit = ((GameCommandActionReveal)commandAction).CardSuit;
                    entry.CardValue = ((GameCommandActionReveal)commandAction).CardValue;
                    break;
                case GameCommandActionKind.FREE:
                    entry.FreedAction = ((GameCommandActionFree)commandAction).Freed;
                    entry.CardSuit = ((GameCommandActionFree)commandAction).CardSuit;
                    entry.CardValue = ((GameCommandActionFree)commandAction).CardValue;
                    break;
                case GameCommandActionKind.RESTOCK:
                    entry.Restock = true;
                    break;
                case GameCommandActionKind.MOVE_STACK:
                    entry.SourcePile = ((GameCommandActionMoveStack)commandAction).SourcePile;
                    entry.SourcePileIndex = ((GameCommandActionMoveStack)commandAction).SourceIndex;
                    entry.TargetPile = ((GameCommandActionMoveStack)commandAction).TargetPile;
                    entry.TargetPileIndex = ((GameCommandActionMoveStack)commandAction).TargetIndex;
                    entry.CardSuit = ((GameCommandActionMoveStack)commandAction).CardSuit;
                    entry.CardValue = ((GameCommandActionMoveStack)commandAction).CardValue;
                    break;
            }

            _db.Insert(entry);
            return entry.ActionId;
        }
        #endregion

        #region READ
        // Default values can be int.MaxValue or NULL
        //_db.ExecuteScalar<int>("SELECT COUNT(*) WHERE time_seconds IS NOT NULL AND column != ?", int.MaxValue

        public SavedGame GetSavedGameByKind(SolitaireKind kind)
        {
            return _db.Table<SavedGame>()
                               .FirstOrDefault(g => g.SolitaireKind == kind);
        }

        public int GetTotalWins(SolitaireKind kind)
        {
            int wins = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM WonGames WHERE solitaire_kind = ?", kind);
            return wins;
        }

        public bool HasSavedGame(SolitaireKind kind)
        {
            return GetSavedGameByKind(kind) != null;
        }
        #endregion
    }
}