using RealmDBHandler.AlarmLanguagesTexts;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.UserManagement;
using SMSHandlerUI.Models;
using System.Collections.Generic;

namespace SMSHandlerUI.RuntimeData
{
    public interface IRuntimeData
    {
        bool CanAlarmManagement { get; }
        bool CanAlarmProfileManager { get; }
        bool CanAlarmsLanguageEdition { get; }
        bool CanMessageReceiverGroupsManager { get; }
        bool CanMessageReceiversManager { get; }
        bool CanNLogParametrization { get; }
        bool CanPLCconnectionSetup { get; }
        bool CanSMSdeviceConnection { get; }
        bool CanUserAdministration { get; }
        bool CanUserLogout { get; }
        List<LanguageItemModel> CustomLanguageList { get; }
        LoggedUserData DataOfCurrentlyLoggedUser { get; }
        List<LanguageEditData> LanguageEditPermissions { get; }
        int NumberOfDefinedAlarms { get; }
        bool DataManipulationEnabled { get; }

        void RefreshRuntimeLanguagesList();
        void SetLoginPermissions(LoggedUserDataGUIModel model);
        void SetNumberOfDefinedAlarms(int amount);
        void SetSettingsMenuPrevilages(AccessLevelEnum accessLevel);
        void SetAlarmsMenuPrevilages(AccessLevelEnum accessLevel);
        void SetDatManipulationPrevilages(AccessLevelEnum accessLevel);
        void SetDataOfCurrentUser(LoggedUserData data);
        void SetLanguageEditData(List<LanguageEditData> langData);
    }
}