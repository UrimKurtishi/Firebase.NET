using System;
using System.Net;
using Firebase.NET.Contracts;

namespace Firebase.NET.Messages
{
    public class ResponseMessageErrorException : Exception
    {
        public ResponseMessageErrorException(string message) : base(message) { }
    }

    public class ResponseMessageHeader
    {
        /// <summary>
        /// It can returned success or error: 200, 400, 401, 5xx    
        /// </summary>
        public HttpStatusCode ResponseStatusCode { get; set; }

        public string ReasonPhrase { get; set; }

        public int RetryAfter { get; set; }

        /// <summary>
        /// Retry-After http header might return date or integer of seconds
        /// https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html (section 14.37) http/1.1
        /// </summary>
        public int? RetryAfterInMilliSeconds
        {
            get
            {
                if (RetryAfter > 0)
                    return RetryAfter * 1000;
                else
                    return null;
            }
        }
    }

    public class ResponseMessageBody
    {
        /// <summary>
        /// Unique ID (number) identifying the multicast message
        /// </summary>
        public long MulticastId { get; set; }

        /// <summary>
        /// Number of messages that were processed without an error
        /// </summary>
        public int Success { get; set; }

        /// <summary>
        /// Number of messages that could not be processed
        /// </summary>
        public int Failure { get; set; }

        /// <summary>
        ///  Number of results that contain a canonical registration token. A canonical registration ID is the registration token of the last registration 
        /// requested by the client app. 
        /// This is the ID that the server should use when sending messages to the device
        /// </summary>
        public int CanonicalIds { get; set; }

        /// <summary>
        /// Array of objects representing the status of the messages processed.
        /// The objects are listed in the same order as the request(aka RegistrationIDs) 
        /// </summary>
        public ResponseMessageResult[] Results { get; set; }
    }

    public class ResponseMessage : IMessage
    {
        public ResponseMessageHeader Header { get; set; }

        public ResponseMessageBody Body { get; set; }

        public ResponseMessage()
        {
            Header = new ResponseMessageHeader();
            Body = new ResponseMessageBody();
        }

        IMessage IMessage.DeepCopy()
        {
            //TO BE IMPLEMENTED FOR FUTURE CASES
            throw new NotImplementedException();
        }
    }
}
