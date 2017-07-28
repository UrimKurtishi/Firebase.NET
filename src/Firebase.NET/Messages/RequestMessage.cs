using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Firebase.NET.Contracts;
using Firebase.NET.Infrastructure;
using Firebase.NET.Serialization;

namespace Firebase.NET.Messages
{
    /// <summary>
    /// This exception is thrown when an attempt is made to set both To and RegistrationIds.
    /// Either set 'To' or 'RegistrationIds' property of RequestMessageBody.
    /// It's either a single client app receiver or multiple receivers (bulk list).
    /// </summary>
    public class ReceiverTypeAlreadySetExcepetion : Exception
    {
        public ReceiverTypeAlreadySetExcepetion(string message) : base(message) { }
    }

    public class RequestMessageHeader
    {
        private readonly ApplicationSettings Settings;

        public RequestMessageHeader(string contentType = "application/json")
        {
            Settings = new ApplicationSettings();
            string authorization = "key=" + Settings.FirebaseServerKey;

            this.Authorization = authorization;
            this.ContentType = contentType;
        }

        /// <summary>
        /// key=YOUR_SERVER_KEY 
        /// Make sure this is the server key, whose value is available in the Cloud Messaging tab of the Firebase console Settings pane.
        /// Android, iOS, and browser keys are rejected by Google Firebase.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// application/json for JSON; application/x-www-form-urlencoded;charset=UTF-8 for plain text.
        /// If Content-Type is omitted, the format is assumed to be plain text.
        /// </summary>
        public string ContentType { get; set; }

        public RequestMessageHeader DeepCopy()
        {
            RequestMessageHeader newRequestMessageHeader = new RequestMessageHeader();
            foreach (PropertyInfo propInfo in newRequestMessageHeader.GetType().GetProperties())
            {
                if (propInfo.CanWrite)
                    propInfo.SetValue(newRequestMessageHeader, propInfo.GetValue(this));
            }

            return newRequestMessageHeader;
        }
    }

    public class RequestMessageBody
    {
        /// <summary>
        /// Duration of time in seconds for the request message to live (max is 2419200 seconds = 4 weeks).
        /// </summary>
        public const int MaxTimeToLive = 2419200;
        public const int FIREBASE_PAYLOAD_MAX_SIZE = 4096;
        public const int FIREBASE_PAYLOAD_MAX_SIZE_IOS = 2048;

        private string to;
        private string[] registrationIds;
        private int timeToLive;

        public RequestMessageBody()
        {
            this.TimeToLive = MaxTimeToLive;
            this.Priority = MessagePriority.High.ToString().ToLower();
            To = null;
            RegistrationIds = null;
        }

        public RequestMessageBody(string to = null, string[] registrationIds = null, string condition = null,
                                       string collapseKey = null, MessagePriority priority = MessagePriority.High, int ttl = MaxTimeToLive,
                                       string restrictedPackageName = null, bool dryRun = false,
                                       IPayload data = null, INotification notification = null)
        {
            if (to != null && registrationIds != null)
                throw new ReceiverTypeAlreadySetExcepetion("Both 'to' and 'registrationIds' are set. You can set only one of them. It's either a single or bulk list.");
            To = to;
            RegistrationIds = registrationIds;
            Condition = condition;
            CollapseKey = collapseKey;
            Priority = priority.ToString().ToLower();
            TimeToLive = ttl;
            RestrictedPackageName = restrictedPackageName;
            DryRun = dryRun;
            Data = data;
            Notification = notification;
        }

        /// <summary>
        /// Request message's payload max size in bytes = 4096 bytes for most messages.
        /// Or 2048 bytes in the case of messages to topics or notification messages on iOS.
        /// </summary>
        public int FirebasePayloadMaxSize
        {
            get { return FIREBASE_PAYLOAD_MAX_SIZE; }
        }

        public bool ShouldSerializeFirebasePayloadMaxSize()
        {
            return false;
        }

        /// <summary>
        /// This parameter specifies the recipient of a message.
        /// The value can be a device's registration token, a device group's notification key, or a single topic (prefixed with /topics/). 
        /// To send to multiple topics, use the condition parameter.
        /// </summary>
        public string To
        {
            get { return to; }
            set
            {
                if (RegistrationIds == null || value == null)
                    to = value;
                else
                    throw new ReceiverTypeAlreadySetExcepetion("'RegistrationIds' has been set. Both can't be set, it's either a single or bulk push");
            }
        }

        /// <summary>
        /// This parameter specifies the recipient of a multicast message, a message sent to more than one registration token.
        /// The value should be an array of registration tokens to which to send the multicast message. 
        /// The array must contain at least 1 and at most 1000 registration tokens. To send a message to a single device, use the to parameter.
        /// Multicast messages are only allowed using the HTTP JSON format.
        /// </summary>
        public string[] RegistrationIds
        {
            get { return registrationIds; }
            set
            {
                if (To == null || value == null)
                    registrationIds = value;
                else
                    throw new ReceiverTypeAlreadySetExcepetion("'To' has been set. Both can't be set, it's either a single or bulk push");
            }
        }

        /// <summary>
        /// This parameter specifies a logical expression of conditions that determine the message target.
        /// Supported condition: Topic, formatted as "'yourTopic' in topics". This value is case-insensitive.
        /// Supported operators: &&, ||. Maximum two operators per topic message supported.
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// This parameter identifies a group of messages (e.g., with collapse_key: "Updates Available") that can be collapsed, so that only the
        /// last message gets sent when delivery can be resumed. This is intended to avoid sending too many of the same messages when the device
        /// comes back online or becomes active.
        /// </summary>
        public string CollapseKey { get; set; }


        /// <summary>
        /// Sets the priority of the message. Valid values are "normal" and "high." On iOS, these correspond to APNs priorities 5 and 10.
        /// By default, notification messages are sent with high priority, and data messages are sent with normal priority.
        /// Normal priority optimizes the client app's battery consumption and should be used unless immediate delivery is required. 
        /// For messages with normal priority, the app may receive the message with unspecified delay. 
        /// When a message is sent with high priority, it is sent immediately, and the app can wake a sleeping device and open a network 
        /// connection to your server.
        /// </summary>
        public string Priority { get; private set; }


        /// <summary>
        /// How long (in seconds) the message should be kept on Google Firebase storage if the device is offline
        /// Default: 4 weeks
        /// Min: 0 -> MAX: 2,419,200 (4weeks)
        /// </summary>
        public int TimeToLive
        {
            get { return timeToLive; }
            set
            {
                if (value < 0)
                    timeToLive = 0;
                else if (value > MaxTimeToLive)
                    timeToLive = MaxTimeToLive;
                else
                    timeToLive = value;
            }
        }

        /// <summary>
        /// This parameter specifies the package name of the application where 
        /// the registration tokens must match in order to receive the message.
        /// </summary>
        public string RestrictedPackageName { get; set; }

        /// <summary>
        /// If included, allows developers to test their request without 
        /// actually sending a message to the client app.
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// This parameter specifies the custom key-value pairs of the message's payload.
        /// </summary>
        public IPayload Data { get; set; }

        /// <summary>
        /// This parameter specifies the predefined, user-visible key-value pairs  of the message's notifictation.
        /// </summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        public INotification Notification { get; set; }

        public RequestMessageBody DeepCopy()
        {
            RequestMessageBody newRequestMessageBody = new RequestMessageBody();
            foreach (PropertyInfo propInfo in newRequestMessageBody.GetType().GetProperties())
            {
                if (propInfo.CanWrite)
                    propInfo.SetValue(newRequestMessageBody, propInfo.GetValue(this));
            }

            var notificationTypeInstance = (INotification)Activator.CreateInstance(this.Notification.GetType());
            foreach (PropertyInfo propInfo in notificationTypeInstance.GetType().GetProperties())
            {
                if (propInfo.CanWrite)
                    propInfo.SetValue(notificationTypeInstance, propInfo.GetValue(this.Notification));
            }
            newRequestMessageBody.Notification = notificationTypeInstance;

            Dictionary<string, string> payLoad = (Dictionary<string, string>)this.Data;
            Payload newPayload = new Payload();
            foreach (var key in payLoad.Keys)
            {
                newPayload.Add(key, payLoad[key]);
            }
            newRequestMessageBody.Data = newPayload;

            return newRequestMessageBody;
        }
    }

    public class RequestMessage : IMessage
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public RequestMessageHeader Header { get; set; }

        public RequestMessageBody Body { get; set; }

        /// <summary>
        /// The protocol used with Google Firebase connection servers is HTTP
        /// </summary>
        public FirebaseProtocol ConnectionProtocol { get; set; }

        /// <summary>
        /// Is this request a bulk push, more than one receiver of notification
        /// </summary>
        public bool IsBulk
        {
            get
            {
                if (Body.RegistrationIds != null && Body.To == null)
                    return true;
                else
                    return false;
            }
        }

        public string BodyToJSON
        {
            get { return JsonConvert.SerializeObject(Body, Formatting.Indented, _jsonSerializerSettings); }
        }

        /// <summary>
        /// Convert messages payload 'Data' to json for measuring size in bytes 
        /// and other various reasons, if needed in the future
        /// </summary>
        public string DataToJson
        {
            get { return JsonConvert.SerializeObject(Body.Data, _jsonSerializerSettings); }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RequestMessage()
        {
            ConnectionProtocol = FirebaseProtocol.HTTP;
            Header = new RequestMessageHeader("application/json");
            Body = new RequestMessageBody();

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new PropertyNameResolver()
            };
        }

        /// <summary>
        /// All parameters are optional, every parameter will be initialized: 
        /// Header will be ready and only Body needs to be filled with neccessary data
        /// </summary>
        /// <param name="body"></param>
        /// <param name="content_type">Default is 'application/json'</param>
        /// <param name="protocol">Default is HTTP</param>
        public RequestMessage(RequestMessageBody body = null, string content_type = "application/json",
                                   FirebaseProtocol protocol = FirebaseProtocol.HTTP)
        {
            ConnectionProtocol = protocol;
            Header = new RequestMessageHeader(content_type);
            Body = (body == null ? new RequestMessageBody() : body);

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new PropertyNameResolver(),
            };
        }

        public RequestMessage(RequestMessageHeader header, RequestMessageBody body,
                                   FirebaseProtocol protocol = FirebaseProtocol.HTTP)
        {
            ConnectionProtocol = protocol;
            Header = header;
            Body = body;

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new PropertyNameResolver()
            };
        }

        public ResponseMessageError? ValidateRequest()
        {
            if (Body.To == null && Body.RegistrationIds == null)
                return ResponseMessageError.MissingRegistration;
            else if (Header.Authorization == null)
                return ResponseMessageError.Unauthorized;
            else if (DataToJson.Length * sizeof(byte) > Body.FirebasePayloadMaxSize) //needs more analysis
                return ResponseMessageError.MessageTooBig;

            return null;
        }

        public IMessage DeepCopy()
        {
            RequestMessage newRequestMessage = new RequestMessage();
            newRequestMessage.ConnectionProtocol = this.ConnectionProtocol;
            newRequestMessage.Header = this.Header.DeepCopy();
            newRequestMessage.Body = this.Body.DeepCopy();

            return newRequestMessage;
        }
    }
}
