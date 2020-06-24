using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.RealmObjects;
using Realms;
using System;

namespace RealmDBHandler.PLCconnectionsHandling
{
    public class PLCconnectionCreator
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public PLCconnectionCreator(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"PLC connection creator object created.");
        }

        #endregion

        #region Adding new S7 PLC connection

        public bool AddNewS7Connection(string connectionName, int oct1, int oct2,
            int oct3, int oct4, int rack, int slot, S7CpuTypeEnum cpuType, bool connectionActivated)
        {
            _logger.Info($"Method for creating new S7 connection fired.");

            return AddNewS7ConnectionDefinitionToDB(connectionName, oct1, oct2, oct3, oct4,
                rack, slot, cpuType, connectionActivated);
        }

        #endregion

        #region Internal methods

        private int GetNewIdentity(IIdentityPrimaryKeyInterface implementingObject)
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(implementingObject);
        }

        private int GetConnectionIDNewIdentity()
        {
            NewConnectionIDProvider provider = new NewConnectionIDProvider(_realmProvider);
            return provider.GetNewPlcConnectionID();
        }

        private bool AddNewS7ConnectionDefinitionToDB(string connectionName, int oct1, int oct2,
            int oct3, int oct4, int rack, int slot, S7CpuTypeEnum cpuType, bool connectionActivated)
        {
            _logger.Info($"Method for writing new S7 PLC connection to DB fired. Connection name: {connectionName}.");

            try
            {
                _realm.Write(() =>
                {
                    _realm.Add(new S7PlcConnectionDefinition()
                    {
                        Identity = GetNewIdentity(new S7PlcConnectionDefinition()),
                        PLCconnectionID = GetConnectionIDNewIdentity(),
                        ConnectionName = connectionName,
                        FirstOctet = oct1,
                        SecondOctet = oct2,
                        ThirdOctet = oct3,
                        FourthOctet = oct4,
                        Rack = rack,
                        Slot = slot,
                        CPUtype = ConvertS7CpuTypeToInt(cpuType),
                        ConnectionActivated = connectionActivated,
                    });
                });

                _logger.Info($"Adding new S7 PLC connection to DB successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add new S7 connection definition to BD: {ex.Message}.");
                return false;
            }
        }

        private int ConvertS7CpuTypeToInt(S7CpuTypeEnum type)
        {
            S7CpuTypeConverter converter = new S7CpuTypeConverter();
            return converter.GetS7CpuTypeInt(type);
        }

        #endregion
    }
}
