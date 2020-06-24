using NLog;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.UserManagement;
using SMSHandlerUI.Converters;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;

namespace SMSHandlerUI.LoggedUserDataHandling
{
    internal class LoggedUserDataHandler
    {
        #region Constructor and fields

        //logger
        private readonly Logger _logger;
        private IRuntimeData _runtimeData;

        public LoggedUserDataHandler(IRuntimeData runtimeData)
        {
            _runtimeData = runtimeData;
            _logger = LogManager.GetCurrentClassLogger();
        }

        #endregion

        #region Method for login and loging out handling

        public LoggedUserDataGUIModel LoginOrLogout(LoggedUserData data, int amountOfCurrentAlarms)
        {
            if (data == null)
            {
                _logger.Info($"User logout data asssignment procedure started.");

                InsertDataToCurrentlyLoggedUserDataLogout(amountOfCurrentAlarms);
                return UserLoggedOut();
            }
            else
            {
                _logger.Info($"User login data assignment procedure started.");

                InsertDataToCurrentlyLoggedUserDataLogin(data);
                return HandleNewLoggedUserData(data, amountOfCurrentAlarms);
            }
        }

        #endregion

        #region Currently logged user data manipulation

        private void InsertDataToCurrentlyLoggedUserDataLogout(int currentlyDefinedAlarmsAmount)
        {
            LoggedUserData data = new LoggedUserData()
            {
                Identity = 0,
                UserName = "-----",
                AccessLevel = "None",
                AccessLevelEnum = AccessLevelEnum.None,
                LogoutEnabled = false,
                LogoutTime = 10,
                LangEditPrevilages = currentlyDefinedAlarmsAmount,
            };

            _runtimeData.SetDataOfCurrentUser(data);
        }

        private void InsertDataToCurrentlyLoggedUserDataLogin(LoggedUserData data)
        {
            LoggedUserData newData = new LoggedUserData()
            {
                Identity = data.Identity,
                UserName = data.UserName,
                AccessLevel = data.AccessLevel,
                AccessLevelEnum = data.AccessLevelEnum,
                LogoutEnabled = data.LogoutEnabled,
                LogoutTime = data.LogoutTime,
                LangEditPrevilages = data.LangEditPrevilages,
            };

            _runtimeData.SetDataOfCurrentUser(newData);
        }

        #endregion

        #region Runtime data manipulation

        private LoggedUserDataGUIModel UserLoggedOut()
        {
            LoggedUserDataGUIModel uderModel = new LoggedUserDataGUIModel()
            {
                CanUserAdministration = false,
                CanUserLogout = false,
                LoggedUserName = "-----",
                UserPrevilages = "None",
                AmountOfCurrentAlarms = 0,
            };

            LanguageCodeConverter langConverter = new LanguageCodeConverter();
            _runtimeData.SetLanguageEditData(langConverter.GetLanguageCode(0));

            LoggedUserData logoutData = new LoggedUserData()
            {
                Identity = 0,
                UserName = "-----",
                AccessLevel = "None",
                AccessLevelEnum = AccessLevelEnum.None,
                LogoutEnabled = false,
                LogoutTime = 0,
                LangEditPrevilages = 0,
            };

            _runtimeData.SetDataOfCurrentUser(logoutData);

            _logger.Info($"Logged user changed to: {uderModel.LoggedUserName}, Previlages: {uderModel.UserPrevilages}.");

            return uderModel;
        }

        private LoggedUserDataGUIModel HandleNewLoggedUserData(LoggedUserData data, int amountOfCurrentAlarms)
        {
            LoggedUserDataGUIModel userModel = new LoggedUserDataGUIModel()
            {
                CanUserAdministration = data.AccessLevelEnum == AccessLevelEnum.Administrator,
                CanUserLogout = true,
                LoggedUserName = data.UserName,
                UserPrevilages = data.AccessLevel,
                AmountOfCurrentAlarms = amountOfCurrentAlarms,
            };

            LanguageCodeConverter langConverter = new LanguageCodeConverter();
            _runtimeData.SetLanguageEditData(langConverter.GetLanguageCode(0));

            _logger.Info($"Logged user changed to: {userModel.LoggedUserName}, Previlages: {userModel.UserPrevilages}.");

            return userModel;
        }

        #endregion
    }
}
