namespace RealmDBHandler.EnumsAndConverters
{
    public enum SystemEventTypeEnum
    {
        None,
        ConnectionTest,
        S7PLCconnectionFailure,
        S7AlarmOccured,
        S7AlarmAcknowledged,
        SMSsending,
        SMSSendingFailed,
    }
}
