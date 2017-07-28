using Firebase.NET.Contracts;

namespace Firebase.NET.Notifications
{
    public class WebNotification : INotification
    {
        public string Title { get; set; }

        public string Body { get; set; }

        public string ClickAction { get; set; }

        public string Icon { get; set; }
    }
}
