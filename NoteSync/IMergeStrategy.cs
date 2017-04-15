namespace NoteSync
{
    public interface IMergeStrategy
    {
        bool Merge(Note left, Note right);
    }
}