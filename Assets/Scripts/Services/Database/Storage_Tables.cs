using Common;
using SQLite;

namespace Storage
{
    [Table("WonGames")]
    public class WonGames
    {
        [PrimaryKey, AutoIncrement]
        [Column("game_id")]
        public int GameId { get; set; }
        [Column("solitaire_kind")]
        public SolitaireKind SolitaireKind { get; set; }
        [Column("time_seconds")]
        public int TimeSeconds { get; set; }
        [Column("moves_made")]
        public int MovesMade { get; set; }
        [Column("undos_count")]
        public int Undos { get; set; }
        [Column("redos_count")]
        public int Redos { get; set; }
    }

    [Table("GameSettings_Klondike")]
    public class Settings_Klondike
    {
        [Column("game_id")]
        public int GameId { get; set; }
        [Column("draws_amount")]
        public int DrawsAmount { get; set; }
        [Column("restocks_amount")]
        public int Restocks { get; set; }
    }

    [Table("GameSettings_Spider")]
    public class Settings_Spider
    {
        [Column("game_id")]
        public int GameId { get; set; }
        [Column("suits_amount")]
        public int Suits { get; set; }
    }
}