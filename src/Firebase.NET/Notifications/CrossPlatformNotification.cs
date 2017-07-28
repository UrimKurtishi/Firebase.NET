using Firebase.NET.Contracts;

namespace Firebase.NET.Notifications
{
    public class CrossPlatformNotification : INotification
    {
        public string Title { get; set; }

        public string Body { get; set; }

        public string Icon { get; set; }

        public string Badge { get; set; }

        public string Sound { get; set; }

        public string Tag { get; set; }

        public string Color { get; set; }

        public string ClickAction { get; set; }

        public string BodyLocKey { get; set; }

        public string BodyLocArgs { get; set; }

        public string TitleLocKey { get; set; }

        public string TitleLocArgs { get; set; }
    }
}
