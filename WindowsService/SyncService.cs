using System;
using NLog;
using NoteSync;

namespace WindowsService
{
    public class SyncService
    {
        private readonly ILogger _logger;
        private readonly ITrigger _trigger;
        private readonly ISynchronizer _synchronizer;

        public SyncService(ILogger logger, ITrigger trigger, ISynchronizer synchronizer)
        {
            _logger = logger;
            _trigger = trigger;
            _synchronizer = synchronizer;

            _trigger.SyncRequested += (sender, args) => Synchronize();
        }


        public void Start()
        {
            _logger.Trace("Starting service");
            _trigger.Listen();
            _logger.Info("Service started.");
        }

        public void Stop()
        {
            _logger.Trace("Stopping service");
            // TODO: Stop the trigger
            //_trigger.Stop();
            _logger.Info("Service Stopped.");
        }

        private void Synchronize()
        {
            try
            {
                _synchronizer.Synchronize();
            }
            catch (Exception e)
            {
                _logger.Warn(e);
            }
        }
    }
}
