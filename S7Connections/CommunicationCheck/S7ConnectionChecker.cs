using Sharp7;
using System;

namespace S7Connections.CommunicationCheck
{
    public class S7ConnectionChecker
    {
        #region Fields and properties

        //instance of connection object
        private readonly S7Client PLC;

        //connection result
        private int _connectionResult;

        #endregion

        #region Constructor

        public S7ConnectionChecker()
        {
            //initialize S7 client object
            PLC = new S7Client();
        }

        #endregion

        #region Public methods

        //checking if connection is correct
        //if not, return string error

        /// <summary>
        /// If connection id ok, then this method returns true, and empty string. In case of error, it returns false, and error message string
        /// </summary>
        /// <param name="ipAddress">pass here string that represents the IP address of S7 PLC</param>
        /// <param name="rack">pass here the S7 PLC rack number</param>
        /// <param name="slot">pass here the S7 PLC slot number</param>
        /// <returns></returns>
        public (bool, string) CheckConnection(string ipAddress, short rack, short slot)
        {
            _connectionResult = PLC.ConnectTo(ipAddress, rack, slot);

            if (_connectionResult == 0)
            {
                return (true, "");
            }
            return (false, PLC.ErrorText(_connectionResult));
        }

        //if connection is ok, check CPU info
        public string GetCPUInfo()
        {
            if (_connectionResult == 0)
            {
                S7Client.S7CpuInfo info = new S7Client.S7CpuInfo();
                int response = PLC.GetCpuInfo(ref info);

                if (response == 0)
                {
                    return $"Module Type Name\t: {info.ModuleTypeName.Replace("\0", "")}\nSerial Number\t\t: {info.SerialNumber.Replace("\0", "")}\nAS Name\t\t\t: {info.ASName.Replace("\0", "")}\nModule Name\t\t: {info.ModuleName.Replace("\0", "")}";
                }
            }
            return "";
        }

        //if cnnection is ok, check SPU date and time
        public string GetCPUDateAndTime()
        {
            if (_connectionResult == 0)
            {
                DateTime DT = new DateTime();
                int response = PLC.GetPlcDateTime(ref DT);
                if (response == 0)
                {
                    return $"{DT.ToLongDateString()} - {DT.ToLongTimeString()}";
                }
            }
            return "";
        }

        #endregion
    }
}
