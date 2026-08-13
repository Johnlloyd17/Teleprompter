using SQLite;

namespace Teleprompter.Models
{
    [Table("Scripts")]
    public class ScriptModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Title { get; set; } = string.Empty;

        [NotNull]
        public string Content { get; set; } = string.Empty;

        [Indexed]
        public int? FolderId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int WordCount { get; set; }
    }
}
