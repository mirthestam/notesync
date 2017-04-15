namespace NoteSync
{
    public interface ITargetRepository
    {
        bool TryGetNote(string id, out Note note);
        void SaveNote(Note note);
    }
}