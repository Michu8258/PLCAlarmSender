using RealmDBHandler.EnumsAndConverters;

namespace SMSHandlerUI.Models
{
    public class S7ConnectionModel
    {
        public int Identity { get; set; }
        public string ConnectionName { get; set; }
        public int ConnectionID { get; set; }
        public string IPaddress { get; set; }
        public int Rack { get; set; }
        public int Slot { get; set; }
        public S7CpuTypeEnum CpuType { get; set; }
        public string CpuTypeString { get; set; }
        public bool ConnectionActivated { get; set; }
    }
}
