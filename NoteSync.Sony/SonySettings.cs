using System.ComponentModel;

namespace NoteSync.Sony
{
    public class SonySettings
    {
        [DefaultValue("READER")]
        public string Volume { get; set; }

        [DefaultValue("database\\media\\notepads")]
        public string Database { get; set; }
    }
}
