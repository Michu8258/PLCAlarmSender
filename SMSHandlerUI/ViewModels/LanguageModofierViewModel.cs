using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmLanguagesTexts;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class LanguageModofierViewModel : Screen
    {
        #region Fields and properties

        //fields
        private LanguageModificationModel _selectedLanguage;
        private BindableCollection<LanguageModificationModel> _languages;
        private List<AlarmLanguageDefinition> _originalLanguagesList;
        private readonly Logger _logger;
        private readonly IRealmProvider _realmProvider;
        private IRuntimeData _runtimeData;

        //properties
        public LanguageModificationModel SelectedLanguage { get { return _selectedLanguage; } set { _selectedLanguage = value; NotifyOfPropertyChange(() => SelectedLanguage); } }
        public BindableCollection<LanguageModificationModel> Languages { get { return _languages; } set { _languages = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public LanguageModofierViewModel(IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            _realmProvider = realmProvider;
            _runtimeData = runtimeData;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            AssignLanguages();

            _logger.Info($"Language modifier window created.");
        }

        #endregion

        #region Reading languages list from DB

        private void AssignLanguages()
        {
            _logger.Info($"Reading alarms languages from DB started.");

            Languages = new BindableCollection<LanguageModificationModel>();
            _originalLanguagesList = ReadAllData();

            if (_originalLanguagesList == null || _originalLanguagesList.Count == 0)
            {
                _logger.Error($"While reading langiages list from DB, received list was null or had 0 elements.");
                MessageBox.Show("Error while reading data from DB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            foreach (var item in _originalLanguagesList)
            {
                Languages.Add(new LanguageModificationModel()
                {
                    Identity = item.Identity,
                    LanguageName = item.LanguageName,
                    LanguageText = item.LanguageText,
                    Editable = item.Identity >= 8,
                    Enabled = item.LanguageEnabled,
                    Selected = item.LanguageSelected,
                });
            }
        }

        private List<AlarmLanguageDefinition> ReadAllData()
        {
            SavedLanguagesReader reader = new SavedLanguagesReader(_realmProvider);
            return reader.GetLanguagesList();
        }

        #endregion

        #region Handling the enable and selected language checkboxes

        //deselecting language enagled checkbox
        public void EnableCheckBoxUnchhecked(int identity)
        {
            //chacking if it is last enabled language
            if (Languages.Where(x => x.Enabled).ToList().Count == 0)
            {
                Languages.Where(x => x.Identity == identity).ToList().ForEach(x => x.Enabled = true);
            }
            else
            {
                //selection of new selected language
                bool selected = Languages.Single(x => x.Identity == identity).Selected;
                if (selected)
                {
                    UnselectAllLanguages();
                    SelectNewSelectedLanguage();
                }
            }
        }

        //method for selecting firs enabled selected checkbox
        private void SelectNewSelectedLanguage()
        {
            foreach (var item in Languages)
            {
                if (item.Enabled)
                {
                    item.Selected = true;
                    _logger.Info($"New current alarn language choosen: {item.LanguageText}.");
                    break;
                }
            }
        }

        //metchod for deselecting all checkboxes of selected language
        private void UnselectAllLanguages()
        {
            foreach (var item in Languages)
            {
                item.Selected = false;
            }
        }

        //method for selecting checkbox of selected language
        public void LangSelectionCheckBoxClicked(int identity)
        {

            if (Languages.Single(x => x.Identity == identity).Selected)
            {
                Languages.Where(x => x.Identity != identity).ToList().ForEach(x => x.Selected = false);
            }
            else
            {
                SelectNewSelectedLanguage();
            }
        }

        #endregion

        #region CLosing the window

        //reaction for sanceling button
        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing the language modofier window pressed.");

            TryClose();
        }

        #endregion

        #region Updating changed languages

        //reaction for applying changes button
        public void ModifyAlarmLanguages()
        {
            bool dataOK = CheckIfSuchLanguagAlreadyExists();

            if (dataOK)
            {
                _logger.Info($"Saving alarm texts languages modifications to DB started.");

                bool savedSuccessfully = SendModifiedDataToDB();
                if (!savedSuccessfully)
                {
                    _logger.Error($"Saving modified alarm texts languages list went wrong!");
                    MessageBox.Show("Modifying languages data was not fully successfull", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    AppConfigHandler.RefreshRuntimeLanguagesList(_runtimeData);
                    TryClose();
                }
            }
            else
            {
                _logger.Info($"Two the same languages names - attempt made by user.");
                MessageBox.Show("There can't be two same languages", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //algorithm method - writing modified data to DB
        private bool SendModifiedDataToDB()
        {
            //create instance of modifying class
            SavedLanguagesModifier modifier = new SavedLanguagesModifier(_realmProvider);

            bool dataSavedSuccessfully = true;

            foreach (var item in Languages)
            {
                AlarmLanguageDefinition definition = _originalLanguagesList.Single(x => x.Identity == item.Identity);
                //if there is async difference beetween original and actual text, send new data to DB
                if (definition.LanguageText != item.LanguageText || definition.LanguageEnabled != item.Enabled || definition.LanguageSelected != item.Selected)
                {
                    //check if modification was saved into db properly
                    if (!modifier.ModifyLanguageText(item.Identity, item.LanguageText, item.Enabled, item.Selected))
                    {
                        dataSavedSuccessfully = false;
                    }
                }
            }

            return dataSavedSuccessfully;
        }

        //checking if any language was created twice
        private bool CheckIfSuchLanguagAlreadyExists()
        {
            bool namesOK = true;
            int amount;

            foreach (var item1 in Languages)
            {
                amount = Languages.Where(x => x.LanguageText.ToLower() == item1.LanguageText.ToLower()).ToList().Count;

                if (amount >= 2)
                {
                    namesOK = false;
                    break;
                }
            }

            return namesOK;
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
