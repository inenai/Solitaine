using Common;
using Scorpion;
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

    [Table("GameSettings_Scorpion")]
    public class Settings_Scorpion
    {
        [Column("game_id")]
        public int GameId { get; set; }
        [Column("variant")]
        public ScorpionVariant Variant { get; set; }
        [Column("suits_amount")]
        public int Suits { get; set; }
    }

    [Table("SavedGames")]
    public class SavedGame
    {
        [PrimaryKey]
        [Column("solitaire_kind")]
        public SolitaireKind SolitaireKind { get; set; }
        [Column("time_seconds")]
        public int TimeSeconds { get; set; }
        [Column("rng_seed")]
        public int RandomSeed { get; set; }
        [Column("commands_list")]
        public string CommandsList { get; set; }
    }

    [Table("GameCommands")]
    public class GameCommandEntry
    {
        [PrimaryKey, AutoIncrement]
        [Column("command_id")]
        public int CommandId { get; set; }
        [Column("actions_list")]
        public string ActionsList { get; set; }
    }

    [Table("GameCommandActions")]
    public class GameCommandActionEntry
    {
        [PrimaryKey, AutoIncrement]
        [Column("action_id")]
        public int ActionId { get; set; }

        [Column("action_kind")]
        public GameCommandActionKind CommandActionKind { get; set; }

        //CARD
        [Column("card_value")]
        public int CardValue { get; set; }
        [Column("card_suit")]
        public CardSuit CardSuit { get; set; }

        //MOVE & MOVE STACK ACTIONS
        [Column("source_pile")]
        public PileKind SourcePile { get; set; }
        [Column("source_pile_index")]
        public int SourcePileIndex { get; set; }
        [Column("target_pile")]
        public PileKind TargetPile { get; set; }
        [Column("target_pile_index")]
        public int TargetPileIndex { get; set; }

        //REVEAL ACTION
        [Column("revealed_action")]
        public RevealedAction RevealedAction { get; set; }

        //FREE ACTION
        [Column("freed_action")]
        public FreedAction FreedAction { get; set; }

        //RESTOCK ACTION
        [Column("restock")]
        public bool Restock { get; set; }
    }
}