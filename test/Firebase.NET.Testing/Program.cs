using System;
using Firebase.NET.Messages;
using Firebase.NET.Notifications;

namespace Firebase.NET.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            RequestMessage requestMessage = new RequestMessage();
            string[] ids = {
                //registration tokens here that are registered with your Firebase project
            };

            requestMessage.Body.RegistrationIds = ids;
            CrossPlatformNotification notification = new CrossPlatformNotification();
            notification.Title = "Important News";
            notification.Body = String.Format("This is a notification send from Firebase on {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
            requestMessage.Body.Notification = notification;

            Payload payload = new Payload();
            payload["leage"] = "UEFA";
            payload["game"] = "Albania vs Kosovo";
            payload["score"] = "1:1";
            requestMessage.Body.Data = payload;

            Func<string, string, bool> updateFunc = new Func<string, string, bool>(Update);
            Func<string, bool> deleteFunc = new Func<string, bool>(Delete);
            
            var pushService = new PushNotificationService(
                "firebaseServerKey",
                updateFunc, deleteFunc);
            pushService.MAX_RETRIES = 5;
            ResponseMessage responseMessage = (ResponseMessage)pushService.PushMessage(requestMessage).Result;

            Console.WriteLine(responseMessage.Header.ResponseStatusCode + " - " + responseMessage.Header.ReasonPhrase);
            if (responseMessage.Body != null)
            {
                for (int i = 0; i < responseMessage.Body.Results.Length; i++)
                {
                    if (responseMessage.Body.Results[i].RequestRetryStatus != PushMessageStatus.NULL)
                        Console.WriteLine(((PushMessageStatus)responseMessage.Body.Results[i].RequestRetryStatus).ToString());
                    string error = responseMessage.Body.Results[i].Error != null ? ((ResponseMessageError)responseMessage.Body.Results[i].Error).ToString() : "";
                    Console.WriteLine("Error: {0};  MessageId: {1};  RegistrationId: {2}", error, responseMessage.Body.Results[i].MessageId, responseMessage.Body.Results[i].RegistrationId);
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// This method updates the old registration token with new registration token
        /// Firebase.NET will call this method with appropriate tokens if Firebase servers return new token that should change the old one
        /// </summary>
        public static bool Update(string oldToken, string newToken) { return true; }

        /// <summary>
        /// This method deletes registration token that is invalid and you should not try to push notifications any longer to it.
        /// </summary>
        public static bool Delete(string oldToken) { return true; }
    }
}
