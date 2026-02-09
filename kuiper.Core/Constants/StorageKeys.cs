namespace kuiper.Core.Constants
{
    public static class StorageKeys
    {
        public const string AuthorizedCommandSlots = "#authorized_command_slots";
        public const string HintPointsPrefix = "#hintpoints:slot:";
        public const string ChecksPrefix = "#checks:slot:";
        public const string ReceivedItemsPrefix = "#received:slot:";
        public const string PasswordPrefix = "#password:slot:";
        public const string SetNotifyPrefix = "#setnotify:";
        public const string ConnectionTagsPrefix = "#connection_tags:";
        public const string HintPrefix = "#hints:slot:";

        public static string HintPoints(long slotId) => $"{HintPointsPrefix}{slotId}";
        public static string Checks(long slotId) => $"{ChecksPrefix}{slotId}";
        public static string ReceivedItems(long slotId) => $"{ReceivedItemsPrefix}{slotId}";
        public static string Password(long slotId) => $"{PasswordPrefix}{slotId}";
        public static string SetNotify(string connectionId) => $"{SetNotifyPrefix}{connectionId}";
        public static string ConnectionTags(string connectionId) => $"{ConnectionTagsPrefix}{connectionId}";
        public static string Hint(long slotId) => $"{HintPrefix}{slotId}";
    }
}
