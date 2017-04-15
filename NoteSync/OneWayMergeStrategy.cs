namespace NoteSync
{
    public class OneWayMergeStrategy : IMergeStrategy
    {
        public bool Merge(Note left, Note right)
        {

            // Simple merge strategy. Always assumes the left side is more up to date.
            // Therefore this is a simple one way (source to target) sync.

            right.Body = left.Body;
            right.Title = left.Title;
            right.Created = left.Created;
            right.Modified = left.Modified;

            return true;
        }
    }
}