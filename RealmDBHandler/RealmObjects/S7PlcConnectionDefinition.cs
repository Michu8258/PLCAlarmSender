using RealmDBHandler.CommonClasses;
using Realms;

namespace RealmDBHandler.RealmObjects
{
    public class S7PlcConnectionDefinition : RealmObject, IIdentityPrimaryKeyInterface, IPlcConnectionIdentity
    {
        [PrimaryKey]
        public int Identity { get; set; }
        public int PLCconnectionID { get; set; }
        public string ConnectionName { get; set; }
        public int FirstOctet { get; set; }
        public int SecondOctet { get; set; }
        public int ThirdOctet { get; set; }
        public int FourthOctet { get; set; }
        public int Rack { get; set; }
        public int Slot { get; set; }
        public int CPUtype { get; set; }
        public bool ConnectionActivated { get; set; }
    }
}
