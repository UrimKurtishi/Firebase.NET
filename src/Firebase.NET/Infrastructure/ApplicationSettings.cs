using System.Configuration;

namespace Firebase.NET.Infrastructure
{
    /// <summary>
    /// ApplicationSettings provides Google Firebase's Ser
    /// </summary>
    public class ApplicationSettings
    {
        public string FirebaseServerKey { get; set; }

        public string FirebaseSenderID { get; set; }

        public string FirebaseConnectionEndpoint { get; set; }

        public ApplicationSettings(string serverKey = "FirebaseServerKey",
                                   string senderId = "FirebaseSenderID",
                                   string connEndpoint = "FirebaseConnectionEndpoint")
        {
            FirebaseServerKey = ConfigurationManager.AppSettings[serverKey];
            FirebaseSenderID = ConfigurationManager.AppSettings[senderId];
            FirebaseConnectionEndpoint = ConfigurationManager.AppSettings[connEndpoint];
        }
    }
}
