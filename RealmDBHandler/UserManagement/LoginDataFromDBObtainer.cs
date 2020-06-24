using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.UserManagement
{
    internal class LoginDataFromDBObtainer
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public LoginDataFromDBObtainer(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Login data from DB obtainer object created.");
        }

        #endregion

        #region Method for checking if user name exists in DB

        public (bool, string, byte[]) GetUserLoginDataFromDB(string userName)
        {
            _logger.Info($"Method for reading user data from Db fired. User name = {userName}.");

            //check if such user name exists in db only once
            bool userExists = CheckIfSuchUserNameExists(userName);

            //if it's true, get hashed password and salt from DB
            if (userExists)
            {
                (string hashedPassword, byte[] salt) = GetHashedPasswordAndSaltFromDB(userName);
                return (true, hashedPassword, salt);
            }
            else return (false, null, null);
        }

        #endregion

        #region Private method for DB interaction

        private bool CheckIfSuchUserNameExists(string userName)
        {
            _logger.Info($"Method for checking in user name '{userName}' exists in DB.");

            bool output = false;

            if (!String.IsNullOrWhiteSpace(userName))
            {
                try
                {
                    string name = _realm.All<UserDefinition>().ToList().Single(x => x.UserName == userName).UserName;
                    _logger.Info($"User name '{userName}' exists in DB.");
                    output = true;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while checking if user name exists in DB: {ex.Message}.");
                }
            }

            return output;
        }

        private (string, byte[]) GetHashedPasswordAndSaltFromDB(string userName)
        {
            _logger.Info($"Method for obtaining hashed password from DB fired. User name = {userName}.");

            string hashedPassword = null;
            byte[] salt = null;

            if (!String.IsNullOrWhiteSpace(userName))
            {
                try
                {
                    hashedPassword = _realm.All<UserDefinition>().ToList().Single(x => x.UserName == userName).Password;
                    salt = _realm.All<UserDefinition>().ToList().Single(x => x.UserName == userName).Salt;

                    _logger.Info($"Reading hashed password from DB successfull. User name = {userName}.");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while obtaining user hashed password and salt: {ex.Message}.");
                }
            }

            return (hashedPassword, salt);
        }

        #endregion
    }
}
