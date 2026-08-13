using SQLite;

namespace Teleprompter.Models
{
    [Table("PlaybackHistory")]
    public class PlaybackHistoryModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, NotNull]
        public int ScriptId { get; set; }

        public double LastPositionPercent { get; set; }

        public DateTime LastOpenedAt { get; set; }
    }
}
