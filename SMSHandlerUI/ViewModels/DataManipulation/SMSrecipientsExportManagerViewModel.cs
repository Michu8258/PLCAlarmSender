using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsHandling;
using SMSHandlerUI.IOfilesHandling;
using SMSHandlerUI.Models;
using SMSHandlerUI.ProgressWindowEnum;
using SMSHandlerUI.RuntimeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels.DataManipulation
{
    class SMSrecipientsExportManagerViewModel : Screen
    {
        #region Filds and properties

        private readonly IWindowManager _manager;
        private readonly IRealmProvider _realmProvider;
        private readonly Logger _logger;

        //filepat to the export file be saved
        private string _filePath;

        //list of recipients
        private List<SMSrecipientDefinition> _originalListOfRecipients;

        //bindable list
        private BindableCollection<SMSrecipientExportModel> _availableRecipients;
        private SMSrecipientExportModel _selectedRecipient;
        private readonly List<int> _recipientsToExportIDlist;

        public BindableCollection<SMSrecipientExportModel> AvailableRecipients { get { return _availableRecipients; } set { _availableRecipients = value; NotifyOfPropertyChange(); } }
        public SMSrecipientExportModel SelectedRecipient { get { return _selectedRecipient; } set { _selectedRecipient = value; NotifyOfPropertyChange(() => SelectedRecipient); } }

        //export selected button enabling
        private bool _enableExportButton;
        public bool EnableExportButton { get { return _enableExportButton; } set { _enableExportButton = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public SMSrecipientsExportManagerViewModel(IWindowManager manager, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;

            AvailableRecipients = new BindableCollection<SMSrecipientExportModel>();
            _recipientsToExportIDlist = new List<int>();
            _originalListOfRecipients = new List<SMSrecipientDefinition>();
            EnableExportButton = false;

            //default filepath
            _filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Alarms profiles export";

            ReadAllRecipients();

            _logger.Info($"SMS recipients export manager window created.");
        }

        #endregion

        #region Reading all SMS recipients list from DB

        private void ReadAllRecipients()
        {
            _logger.Info($"Reading full list of SMS recipients from DB.");

            ReadRawRecipients();
            if (_originalListOfRecipients.Count > 0)
            {
                PopulateRecipientsModelCollection();
                if (AvailableRecipients.Count > 0)
                {
                    EnableExportButton = true;
                }
            }
        }

        private void ReadRawRecipients()
        {
            //create SMS recipients reader instance
            SMSrecipientReader reader = new SMSrecipientReader(_realmProvider);

            //get full list of SMS recipients
            _originalListOfRecipients = reader.GetAllActualRecipients();
        }

        private void PopulateRecipientsModelCollection()
        {
            //clear list (bindable collection)
            AvailableRecipients.Clear();

            //for every read SMS recipient
            foreach (var item in _originalListOfRecipients)
            {
                SMSrecipientExportModel model = new SMSrecipientExportModel()
                {
                    ToExport = false,
                    Identity = item.Identity,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    AreaCode = item.AreaCode,
                    PhoneNumber = item.PhoneNumber,
                };

                AvailableRecipients.Add(model);
            }
        }

        #endregion

        #region Exporting recipients

        public void ExportSelectedSMSrecipients()
        {
            _logger.Info($"Button for exporting all selected SMS recipients pressed.");

            //check if at least one SMS recipient is marked as SMS recipient to export
            if (CheckIfAtLeastOneRecipientIsSelected())
            {
                //open dialog file - location of file to save
                ShowExportFileLocationDialog();
                if (_filePath != null && _filePath.Substring(_filePath.Count() - 5, 5) == ".json")
                {
                    //create list of profiles ID to export
                    CreateListOfRecipientsToExport();

                    //exporting
                    Export();
                }
            }
            else
            {
                MessageBox.Show($"You need to choose at least one SMS recipient before export.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CheckIfAtLeastOneRecipientIsSelected()
        {
            return AvailableRecipients.Where(x => x.ToExport == true).Count() > 0;
        }

        private void ShowExportFileLocationDialog()
        {
            CreateJSONFile fileCreator = new CreateJSONFile();
            fileCreator.SaveFileDialg(ref _filePath);
        }

        private void CreateListOfRecipientsToExport()
        {
            _recipientsToExportIDlist.Clear();

            foreach (var item in AvailableRecipients)
            {
                if (item.ToExport)
                {
                    _recipientsToExportIDlist.Add(item.Identity);
                }
            }
        }

        private void Export()
        {
            _logger.Info($"Export of SMS recipients starting.");

            ProgressBarViewModel pbvm = new ProgressBarViewModel(ProgressWindowTypeEnum.RecipientsExport,
                        _filePath, _recipientsToExportIDlist, 0, _realmProvider);
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
