using RealmDBHandler.EnumsAndConverters;
using NLog;

namespace SMSHandlerUI.Converters
{
    public static class S7TypeToStringConverter
    {
        public static string ConvertToString(S7CpuTypeEnum type)
        {
            switch (type)
            {
                case S7CpuTypeEnum.S7300: return "S7-300";
                case S7CpuTypeEnum.S7400: return "S7-400";
                case S7CpuTypeEnum.S71200: return "S7-1200";
                case S7CpuTypeEnum.S71500: return "S7-1500";
                default: return "Unknown";
            }
        }
    }
}
