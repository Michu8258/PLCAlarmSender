using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.CommonClasses;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels.AlarmManagement
{
    class DefaultS7AlarmHendlerViewModel : Screen
    {
        #region Fields and properties

        //logger
        private readonly Logger _logger;

        //information about connection
        private readonly PLCconnectionComboBoxModel _connectionData;

        //window mode - 0 = new alarm, 1 = modify alarm
        private readonly bool _windowMode;

        //data for modified alarm
        private readonly AlarmS7UImodel _modifiedAlarmData;

        //realm provider
        private readonly IRealmProvider _realmProvider;

        //runtimeData
        private IRuntimeData _runtimeData;

        #endregion

        #region Fields and properties for GUI

        //definitions for comboboxes with choice of alarm pofile and sms recipients groups.
        private BindableCollection<AlarmProfileComboBoxModel> _alarmUrgencyProfile;
        private AlarmProfileComboBoxModel _selectedAlarmUrgencyProfile;
        private BindableCollection<SMSgroupsComboBoxModel> _smsRecipientsGroups;
        private SMSgroupsComboBoxModel _selectedSMSrecipientsGroup;

        public BindableCollection<AlarmProfileComboBoxModel> AlarmUrgencyProfile { get { return _alarmUrgencyProfile; } set { _alarmUrgencyProfile = value; NotifyOfPropertyChange(); } }
        public AlarmProfileComboBoxModel SelectedAlarmUrgencyProfile { get { return _selectedAlarmUrgencyProfile; } set { _selectedAlarmUrgencyProfile = value; NotifyOfPropertyChange(() => SelectedAlarmUrgencyProfile); } }
        public BindableCollection<SMSgroupsComboBoxModel> SmsRecipientsGroups { get { return _smsRecipientsGroups; } set { _smsRecipientsGroups = value; NotifyOfPropertyChange(); } }
        public SMSgroupsComboBoxModel SelectedSMSrecipientsGroup { get { return _selectedSMSrecipientsGroup; } set { _selectedSMSrecipientsGroup = value; NotifyOfPropertyChange(() => SelectedSMSrecipientsGroup); } }

        //alarm definition
        private bool _alarmActivated;
        private string _almTagName;
        private int _almDBnumber;
        private int _almByteNumber;
        private byte _almBitNumber;
        private string _ackTagName;
        private int _ackDBnumber;
        private int _ackByteNumber;
        private byte _ackBitNumber;

        public string ConnectionName { get { return _connectionData.ConnectionName; } set { _connectionData.ConnectionName = value; NotifyOfPropertyChange(); } }
        public bool AlarmActivated { get { return _alarmActivated; } set { _alarmActivated = value; NotifyOfPropertyChange(); } }
        public string AlmTagName { get { return _almTagName; } set { _almTagName = value; NotifyOfPropertyChange(); } }
        public int AlmDBnumber { get { return _almDBnumber; } set { _almDBnumber = value; NotifyOfPropertyChange(); } }
        public int AlmByteNumber { get { return _almByteNumber; } set { _almByteNumber = value; NotifyOfPropertyChange(); } }
        public byte AlmBitNumber { get { return _almBitNumber; } set { _almBitNumber = value; NotifyOfPropertyChange(); } }
        public string AckTagName { get { return _ackTagName; } set { _ackTagName = value; NotifyOfPropertyChange(); } }
        public int AckDBnumber { get { return _ackDBnumber; } set { _ackDBnumber = value; NotifyOfPropertyChange(); } }
        public int AckByteNumber { get { return _ackByteNumber; } set { _ackByteNumber = value; NotifyOfPropertyChange(); } }
        public byte AckBitNumber { get { return _ackBitNumber; } set { _ackBitNumber = value; NotifyOfPropertyChange(); } }

        //alarm texts for SMSes
        private List<S7AlarmEnabledTextsModel> _listOfAllAlarmTexts;
        private BindableCollection<S7AlarmEnabledTextsModel> _listOfEditableTexts;
        private S7AlarmEnabledTextsModel _selectedEditableText;

        public BindableCollection<S7AlarmEnabledTextsModel> ListOfEditableTexts { get { return _listOfEditableTexts; } set { _listOfEditableTexts = value; NotifyOfPropertyChange(); } }
        public S7AlarmEnabledTextsModel SelectedEditableText { get { return _selectedEditableText; } set { _selectedEditableText = value; NotifyOfPropertyChange(() => SelectedEditableText); } }

        #endregion

        #region Constructor

        public DefaultS7AlarmHendlerViewModel(PLCconnectionComboBoxModel connectionData, bool windowMode,
            AlarmS7UImodel modifiedAlarmData, IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            _realmProvider = realmProvider;
            _runtimeData = runtimeData;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            //assign data of modified alarm
            _modifiedAlarmData = modifiedAlarmData;

            //assing window parameters
            _connectionData = connectionData;
            _windowMode = windowMode;

            //assign connection name
            ConnectionName = _connectionData.ConnectionName;

            //Assign data of modified alarm(in case of alarm modification)
            AssignWindowData(modifiedAlarmData);

            _logger.Info($"Default S7 alarm handler window created.");
        }

        #endregion

        #region Applying changes

        public void ApplyAndCLose()
        {
            _logger.Info($"Button for applying changes pressed; window mode is: {_windowMode}.");

            bool dataOK = ExecuteDataCorrectnessCheck();
            if (dataOK)
            {
                if (!_windowMode) //new alarm
                {
                    _logger.Info($"Attempt to save new S7 alarm to DB.");

                    bool done = SendDataOfNewS7AlarmToDB();
                    if (!done)
                    {
                        MessageBox.Show($"Error while trying to save new S7 alarm to DB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        TryClose();
                    }
                }
                else //modify alarm
                {
                    _logger.Info($"Attempt to save modifications of existing S7 alarm. Alarm ID: {_modifiedAlarmData.Identity}.");

                    AlarmS7UImodel model = GatherDataForModifyingExistingS7Alarm();
                    bool done = SavingDataOfModifiedS7Alarm(model);
                    if (!done)
                    {
                        MessageBox.Show($"Error while trying to save S7 alarm modifications to DB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        TryClose();
                    }
                }
            }
        }

        private bool SavingDataOfModifiedS7Alarm(AlarmS7UImodel model)
        {
            _logger.Info($"Procedure of sending gathered data about modified alarm, to DB, started.");

            AlarmS7Modifier modifier = new AlarmS7Modifier(_realmProvider);
            return modifier.ModifySingleS7Alarm(model);
        }

        #endregion

        #region Gathering data to modify existing alarm

        private AlarmS7UImodel GatherDataForModifyingExistingS7Alarm()
        {
            _logger.Info($"Gathering data for modification of existing S7 alarm stated.");

            AlarmS7UImodel outputModel = new AlarmS7UImodel()
            {
                Identity = _modifiedAlarmData.Identity,
                PLCconnectionID = _modifiedAlarmData.PLCconnectionID,
                AlarmProfileIdentity = SelectedAlarmUrgencyProfile.Identity,
                SMSrecipientsGroupIdentity = SelectedSMSrecipientsGroup.Identity,
                AlarmActivated = AlarmActivated,
                AlarmTagName = AlmTagName,
                AlarmTagDBnumber = AlmDBnumber,
                AlarmTagByteNumber = AlmByteNumber,
                AlarmTagBitNumber = AlmBitNumber,
                AckTagName = AckTagName,
                AckTagDBnumber = AckDBnumber,
                AckTagByteNumber = AckByteNumber,
                AckTagBitNumber = AckBitNumber,
                SysLang1 = _listOfAllAlarmTexts[0].AlarmText,
                SysLang2 = _listOfAllAlarmTexts[1].AlarmText,
                SysLang3 = _listOfAllAlarmTexts[2].AlarmText,
                SysLang4 = _listOfAllAlarmTexts[3].AlarmText,
                SysLang5 = _listOfAllAlarmTexts[4].AlarmText,
                SysLang6 = _listOfAllAlarmTexts[5].AlarmText,
                SysLang7 = _listOfAllAlarmTexts[6].AlarmText,
                UserLang1 = _listOfAllAlarmTexts[7].AlarmText,
                UserLang2 = _listOfAllAlarmTexts[8].AlarmText,
                UserLang3 = _listOfAllAlarmTexts[9].AlarmText,
                UserLang4 = _listOfAllAlarmTexts[10].AlarmText,
                UserLang5 = _listOfAllAlarmTexts[11].AlarmText,
                UserLang6 = _listOfAllAlarmTexts[12].AlarmText,
                UserLang7 = _listOfAllAlarmTexts[13].AlarmText,
                UserLang8 = _listOfAllAlarmTexts[14].AlarmText,
                UserLang9 = _listOfAllAlarmTexts[15].AlarmText,
            };

            return outputModel;
        }

        private bool SendDataOfNewS7AlarmToDB()
        {
            _logger.Info($"Gathering data for definition of new S7 alarms and texts.");

            List<string> texts = new List<string>();
            foreach (var item in _listOfAllAlarmTexts)
            {
                texts.Add(item.AlarmText);
            }

            _logger.Info($"Attempt of adding new S7 alarm definition to DB.");

            AlarmS7Creator creator = new AlarmS7Creator(_realmProvider);
            return creator.AddNewS7Alarm(_connectionData.PLCconnectionID, SelectedAlarmUrgencyProfile.Identity,
                SelectedSMSrecipientsGroup.Identity, AlarmActivated, AlmTagName, AlmDBnumber, AlmByteNumber,
                AlmBitNumber, AckTagName, AckDBnumber, AckByteNumber, AckBitNumber, texts);
        }

        #endregion

        #region Checking inputed data correctness

        private bool ExecuteDataCorrectnessCheck()
        {
            _logger.Info($"Checking correctness of inputed data procedure started.");

            bool OK = CheckIfAlarmProfileIsSelected();
            if (OK) OK = CheckIfSMSrecipientsGroupIsSelected();
            if (OK) OK = CheckTagName("alarm", AlmTagName);
            if (OK) OK = CheckIfAlarmTagIsOK();
            if (OK) OK = CheckTagName("acknowledgement", AckTagName);
            if (OK) OK = CheckIfAckTagIsOK();

            return OK;
        }

        private bool CheckIfAlarmProfileIsSelected()
        {
            bool selected = SelectedAlarmUrgencyProfile != null;
            if (!selected)
            {
                MessageBox.Show("Before saving the alarm, you have to choose alarm profile name.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool CheckIfSMSrecipientsGroupIsSelected()
        {
            bool selected = SelectedSMSrecipientsGroup != null;
            if (!selected)
            {
                MessageBox.Show("Before saving the alarm, you have to choose SMS recipients group.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool CheckIfAlarmTagIsOK()
        {
            bool OK = CheckDBnumber("alarm", AlmDBnumber);
            if (OK) OK = CheckByteNumber("alarm", AlmByteNumber);
            if (OK) OK = CheckBitNumber("alarm", AlmBitNumber);
            return OK;
        }

        private bool CheckIfAckTagIsOK()
        {
            bool OK = CheckDBnumber("acknowledgement", AckDBnumber);
            if (OK) OK = CheckByteNumber("acknowledgement", AckByteNumber);
            if (OK) OK = CheckBitNumber("acknowledgement", AckBitNumber);
            return OK;
        }

        private bool CheckTagName(string alarmOrAck, string value)
        {
            bool OK = value.Length >= 5;
            if (!OK)
            {
                MessageBox.Show($"The {alarmOrAck} tag name has to have at least five characters of length.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool CheckDBnumber(string alarmOrAck, int value)
        {
            bool ok = value > 0 && value < 65536;
            if (!ok)
            {
                MessageBox.Show($"DB number of {alarmOrAck} tag sholud be greather than 0 and less than 65536.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool CheckByteNumber(string alarmOrAck, int value)
        {
            bool ok = value >= 0 && value < 65536;
            if (!ok)
            {
                MessageBox.Show($"The byte number of {alarmOrAck} tag sholud be greather or equal to 0 and less than 65536.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool CheckBitNumber(string alarmOrAck, int value)
        {
            bool ok = value >= 0 && value < 8;
            if (!ok)
            {
                MessageBox.Show($"The Bit of {alarmOrAck} tag sholud be greather or equal to 0 and less than 8.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        #endregion

        #region Data assignment

        private void AssignWindowData(AlarmS7UImodel modifiedAlarmData)
        {
            _logger.Info($"Assigning properties of window at the window creation.");

            GetListOfActualProfiles();
            GetListOfActualSMSgroups();
            AssignAlarmDefinitionData(modifiedAlarmData);
            AssignAlarmTexts(modifiedAlarmData);
            PopulateListViewWithAlarmTexts();

            //in case of modifying - assign sms recipients group and alarm profile names to selected items in comboboxes
            if (modifiedAlarmData != null)
            {
                SelectedAlarmUrgencyProfile = AlarmUrgencyProfile.Where(x => x.Identity == modifiedAlarmData.AlarmProfileIdentity).First();
                SelectedSMSrecipientsGroup = SmsRecipientsGroups.Where(x => x.Identity == modifiedAlarmData.SMSrecipientsGroupIdentity).First();
            }
        }

        private void AssignAlarmDefinitionData(AlarmS7UImodel modifiedAlarmData)
        {
            _logger.Info($"Assigning S7 alarm definition data.");

            if (modifiedAlarmData == null) //creating new alarm
            {
                AlarmActivated = true;
                AlmTagName = "";
                AlmDBnumber = 0;
                AlmByteNumber = 0;
                AlmBitNumber = 0;
                AckTagName = "";
                AckDBnumber = 0;
                AckByteNumber = 0;
                AckBitNumber = 0;
            }
            else //modifying alarm
            {
                AlarmActivated = modifiedAlarmData.AlarmActivated;
                AlmTagName = modifiedAlarmData.AlarmTagName;
                AlmDBnumber = modifiedAlarmData.AlarmTagDBnumber;
                AlmByteNumber = modifiedAlarmData.AlarmTagByteNumber;
                AlmBitNumber = modifiedAlarmData.AlarmTagBitNumber;
                AckTagName = modifiedAlarmData.AckTagName;
                AckDBnumber = modifiedAlarmData.AckTagDBnumber;
                AckByteNumber = modifiedAlarmData.AckTagByteNumber;
                AckBitNumber = modifiedAlarmData.AckTagBitNumber;
            }
        }

        private void AssignAlarmTexts(AlarmS7UImodel modifiedAlarmData)
        {
            _logger.Info($"Assgning S7 alarm texts.");

            //create list of alarm texts = data passed in constructor. Otained from DB
            List<string> currentTexts = CreateAlarmsList(modifiedAlarmData);

            //clear and initiate instance of all texts
            _listOfAllAlarmTexts = new List<S7AlarmEnabledTextsModel>();

            for (int i = 0; i < _runtimeData.CustomLanguageList.Count; i++)
            {
                //if new alarm, alarm texts are "", but in modification mode, get thse from DB
                string alarmText;
                if (currentTexts.Count < 16) alarmText = "";
                else alarmText = currentTexts[i];

                //add new wlement to list with all texts - not showed in GUI
                _listOfAllAlarmTexts.Add(new S7AlarmEnabledTextsModel()
                {
                    LanguageEnabled = _runtimeData.CustomLanguageList[i].Enabled,
                    LanguageName = _runtimeData.CustomLanguageList[i].Language,
                    AlarmText = alarmText,
                    AlarmTextIdex = i,
                });
            }
        }

        private List<string> CreateAlarmsList(AlarmS7UImodel modifiedAlarmData)
        {
            _logger.Info($"Creating list of alarm texts = 16 elements in List.");

            List<string> alarmTexts = new List<string>();
            if (modifiedAlarmData != null)
            {
                alarmTexts.Add(modifiedAlarmData.SysLang1);
                alarmTexts.Add(modifiedAlarmData.SysLang2);
                alarmTexts.Add(modifiedAlarmData.SysLang3);
                alarmTexts.Add(modifiedAlarmData.SysLang4);
                alarmTexts.Add(modifiedAlarmData.SysLang5);
                alarmTexts.Add(modifiedAlarmData.SysLang6);
                alarmTexts.Add(modifiedAlarmData.SysLang7);
                alarmTexts.Add(modifiedAlarmData.UserLang1);
                alarmTexts.Add(modifiedAlarmData.UserLang2);
                alarmTexts.Add(modifiedAlarmData.UserLang3);
                alarmTexts.Add(modifiedAlarmData.UserLang4);
                alarmTexts.Add(modifiedAlarmData.UserLang5);
                alarmTexts.Add(modifiedAlarmData.UserLang6);
                alarmTexts.Add(modifiedAlarmData.UserLang7);
                alarmTexts.Add(modifiedAlarmData.UserLang8);
                alarmTexts.Add(modifiedAlarmData.UserLang9);
            }

            return alarmTexts;
        }

        private void PopulateListViewWithAlarmTexts()
        {
            ListOfEditableTexts = new BindableCollection<S7AlarmEnabledTextsModel>();

            int test = _runtimeData.DataOfCurrentlyLoggedUser.LangEditPrevilages;
            int counter = 0;

            foreach (var item in _listOfAllAlarmTexts)
            {
                if (item.LanguageEnabled && (test & (1 << counter)) != 0)
                {
                    ListOfEditableTexts.Add(item);
                }
                counter++;
            }

            SelectedEditableText = null;
        }

        private void GetListOfActualProfiles()
        {
            _logger.Info($"Reading list of currently defined alarm urgency profiles.");

            //initialize comboBox list
            AlarmUrgencyProfile = new BindableCollection<AlarmProfileComboBoxModel>();
            SelectedAlarmUrgencyProfile = null;

            //assign new items to comboBox
            AlarmUrgencyProfile = AlarmProfilesAndSMSgroupsReader.GetListOfAlarmProfileModels(_realmProvider);
        }

        private void GetListOfActualSMSgroups()
        {
            _logger.Info($"Reading list of currently defined SMS recipients groups.");

            //initialize comboBox list
            SmsRecipientsGroups = new BindableCollection<SMSgroupsComboBoxModel>();
            SelectedSMSrecipientsGroup = null;

            //assign new items to comboBox
            SmsRecipientsGroups = AlarmProfilesAndSMSgroupsReader.GetListOfSMSrecipientsModels(_realmProvider);
        }

        #endregion

        #region Cancel button

        public void CloseTheWindow()
        {
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
