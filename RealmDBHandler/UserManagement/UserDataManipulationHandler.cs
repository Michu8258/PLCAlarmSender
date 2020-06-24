using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.UserManagement
{
    public class UserDataManipulationHandler
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public UserDataManipulationHandler(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"User data manipulation handler object created.");
        }

        #endregion

        #region Adding new and updating existing user methods

        public bool AddNewUser(string userName, string password, AccessLevelEnum accessLevel,
            bool logOff, int logOffTimeout, int langPrevillages)
        {
            _logger.Info($"Method for adding new user to DB fired. User name = {userName}, Access level = {accessLevel.ToString()}.");

            try
            {
                (string hashPass, byte[] salt) = EncryptPassword(password);

                _realm.Write(() =>
                {
                    _realm.Add(new UserDefinition()
                    {
                        Identity = GetNewID(),
                        UserName = userName,
                        Password = hashPass,
                        AccessLevel = GetAccessLevelInt(accessLevel),
                        Salt = salt,
                        LogoutEnabled = logOff,
                        LogoutTime = logOffTimeout,
                        LanguageEditorPrevilages = langPrevillages,
                    });
                });

                _logger.Info($"Adding new user to DB successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add new user to DB: {ex.Message}.");
                return false;
            }
        }

        public bool ModifyUser(int userID, AccessLevelEnum newAccessLevel, bool logOff,
            int logOffTimeout, int newPrevilages)
        {
            _logger.Info($"Method for modifying existing user in Db fired. User ID = {userID}, Access level = {newAccessLevel.ToString()}.");

            try
            {
                UserDefinition user = _realm.All<UserDefinition>().ToList().Single(x => x.Identity == userID);

                using (var trans = _realm.BeginWrite())
                {
                    user.AccessLevel = GetAccessLevelInt(newAccessLevel);
                    user.LogoutEnabled = logOff;
                    user.LogoutTime = logOffTimeout;
                    user.LanguageEditorPrevilages = newPrevilages;
                    trans.Commit();
                }

                _logger.Info($"Modifying exisng user data in DB successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to modify existing user data: {ex.Message}.");
                return false;
            }
        }

        #endregion

        #region Amount of users

        public int CheckHowManyUsersSavedInDB()
        {
            _logger.Info($"Method for checking how many users are saved in DB fired.");

            try
            {
                return _realm.All<UserDefinition>().Count();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to check amount of users stored in DB. Exception: {ex.Message}.");
                return -10;
            }
        }

        #endregion

        #region Internal helper methods

        private int GetNewID()
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(new UserDefinition());
        }

        private int GetAccessLevelInt(AccessLevelEnum level)
        {
            AccesLevelConverter converter = new AccesLevelConverter();
            return converter.GetAccessLevelInt(level);
        }

        private (string, byte[]) EncryptPassword(string password)
        {
            string hashedPassword;
            byte[] salt;

            PasswordHanling.PasswordEncryptor encryptor = new PasswordHanling.PasswordEncryptor();
            (hashedPassword, salt) = encryptor.EnctyptPassword(password);

            return (hashedPassword, salt);
        }

        #endregion

        #region Deleting user from DB

        public bool DeleteUser(int userID)
        {
            _logger.Info($"Method for deletion of user from DB fired. User ID = {userID}.");

            try
            {
                UserDefinition user = _realm.All<UserDefinition>().ToList().Single(x => x.Identity == userID);
                using (var trans = _realm.BeginWrite())
                {
                    _realm.Remove(user);
                    trans.Commit();
                }

                _logger.Info($"Deleting user from DB successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to delete existing user data: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
