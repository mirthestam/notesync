using StructureMap;
using Topshelf;

namespace WindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // Initialize the IoC Container
            var container = new Container(new ServiceRegistry());

            // Run the service
            HostFactory.Run(configurator =>
            {
                configurator.Service<SyncService>(s =>
                {
                    s.ConstructUsing(f => container.GetInstance<SyncService>());
                    s.WhenStarted(ss => ss.Start());
                    s.WhenStopped(ss => ss.Stop());
                });

                configurator.UseNLog();

                configurator.RunAsLocalSystem();
                configurator.StartAutomatically();

                configurator.SetDescription("NoteSync Host");
                configurator.SetDisplayName("NoteSync");
                configurator.SetServiceName("NoteSync");
            });
        }
    }
}
