namespace SMSHandlerUI.Models
{
    public class PLCconnectionComboBoxModel
    {
        public int CPUmanufacturer { get; set; } //1 - S7, 2 - AB.
        public int Identity { get; set; } //From table with manufacturers
        public int PLCconnectionID { get; set; } //global
        public string ConnectionName { get; set; }
    }
}
