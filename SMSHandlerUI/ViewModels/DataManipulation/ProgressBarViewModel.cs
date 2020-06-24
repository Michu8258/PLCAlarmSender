using Caliburn.Micro;
using DataEdportImport.AlarmProfilesHandling;
using DataEdportImport.AlarmsHandling;
using DataEdportImport.SMSrecipientsHandling;
using NLog;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.CommonClasses;
using SMSHandlerUI.Models;
using SMSHandlerUI.ProgressWindowEnum;
using SMSHandlerUI.RuntimeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SMSHandlerUI.ViewModels.DataManipulation
{
    class ProgressBarViewModel : Screen
    {
        #region Fields and properties

        //placeholder for window type
        private readonly ProgressWindowTypeEnum _windowType;

        //logger
        private readonly Logger _logger;

        //realm provider
        private readonly IRealmProvider _realmProvider;

        //file location
        private readonly string _fileLocation;

        //progres bar data
        private int _lowLimit;
        private int _highLimit;
        private int _progressBarCurrentValue;

        public int LowLimit { get { return _lowLimit; } set { _lowLimit = value; NotifyOfPropertyChange(); } }
        public int HighLimit { get { return _highLimit; } set { _highLimit = value; NotifyOfPropertyChange(); } }
        public int ProgressBarCurrentValue { get { return _progressBarCurrentValue; } set { _progressBarCurrentValue = value; NotifyOfPropertyChange(); } }

        //objects to export
        private List<int> _listToExportID;
        private int _plcConnectionID;

        //synchronization context
        private readonly SynchronizationContext _synchCont;

        //close button
        private bool _closingButtonEnabled;

        public bool ClosingButtonEnabled { get { return _closingButtonEnabled; } set { _closingButtonEnabled = value; NotifyOfPropertyChange(); } }

        //progress events list
        private BindableCollection<ImportExportEventModel> _progressEventsList;
        private ImportExportEventModel _selectedProgressEvent;

        public BindableCollection<ImportExportEventModel> ProgressEventsList { get { return _progressEventsList; } set { _progressEventsList = value; NotifyOfPropertyChange(); } }
        public ImportExportEventModel SelectedProgressEvent { get { return _selectedProgressEvent; } set { _selectedProgressEvent = value; NotifyOfPropertyChange(() => SelectedProgressEvent); } }

        #endregion

        #region Constructor

        public ProgressBarViewModel(ProgressWindowTypeEnum windowType, string fileLocation, List<int> dataToExport,
            int plcConnectionIID, IRealmProvider realmProvider, List<AlarmS7UImodel> winCCAlarms = null)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _synchCont = SynchronizationContext.Current;

            ProgressEventsList = new BindableCollection<ImportExportEventModel>();
            _listToExportID = new List<int>();
            _windowType = windowType;
            _fileLocation = fileLocation;
            ClosingButtonEnabled = false;
            AssignWindowTitle();

            _logger.Info($"Export/Import progress bar window created for type {_windowType.ToString()}.");

            if (_windowType == ProgressWindowTypeEnum.AlarmsExport || _windowType == ProgressWindowTypeEnum.ProfilesExport || _windowType == ProgressWindowTypeEnum.RecipientsExport)
            {
                ExportData(dataToExport, plcConnectionIID);
            }
            else if (_windowType == ProgressWindowTypeEnum.WinCCalarmsImport)
            {
                StartWinCCImportTask(plcConnectionIID, winCCAlarms);
            }
            else
            {
                ImportData(plcConnectionIID);
            }
        }

        private void AssignWindowTitle()
        {
            switch (_windowType)
            {
                case ProgressWindowTypeEnum.AlarmsExport: DisplayName = "Alarms export progress"; break;
                case ProgressWindowTypeEnum.AlarmsImport: DisplayName = "Alarms import progress"; break;
                case ProgressWindowTypeEnum.ProfilesExport: DisplayName = "Alarm profiles export progress"; break;
                case ProgressWindowTypeEnum.ProfileIsmport: DisplayName = "Alarm profiles import progress"; break;
                case ProgressWindowTypeEnum.RecipientsExport: DisplayName = "SMS recipients export progress"; break;
                case ProgressWindowTypeEnum.RecipientsImport: DisplayName = "SMS recipients import progress"; break;
                case ProgressWindowTypeEnum.WinCCalarmsImport: DisplayName = "WinCC alarms import progress"; break;
                default: DisplayName = "Export/import progress"; break;
            }
        }

        #endregion

        #region Passing data to export

        private void ExportData(List<int> dataToExport, int plcConnectionIID)
        {
            _logger.Info($"Method for passing in list of data to export fired.");

            LowLimit = 0;
            HighLimit = dataToExport.Count();
            ProgressBarCurrentValue = 0;

            _listToExportID = dataToExport;
            _plcConnectionID = plcConnectionIID;
            Export();
        }

        #endregion

        #region Exporting data

        private void Export()
        {
            switch (_windowType)
            {
                case ProgressWindowTypeEnum.AlarmsExport: Task.Run(() => ExportAlarms()); break;
                case ProgressWindowTypeEnum.ProfilesExport: Task.Run(() => ExportAlarmProfiles()); break;
                case ProgressWindowTypeEnum.RecipientsExport: Task.Run(() => ExportSMSrecipients()); break;
                default: break;
            }
        }

        private void ExportAlarms()
        {
            try
            {
                AlarmExport exporter = new AlarmExport(_listToExportID, _plcConnectionID, _fileLocation, _realmProvider);
                exporter.ImportExportUpdate += Exporter_OneExportDone;
                exporter.StartExport();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to export Alarms data to JSON. ERROR: {ex.Message}.");
                _synchCont.Post(_ => MessageBox.Show($"Error while exporting alarms. Alarms not exported properly!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error), null);
                _synchCont.Post(_ => ClosingButtonEnabled = true, null);
            }
        }

        private void ExportAlarmProfiles()
        {
            try
            {
                AlarmProfileExport exporter = new AlarmProfileExport(_listToExportID, _fileLocation, _realmProvider);
                exporter.ImportExportUpdate += Exporter_OneExportDone;
                exporter.StartExport();

            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to export alarm urgency profiles data to JSON. ERROR: {ex.Message}.");
                _synchCont.Post(_ => MessageBox.Show($"Error while exporting alarm urgency profiles. Alarm profiles not exported properly!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error), null);
                _synchCont.Post(_ => ClosingButtonEnabled = true, null);
            }
        }

        private void ExportSMSrecipients()
        {
            try
            {
                SMSrecipientsExport exporter = new SMSrecipientsExport(_listToExportID, _fileLocation, _realmProvider);
                exporter.ImportExportUpdate += Exporter_OneExportDone;
                exporter.StartExport();

            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to export SMS recipients data to JSON. ERROR: {ex.Message}.");
                _synchCont.Post(_ => MessageBox.Show($"Error while exporting SMS recipients. Recipients not exported properly!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error), null);
                _synchCont.Post(_ => ClosingButtonEnabled = true, null);
            }
        }

        private void Exporter_OneExportDone(object sender, DataEdportImport.Common.ExportImportEventTextEventArgs e)
        {
            _synchCont.Post(_ => ClosingButtonEnabled = true, null);
            _synchCont.Post(_ => ProgressBarCurrentValue = HighLimit, null);
            _synchCont.Post(_ => ProgressEventsList.Add(new ImportExportEventModel() { Success = e.Success, ObjectName = e.ObjectName, Message = e.MessageText }), null);
        }

        #endregion

        #region Importing data

        private void ImportData(int plcConnectionID)
        {
            _logger.Info($"Method forimporting data fired.");

            LowLimit = 0;
            ProgressBarCurrentValue = 0;
            Import(plcConnectionID);
        }

        private void Import(int plcConnectionID)
        {
            switch (_windowType)
            {
                case ProgressWindowTypeEnum.AlarmsImport: Task.Run(() => ImportAlarms(plcConnectionID)); break;
                case ProgressWindowTypeEnum.ProfileIsmport: Task.Run(() => ImportAlarmProfiles()); break;
                case ProgressWindowTypeEnum.RecipientsImport: Task.Run(() => ImportSMSrecipients()); break;
                default: break;
            }
        }

        private void ImportAlarms(int plcConnectionID)
        {
            try
            {
                AlarmImport importer = new AlarmImport(_fileLocation, plcConnectionID, _realmProvider);
                importer.SingleImportDone += Importer_SingleImportDone;
                importer.ImportStart += Importer_ImportStart;
                importer.Import();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to import Alarms from JSON. ERROR: {ex.Message}.");
                _synchCont.Post(_ => MessageBox.Show($"Error while importing alarms. Alarms not imported properly!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error), null);
                _synchCont.Post(_ => ClosingButtonEnabled = true, null);
            }
        }

        private void ImportAlarmProfiles()
        {
            try
            {
                AlarmProfileImport importer = new AlarmProfileImport(_fileLocation, _realmProvider);
                importer.SingleImportDone += Importer_SingleImportDone;
                importer.ImportStart += Importer_ImportStart;
                importer.Import();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to import alarm urgency profiles from JSON. ERROR: {ex.Message}.");
                _synchCont.Post(_ => MessageBox.Show($"Error while importing alarm urgecy profiles. Profiles not imported properly!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error), null);
                _synchCont.Post(_ => ClosingButtonEnabled = true, null);
            }
        }

        private void ImportSMSrecipients()
        {
            try
            {
                SMSrecipientsImport importer = new SMSrecipientsImport(_fileLocation, _realmProvider);
                importer.SingleImportDone += Importer_SingleImportDone;
                importer.ImportStart += Importer_ImportStart;
                importer.Import();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to import SMS recipients from JSON. ERROR: {ex.Message}.");
                _synchCont.Post(_ => MessageBox.Show($"Error while importing SMS recipients. Recipients not imported properly!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error), null);
                _synchCont.Post(_ => ClosingButtonEnabled = true, null);
            }
        }

        private void Importer_ImportStart(object sender, DataEdportImport.Common.ImportElementsCountEventArgs e)
        {
            _synchCont.Post(_ => HighLimit = e.MaxValueOfProgressBar, null);
        }

        private void Importer_SingleImportDone(object sender, DataEdportImport.Common.ExportImportEventTextEventArgs e)
        {
            _synchCont.Post(_ => ClosingButtonEnabled = e.Done, null);
            _synchCont.Post(_ => ProgressBarCurrentValue++, null);
            _synchCont.Post(_ => ProgressEventsList.Add(new ImportExportEventModel() { Success = e.Success, ObjectName = e.ObjectName, Message = e.MessageText }), null);
        }

        #endregion

        #region Importing WinCC alarms

        private void StartWinCCImportTask(int plcConnectionID, List<AlarmS7UImodel> winCCAlarms)
        {
            Task.Run(() => ImportWinCCAlarms(plcConnectionID, winCCAlarms));
        }

        private void ImportWinCCAlarms(int plcConnectionID, List<AlarmS7UImodel> winCCAlarms)
        {
            try
            {
                AlarmImport importer = new AlarmImport(plcConnectionID, winCCAlarms, _realmProvider);
                importer.SingleImportDone += Importer_SingleImportDone;
                importer.ImportStart += Importer_ImportStart;
                importer.ImportWinCCAlarms();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to import Alarms from JSON. ERROR: {ex.Message}.");
                _synchCont.Post(_ => MessageBox.Show($"Error while importing alarms. Alarms not imported properly!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error), null);
                _synchCont.Post(_ => ClosingButtonEnabled = true, null);
            }
        }

        #endregion

        #region Closing window

        public void CloseButton()
        {
            _logger.Info($"Button for closing progress window pressed.");
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
