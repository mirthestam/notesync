using System.Management;
using System.Threading;
using NLog;

namespace NoteSync
{
    public class RemovableDeviceTrigger : Trigger
    {
        private readonly ILogger _logger;

        public RemovableDeviceTrigger(ILogger logger)
        {
            _logger = logger;
        }

        private ManagementEventWatcher _eventWatcher;

        public override void Listen()
        {
            // Code based upon question Detecting USB drive insertion and removal using windows service and C#
            // http://stackoverflow.com/a/2988572/296526

            _logger.Debug("Start listening for Volume Change Events");

            var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            _eventWatcher = new ManagementEventWatcher(query);
            _eventWatcher.EventArrived += (sender, args) =>
            {
                _logger.Debug("Volume Event Change received. Sleeping 5000 ms for the volume to initialize.");
                _eventWatcher.Stop();
                Thread.Sleep(5000); // Wait
                _logger.Debug("Requesting sync");
                OnSyncRequested();
                _eventWatcher.Start();
                _logger.Debug("Listening for Volume Change Events");
            };
            _eventWatcher.Start();
        }

        protected override void Dispose(bool disposing)
        {
            _eventWatcher.Dispose();
            base.Dispose(disposing);            
        }

        public override void Stop()
        {
            _eventWatcher.Stop();
        }
    }
}