using System;

namespace NoteSync
{
    public abstract class Trigger : ITrigger
    {
        public abstract void Listen();
        public abstract void Stop();

        public event EventHandler SyncRequested;

        protected void OnSyncRequested()
        {
            SyncRequested?.Invoke(this, new EventArgs());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
