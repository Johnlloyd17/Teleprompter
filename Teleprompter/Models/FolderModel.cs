using SQLite;

namespace Teleprompter.Models
{
    [Table("Folders")]
    public class FolderModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
