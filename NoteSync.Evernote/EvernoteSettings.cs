using System.ComponentModel;

namespace NoteSync.Evernote
{
    public class EvernoteSettings
    {
        public string SessionDeveloperToken { get; set; }

        public string SessionNoteStoreUrl { get; set; }

        [DefaultValue("NoteSync")]
        public string Tag { get; set; }
    }
}