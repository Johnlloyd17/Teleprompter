using SQLite;

namespace Teleprompter.Models
{
    [Table("AppSettings")]
    public class AppSettingModel
    {
        [PrimaryKey, NotNull]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
