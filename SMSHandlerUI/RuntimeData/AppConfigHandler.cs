using NLogHandler;
using RealmDBHandler.NLogConfig;
using RealmDBHandler.RealmObjects;
using NLog;
using RealmDBHandler.CommonClasses;

namespace SMSHandlerUI.RuntimeData
{
    public static class AppConfigHandler
    {
        //logger
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //realmProvider
        private static IRealmProvider _realmProvider;

        #region App ctartup configuration

        //method for executing app config algorithm
        public static void ExecuteStertupConfig(IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            //provider
            _realmProvider = realmProvider;

            //NLog config
            ConfigNLogForFirstTime();

            //read runtime languages config from DB
            runtimeData.RefreshRuntimeLanguagesList();
        }

        #endregion

        #region Application logs - NLOG

        //assigning current NLog config at the application startup
        private static void ConfigNLogForFirstTime()
        {
            _logger.Info($"Configuring NLog at application startup started.");

            NLogConfigurationDefinition nlogCurrentConfig = GetCurrentNLogCOnfig();
            NLogConfigurator.NLogConfiguration(nlogCurrentConfig.OldLogDeletion, nlogCurrentConfig.OldLogDeletionDays,
                nlogCurrentConfig.HoursToCreateNewLogFile);
        }

        //getting current config grom DB
        private static NLogConfigurationDefinition GetCurrentNLogCOnfig()
        {
            NlogConfigReader reader = new NlogConfigReader(_realmProvider);
            return reader.GetCurrentConfig();
        }

        //change NLog config in runtime
        public static void ChangeCurrentNLogConfig()
        {
            _logger.Info($"Changing current NLog configuration procedure started.");

            NLogConfigurationDefinition nlogCurrentConfig = GetCurrentNLogCOnfig();
            NLogConfigurator.ChangeNLogConfiguration(nlogCurrentConfig.OldLogDeletion, nlogCurrentConfig.OldLogDeletionDays,
                nlogCurrentConfig.HoursToCreateNewLogFile);
        }

        #endregion

        #region Runtime languages handling

        //change languages list in runtime
        public static void RefreshRuntimeLanguagesList(IRuntimeData runtimeData)
        {
            _logger.Info($"Refreshing current runtime languages list.");

            runtimeData.RefreshRuntimeLanguagesList();
        }

        #endregion
    }
}
