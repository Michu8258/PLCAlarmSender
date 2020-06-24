using Realms;
using System;
using NLog;
using Realms.Exceptions;
using System.Windows.Forms;

namespace RealmDBHandler.CommonClasses
{
    public sealed class RealmDBLocator : IRealmProvider
    {
        private Realm _realm;

        #region Public methods for geting realm instance and checking if DB is accessible

        public bool CheckIfDBisAccessible()
        {
            return GetRealmDBInstance() != null;
        }

        public Realm GetRealmDBInstance()
        {
            EstablishDBConnection();
            return _realm;
        }

        #endregion

        #region Private method

        private void EstablishDBConnection()
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"Method for establishing location of Realm DB file fired.");

            try
            {
                _realm = Realm.GetInstance(string.Concat(Application.ExecutablePath.Remove(Application.ExecutablePath.Length - 16), @"RealmDB\SMSHandlerDB.realm"));
            }
            catch (RealmFileAccessErrorException ex)
            {
                //FilePath error

                logger.Error($"Correct realm DB file path does not exist. Exception {ex.Message}.");
                OnCouldNotCreateDB();
                _realm = null;
            }
            catch (Exception ex)
            {
                //file opened in another app

                logger.Error($"Realm DB file opened in another application: {ex.Message}.");
                OnRealmDBfileOpenedInAnotherApp();
                _realm = null;
            }
        }

        #endregion

        #region Events

        //event - there is an error : Realm DB file already opened in another app
        public delegate void RealmDbUncreatableEventHandler(object sender, EventArgs e);
        public event RealmDbUncreatableEventHandler CouldNotCreateDB;
        private void OnCouldNotCreateDB()
        {
            CouldNotCreateDB?.Invoke(typeof(RealmDBLocator), EventArgs.Empty);
        }

        //event - there is an error : Realm DB file already opened in another app
        public delegate void RealmDBReadingErrorEventHandler(object sender, EventArgs e);
        public event RealmDBReadingErrorEventHandler RealmDBfileAlreadyOpened;
        private void OnRealmDBfileOpenedInAnotherApp()
        {
            RealmDBfileAlreadyOpened?.Invoke(typeof(RealmDBLocator), EventArgs.Empty);
        }

        #endregion
    }
}
