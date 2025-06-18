namespace Redis.OM.Playground.Api.GraphQL;

internal static class Constants
{
    public static class Channels
    {
        public const string Raw = "update:raw";
        public const string OneMinute = "update:1m";
        public const string FiveMinutes = "update:5m";
        public const string FifteenMinutes = "update:15m";
        public const string OneHour = "update:1h";
    }
}
