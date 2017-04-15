using System;

namespace NoteSync
{
    public interface ITrigger : IDisposable
    {
        void Listen();
        void Stop();
        event EventHandler SyncRequested;
    }
}