using NLog;
using System;

namespace NLogHandler
{
    public static class NLogConfigurator
    {
        #region Provate fields

        //strings that hold location of current log file
        private static readonly string _logsFolderLocation;
        private static string _currentLogFileNameApp;

        //tiimer for creation of new file
        private static NewLogFileCreatorTimer _creationTimer;

        //fields - parameters for deletion of nold log files
        private static bool _deletionOfOldLogFilesActive;
        private static int _amountOfDaysForOldFilesDeletion;
        private static int _amountOfHoursToCreateNewFile;
        
        #endregion

        #region Properties

        public static string LogFolder { get { return _logsFolderLocation; } }
        public static string AppLogFileName { get { return _currentLogFileNameApp; } }
        public static bool DeletionOfOldLogsActive { get { return _deletionOfOldLogFilesActive; } }
        public static int DeletionOldLogDays { get { return _amountOfDaysForOldFilesDeletion; } }
        public static int HoursPeriodForNewLogFileCreation { get { return _amountOfHoursToCreateNewFile; } }

        #endregion

        #region Constructor

        static NLogConfigurator()
        {
            _logsFolderLocation = $"{System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.Length - 15)}Logs\\";
            _currentLogFileNameApp = $"{_logsFolderLocation}AppLogFile_{DateTime.Now.ToString()}.txt";

            _creationTimer = null;
        }

        #endregion

        #region NLog configuration

        //main method called at the app starte
        public static void NLogConfiguration(bool active, int amountOfDays,
            int hoursToNewFile)
        {
            //safety - only can config all, at the ctartup
            if (_creationTimer == null)
            {
                //configure NLOG
                NLogConfigurationCommonMethod();

                //creation of new NLOg file parameters
                AssignDeletionOfLogFilesParameters(active, amountOfDays, hoursToNewFile);

                //Log application startup
                LogApplicationinfo("Application started");

                //delete old log files
                if (_deletionOfOldLogFilesActive) DeleteOldLogs();
            }
        }

        //common method called after NLOg changes log file (at startup
        //and after timer elapses)
        private static void NLogConfigurationCommonMethod()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var appLogfile = new NLog.Targets.FileTarget("appLogfile") { FileName = _currentLogFileNameApp };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, appLogfile);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);

            NLog.LogManager.Configuration = config;

            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("New log file created");
        }

        #endregion

        #region Handling changing time when new log file is created

        //starting timer - at the app startup
        private static void StartTimerForNewLogFileCreationInterval(int hours)
        {
            _creationTimer = new NewLogFileCreatorTimer(hours);
            _creationTimer.TimerElapsed += Creator_TimerElapsed;
        }

        //when timer of creation of new log file elapses, create new file
        private static void Creator_TimerElapsed(object sender, EventArgs e)
        {
            ChangeLogFile();
        }

        //public method for changing NLog config in runtime
        public static void ChangeNLogConfiguration(bool active, int amountOfDays,
            int hoursToNewFile)
        {
            AssignNLogParamsCommonMethod(active, amountOfDays, hoursToNewFile);
            _creationTimer.ChangeTimerBase(Math.Abs(hoursToNewFile));
        }

        //seting nlog config at the app startup
        private static void AssignDeletionOfLogFilesParameters(bool active, int amountOfDays,
            int hoursToNewFile)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"Assigning nlog parameters: deletion of old files active = {active}, amount of days = {amountOfDays}, hours to new file = {hoursToNewFile}.");

            AssignNLogParamsCommonMethod(active, amountOfDays, hoursToNewFile);
            StartTimerForNewLogFileCreationInterval(_amountOfHoursToCreateNewFile);
        }

        //changing NLog config common method
        private static void AssignNLogParamsCommonMethod(bool active, int amountOfDays,
            int hoursToNewFile)
        {
            _deletionOfOldLogFilesActive = active;
            _amountOfDaysForOldFilesDeletion = Math.Abs(amountOfDays);
            _amountOfHoursToCreateNewFile = Math.Abs(hoursToNewFile);
        }

        #endregion

        #region Changing log files - new ones

        //mai method for changing current log file
        private static void ChangeLogFile()
        {
            //log last registration in old log file
            LogApplicationinfo($"Changing log file to this one: {_currentLogFileNameApp}");

            //change log file
            _currentLogFileNameApp = $"{_logsFolderLocation}AppLogFile_{DateTime.Now.ToString()}.txt";

            //reconfigure NLog
            NLogConfigurationCommonMethod();

            if (_deletionOfOldLogFilesActive)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Info($"Because of activated option of deleting old log files, the deletion procedure is fired.");

                DeleteOldLogs();
            }
        }

        //calling anoter class - deletion of old log files
        private static void DeleteOldLogs()
        {
            OldLogFilesDeleter deleter = new OldLogFilesDeleter();
            deleter.DeleteOldLogFiles(_amountOfDaysForOldFilesDeletion);
        }

        #endregion

        #region Logging to files from other assemblies

        //method for logging INFO
        private static void LogApplicationinfo(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }

        #endregion
    }
}
