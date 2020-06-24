namespace SMSHandlerUI.Models
{
    public class S7AlarmEnabledTextsModel
    {
        public bool LanguageEnabled { get; set; }
        public string LanguageName { get; set; }
        public string AlarmText { get; set; }
        public int AlarmTextIdex { get; set; } //[0...15]
    }
}
