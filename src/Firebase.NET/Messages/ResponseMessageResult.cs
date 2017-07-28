using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firebase.NET.Messages
{
    public class ResponseMessageResult
    {
        public string MessageId { get; set; }

        /// <summary>
        /// If Firebase servers return data for this property, it means
        /// that the device has a new registration token and it should be 
        /// used instead of the old one.
        /// </summary>
        public string RegistrationId { get; set; }

        public ResponseMessageError? Error { get; set; }

        /// <summary>
        /// The OriginalRegistrationId is for internal checking and validation.
        /// In case the push failes for this device than its registration token is 
        /// assigned to OriginalRegistrationId for further debugging purposes.
        /// </summary>
        public string OriginalRegistrationId { get; set; }

        public PushMessageStatus RequestRetryStatus { get; set; }

        public ResponseMessageResult()
        {
            RequestRetryStatus = PushMessageStatus.NULL; //no retry attempt was made
        }
    }
}
