using System.Collections.Generic;

namespace NoteSync
{
    public interface ISourceRepository
    {
        IEnumerable<Note> GetNotes();
    }
}