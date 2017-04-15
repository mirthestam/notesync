namespace NoteSync
{
    public class InstantTrigger : Trigger
    {
        public override void Listen()
        {
            // Instantly trigger the sync
            OnSyncRequested();            
        }

        public override void Stop()
        {            
        }
    }
}