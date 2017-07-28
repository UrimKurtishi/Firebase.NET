namespace Firebase.NET.Contracts
{
    /// <summary>
    /// Request message's notification specifies the predefined, user-visible key-value pairs 
    /// </summary>
    public interface INotification
    {
        string Title { get; set; }

        string Body { get; set; }

        string ClickAction { get; set; }
    }
}
