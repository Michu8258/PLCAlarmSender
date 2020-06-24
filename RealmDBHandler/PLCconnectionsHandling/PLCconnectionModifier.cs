using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.PLCconnectionsHandling
{
    public class PLCconnectionModifier
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public PLCconnectionModifier(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"PLC connection modifier object created.");
        }

        #endregion

        #region Modifying S7 connection

        public bool ModifyS7Connection(int identity, int connectionID, int firstOctet, int secondOctet, int rhirdOctet,
            int fourthOctet, int rack, int slot, S7CpuTypeEnum cpuType, bool connectionActivated)
        {
            _logger.Info($"Method for modifying S7 connection fired. Identity = {identity}, connection ID = {connectionID}.");

            try
            {
                S7PlcConnectionDefinition definition = _realm.All<S7PlcConnectionDefinition>().
                    Where(x => x.Identity == identity && x.PLCconnectionID == connectionID).ToList().First();

                using (var trans = _realm.BeginWrite())
                {
                    definition.FirstOctet = firstOctet;
                    definition.SecondOctet = secondOctet;
                    definition.ThirdOctet = rhirdOctet;
                    definition.FourthOctet = fourthOctet;
                    definition.Rack = rack;
                    definition.Slot = slot;
                    definition.CPUtype = ConvertS7CpuTypeToInt(cpuType);
                    definition.ConnectionActivated = connectionActivated;
                    trans.Commit();
                }

                _logger.Info($"Modification of S7 PLC connection successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to modify S7 PLC connection: {ex.Message}.");
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
