namespace Redis.OM.Playground.Api.Messaging;

public static class MessagingConstants
{
    public const string UserCreatedKey = "user:created";
    public const string UserCreated1MinKey = "user:created:1m";
    public const string UserCreated5MinKey = "user:created:5m";
    public const string UserCreated15MinKey = "user:created:15m";
    public const string UserCreated1HourKey = "user:created:1h";

    public const string UserUpdatedKey = "user:updated";
    public const string UserUpdated1MinKey = "user:updated:1m";
    public const string UserUpdated5MinKey = "user:updated:5m";
    public const string UserUpdated15MinKey = "user:updated:15m";
    public const string UserUpdated1HourKey = "user:updated:1h";
}
