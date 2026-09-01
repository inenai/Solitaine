using System;
using Common;
using Scorpion;
using SQLite;
using Storage;
using UnityEngine;

namespace Services
{
    public class DatabaseService : Service
    {
        private SQLiteConnection _db;
        private string databaseFileName = $"WonGamesDb.db";
        public override void Init()
        {
            _db = new SQLiteConnection($"{Application.persistentDataPath}/{databaseFileName}");
            _db.CreateTable<WonGames>();
            _db.CreateTable<Settings_Klondike>();
            _db.CreateTable<Settings_Spider>();
            _db.CreateTable<Settings_Scorpion>();

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

        public void AddGameEntry_Scorpion(int timeSpentSeconds, int movesCount, int usedUndos, int usedRedos, ScorpionVariant variant)
        {
            var game = AddGameEntry(SolitaireKind.SCORPION, timeSpentSeconds, movesCount, usedUndos, usedRedos);

            var settings_Scorpion = new Settings_Scorpion
            {
                GameId = game.GameId,
                Variant = variant
            };

            _db.Insert(settings_Scorpion);
        }
        #endregion

        #region READ
        // Default values can be int.MaxValue or NULL
        //_db.ExecuteScalar<int>("SELECT COUNT(*) WHERE time_seconds IS NOT NULL AND column != ?", int.MaxValue

        public int GetTotalWins(SolitaireKind kind)
        {
            int wins = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM WonGames WHERE solitaire_kind = ?", kind);
            return wins;
        }
        #endregion
    }
}