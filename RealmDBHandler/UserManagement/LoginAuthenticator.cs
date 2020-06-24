using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.PasswordHanling;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.UserManagement
{
    public class LoginAuthenticator
    {
        #region Fields and constructor

        private readonly string _userName;
        private readonly string _password;
        private bool _loginSuccessful;
        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public LoginAuthenticator(string userName, string password, IRealmProvider realmPRovider)
        {
            _password = password;
            _userName = userName;
            _loginSuccessful = false;

            _realmProvider = realmPRovider;
            _realm = _realmProvider.GetRealmDBInstance();

            //NLOG
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Login authenticator object created.");
        }

        #endregion

        #region Authentication method (algorithm)

        public AuthenticationResultEnum Authenticate()
        {
            _logger.Info($"Method for authenticating user fired. User name = {_userName}.");

            bool userNameExists;
            string hashedPassword;
            byte[] salt;

            //obtain login data from DB
            LoginDataFromDBObtainer obtainer = new LoginDataFromDBObtainer(_realmProvider);
            (userNameExists, hashedPassword, salt) = obtainer.GetUserLoginDataFromDB(_userName);

            //if there is no user wuth equal user name
            if (!userNameExists) return AuthenticationResultEnum.UsedDoesNotExist;

            //username exiss, but reading hashed password and salt failed
            if (hashedPassword == null || salt == null) return AuthenticationResultEnum.InternalError;
            else
            {
                string decryptedHashedPassword = null;

                //decrypting password
                PasswordEncryptor decryptor = new PasswordEncryptor();
                try
                {
                    decryptedHashedPassword = decryptor.DecryptPassword(hashedPassword, _password, salt);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while decrypting password (not matching): {ex.Message}.");
                }

                //if password is correct
                if (decryptedHashedPassword == _password)
                {
                    _loginSuccessful = true;
                    return AuthenticationResultEnum.LoginSuccessfull;
                }
                //if password is incorrect
                else return AuthenticationResultEnum.WrongPassword;
            }
        }

        #endregion

        #region If logged successfully, can read user data

        public LoggedUserData GetUserData()
        {
            _logger.Info($"Method for getin user data in case of successfull login fored.");

            if (_loginSuccessful)
            {
                try
                {
                    //read all data from DB
                    UserDefinition userData = _realm.All<UserDefinition>().Single(x => x.UserName == _userName);

                    //convert access level from number to enum
                    AccesLevelConverter converter = new AccesLevelConverter();
                    AccessLevelEnum accessLevel = converter.GetAccesLevelEnum(userData.AccessLevel);

                    //pass to caller only necessary data
                    return new LoggedUserData()
                    {
                        Identity = userData.Identity,
                        UserName = userData.UserName,
                        AccessLevel = accessLevel.ToString(),
                        AccessLevelEnum = accessLevel,
                        LogoutEnabled = userData.LogoutEnabled,
                        LogoutTime = userData.LogoutTime,
                        LangEditPrevilages = userData.LanguageEditorPrevilages,
                    };
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while obtaining user data from DB: {ex.Message}.");
                    return null;
                }

            }
            else return null;
        }

        #endregion
    }
}
