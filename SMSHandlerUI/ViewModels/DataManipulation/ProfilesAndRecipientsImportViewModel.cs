using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using SMSHandlerUI.IOfilesHandling;
using SMSHandlerUI.ProgressWindowEnum;
using SMSHandlerUI.RuntimeData;
using System;

namespace SMSHandlerUI.ViewModels.DataManipulation
{
    class ProfilesAndRecipientsImportViewModel : Screen
    {
        #region Fields and properties

        private readonly IWindowManager _manager;
        private readonly IRealmProvider _realmProvider;
        private readonly Logger _logger;
        private readonly int _windowType;

        //enabling of IMPORT button
        private bool _importEnabled;

        public bool ImportEnabled { get { return _importEnabled; } set { _importEnabled = value; NotifyOfPropertyChange(); } }

        //file path
        private string _filePath;
        public string FilePath { get { return _filePath; } set { _filePath = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor of SMS recipients and Alarm profiles import window
        /// </summary>
        /// <param name="manager">Caliburn micro IwWindowManager object instance</param>
        /// <param name="type">1 - alarm profiles, 2 - SMS recipients</param>
        public ProfilesAndRecipientsImportViewModel(IWindowManager manager, int type, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;
            _windowType = type;
            ImportEnabled = false;
            FilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\";

            AssignWindowName();

            _logger.Info($"Alarm import window created. Window type {_windowType.ToString()}.");
        }

        private void AssignWindowName()
        {
            //switch between window import types
            switch (_windowType)
            {
                case 1: DisplayName = "Import alarm urgency profiles"; break;
                case 2: DisplayName = "Import SMS recipients"; break;
                default: break;
            }
        }

        #endregion

        #region Opening file dialog

        public void SelectFile()
        {
            _logger.Info($"Opening window for choosing json file to import data from.");

            string filePath = FilePath;
            OpenFile fileOpener = new OpenFile();
            fileOpener.OpenFileDialog(ref filePath, false);
            FilePath = filePath;

            ActivateImporting();
        }

        #endregion

        #region Activate importing if proper json file selected

        private void ActivateImporting()
        {
            ImportEnabled = FilePath.Substring(FilePath.Length - 5, 5) == ".json";
        }

        #endregion

        #region Importing

        public void ImportData()
        {
            _logger.Info($"Button for importing data pressed.");

            //switch between window import types
            switch (_windowType)
            {
                case 1: ImportAlarmProfiles(); break;
                case 2: ImportSMSrecipients(); break;
                default: break;
            }
        }

        private void ImportAlarmProfiles()
        {
            _logger.Info($"Start to import alarm profiles.");

            ProgressBarViewModel pbvm = new ProgressBarViewModel(ProgressWindowTypeEnum.ProfileIsmport,
                FilePath, null, 0, _realmProvider);
            _manager.ShowDialog(pbvm);
        }

        private void ImportSMSrecipients()
        {
            _logger.Info($"Start to import SMS recipients.");

            ProgressBarViewModel pbvm = new ProgressBarViewModel(ProgressWindowTypeEnum.RecipientsImport,
                FilePath, null, 0, _realmProvider);
            _manager.ShowDialog(pbvm);
        }

        #endregion

        #region Closing window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing the window pressed. Closing the window.");

            TryClose();
        }

        #endregion

        #region User activity

        public void ResetLogoutTimer()
        {
            RuntimeLogoutTimer.UserActivityDetected();
        }

        #endregion
    }
}
