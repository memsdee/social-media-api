namespace be.Application.Common.Constants;

public readonly struct SignalRMethods
{
    public static class Notification
    {
        public const string New = "NewNotification";
    }

    public static class Post
    {
        public const string Follow = "Follow";
        public const string Comment = "Comment";
        public const string React = "React";
    }

    public static class Conversation
    {
        public const string New = "NewMessage";
        public const string Noti = "NewNoti";
    }
}