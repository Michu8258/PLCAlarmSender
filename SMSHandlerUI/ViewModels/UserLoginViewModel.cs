using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.UserManagement;
using SMSHandlerUI.EventMessages;
using SMSHandlerUI.RuntimeData;

namespace SMSHandlerUI.ViewModels
{
    class UserLoginViewModel : Screen
    {
        #region Fields

        //user login view model data
        private string _userNameText;
        private string _passwordText;
        private string _loginErrorText;
        private LoggedUserData _userData;

        //realm provider
        private readonly IRealmProvider _reamProvider;

        //logger
        private readonly Logger _logger;

        //event aggregator
        private readonly IEventAggregator _eventAggregator;
        public UserLoginEventMessage Tosend = new UserLoginEventMessage();

        #endregion

        #region Properties

        public string LoginErrorText { get { return _loginErrorText; } set { _loginErrorText = value; NotifyOfPropertyChange(); } }
        public string UserNameText { get { return _userNameText; } set { _userNameText = value; NotifyOfPropertyChange(); } }
        public string PasswordText { get { return _passwordText; } set { _passwordText = value; NotifyOfPropertyChange(); } }
        public LoggedUserData LoggedUserData { get { return _userData; } set { _userData = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public UserLoginViewModel(IEventAggregator eventAggregator, IRealmProvider realmProvider)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _reamProvider = realmProvider;

            _eventAggregator = eventAggregator;
            _userNameText = "";
            _passwordText = "";
            _loginErrorText = "";
            _userData = null;

            _logger.Info($"User login window created.");
        }

        #endregion

        #region Ok and Cancel buttons

        public void ConfirmButton()
        {
            _logger.Info($"Button for confirming inputed data pressed.");

            if (UserNameText.Length > 0 && PasswordText.Length > 0)
            {
                _logger.Info($"User name lenght OK and password lengts OK");

                LoginAuthenticator authenticator = new LoginAuthenticator(UserNameText, PasswordText, _reamProvider);
                AuthenticationResultEnum result = authenticator.Authenticate();

                _logger.Info($"Authentication of inputed data rresult: {result.ToString()}.");

                GenerateLoginErrorString(result);

                if (result == AuthenticationResultEnum.LoginSuccessfull)
                {
                    _logger.Info($"Login successfull.");

                    GetLoggedUserData(authenticator);
                    TryClose();
                }
                else
                {
                    _logger.Info($"Login failed.");

                    UserNameText = "";
                    PasswordText = "";
                }
            }
        }

        public void CancelButton()
        {
            _logger.Info($"Button for closing login window pressed.");

            TryClose();
        }

        #endregion

        #region Private methods

        private void GetLoggedUserData(LoginAuthenticator authenticator)
        {
            _logger.Info($"Getting data of siccessfully loged user started.");

            _userData = authenticator.GetUserData();
            Tosend.UserData = LoggedUserData;
            _eventAggregator.BeginPublishOnUIThread(Tosend);
        }

        private void GenerateLoginErrorString(AuthenticationResultEnum authRes)
        {
            switch (authRes)
            {
                case AuthenticationResultEnum.UsedDoesNotExist: LoginErrorText = "This user does not exist"; break;
                case AuthenticationResultEnum.InternalError: LoginErrorText = "Internal login error"; break;
                case AuthenticationResultEnum.WrongPassword: LoginErrorText = "Password does not match"; break;
                case AuthenticationResultEnum.LoginSuccessfull: LoginErrorText = ""; break;
                default: LoginErrorText = "Unknown login error"; break;
            }
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
