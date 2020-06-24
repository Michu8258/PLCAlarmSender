namespace RealmDBHandler.EnumsAndConverters
{
    public class S7CpuTypeConverter
    {
        public int GetS7CpuTypeInt(S7CpuTypeEnum type)
        {
            switch (type)
            {
                case S7CpuTypeEnum.S7300: return 10;
                case S7CpuTypeEnum.S7400: return 20;
                case S7CpuTypeEnum.S71200: return 30;
                case S7CpuTypeEnum.S71500: return 40;
                default: return 0;
            }
        }

        public S7CpuTypeEnum GetS7TypeEnum(int type)
        {
            if (type <= 10) return S7CpuTypeEnum.S7300;
            else if (type <= 20) return S7CpuTypeEnum.S7400;
            else if (type <= 30) return S7CpuTypeEnum.S71200;
            else if (type <= 40) return S7CpuTypeEnum.S71500;
            else return S7CpuTypeEnum.S7300;
        }
    }
}
