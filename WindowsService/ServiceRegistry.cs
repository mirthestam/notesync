using NLog;
using NoteSync;
using NoteSync.Evernote;
using NoteSync.Sony;
using SettingsProviderNet;
using StructureMap;

namespace WindowsService
{
    public class ServiceRegistry : Registry
    {
        public ServiceRegistry()
        {
            // The repositories to sync between
            For<ISourceRepository>().Use<SonyRepository>();
            For<ITargetRepository>().Use<EvernoteRepository>();

            // The trigger do determine when to syncronize
            For<ITrigger>().Use<RemovableDeviceTrigger>();

            // The strategy for how to merge notes
            For<IMergeStrategy>().Use<OneWayMergeStrategy>();

            // Default implementations
            For<ISynchronizer>().Use<Synchronizer>();

            // Logging
            For<ILogger>().Use(context => LogManager.GetLogger(context.ParentType.Name));
            
            // Settings
            For<ISettingsProvider>().Use(context => new SettingsProvider(context.GetInstance<ISettingsStorage>(), null));
            For<ISettingsStorage>().Use(context => new RoamingAppDataStorage("NoteSync"));

            For<EvernoteSettings>().Use(context => GetSettings<EvernoteSettings>(context));
            For<SonySettings>().Use(context => GetSettings<SonySettings>(context));
        }

        private static T GetSettings<T>(IContext context) where T : class, new()
        {
            var settingsProvider = context.GetInstance<ISettingsProvider>();
            var settings = settingsProvider.GetSettings<T>();
            
            // Instantly save the settings to make sure the settings file exists.
            settingsProvider.SaveSettings(settings);
            return settings;
        }
    }
}
