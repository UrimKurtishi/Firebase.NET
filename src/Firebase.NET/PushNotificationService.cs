using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Firebase.NET.Infrastructure;
using Firebase.NET.Serialization;
using Firebase.NET.Contracts;
using Firebase.NET.Messages;

namespace Firebase.NET
{
    public class PushNotificationService : IPushService
    {
        public ApplicationSettings Settings;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private const string FirebaseEndpoint = "https://fcm.googleapis.com/fcm/send";
        public const int MAX_TIMEOUT_TIME = 32;

        private int max_retries;
        /// <summary>
        /// The max number of retries if push failes the first time. Not bigger than 5.
        /// </summary>
        public int MAX_RETRIES
        {
            get { return max_retries; }
            set {
                if (value >= 1 && value <= 5)
                    max_retries = value;
                else
                    max_retries = 5;
            }
        }

        /// <summary>
        /// Wait time between retries
        /// </summary>
        public int WAIT_BETWEEN_RETRIES = 10;

        public string FirebaseServerKey { get; set; }
        public string FirebaseSenderID { get; set; }
        public string FirebaseConnectionEndpoint { get; set; }

        internal static Func<string, string, bool> updateTokenFunc;
        internal static Func<string, bool> deleteTokenFunc;

        public PushNotificationService()
        {
            Settings = new ApplicationSettings();
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new PropertyNameResolver()
            };

            this.FirebaseSenderID = Settings.FirebaseSenderID;
            this.FirebaseServerKey = Settings.FirebaseServerKey;
            this.FirebaseConnectionEndpoint = Settings.FirebaseConnectionEndpoint;
        }

        public PushNotificationService(Func<string, string, bool> updateFunc = null, Func<string, bool> deleteFunc = null)
        {
            Settings = new ApplicationSettings();
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new PropertyNameResolver()
            };

            this.FirebaseSenderID = Settings.FirebaseSenderID;
            this.FirebaseServerKey = Settings.FirebaseServerKey;
            this.FirebaseConnectionEndpoint = Settings.FirebaseConnectionEndpoint;

            updateTokenFunc = updateFunc;
            deleteTokenFunc = deleteFunc;
        }
        
        public PushNotificationService(Func<string, string, bool> updateFunc = null, Func<string, bool> deleteFunc = null,
                                       ApplicationSettings settings = null)
        {
            Settings = (settings != null ? settings : new ApplicationSettings());
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new PropertyNameResolver()
            };

            this.FirebaseSenderID = Settings.FirebaseSenderID;
            this.FirebaseServerKey = Settings.FirebaseServerKey;
            this.FirebaseConnectionEndpoint = Settings.FirebaseConnectionEndpoint;

            updateTokenFunc = updateFunc;
            deleteTokenFunc = deleteFunc;
        }

        public PushNotificationService(string serverKey = null,
                                       Func<string, string, bool> updateFunc = null, Func<string, bool> deleteFunc = null)
        {
            Settings = new ApplicationSettings();
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new PropertyNameResolver()
            };

            this.FirebaseServerKey = (serverKey != null ? serverKey : Settings.FirebaseServerKey);
            this.FirebaseConnectionEndpoint = FirebaseEndpoint;

            updateTokenFunc = updateFunc;
            deleteTokenFunc = deleteFunc;
        }

        /// <summary>
        /// Pushes requestMessage to client apps provided either on To or RegistrationIds
        /// </summary>
        /// <returns>It returns a ResponseMessage.</returns>
        public async Task<IMessage> PushMessage(IMessage requestMessage)
        {
            var requestValidationStatus = ((RequestMessage)requestMessage).ValidateRequest();
            if (requestValidationStatus != null)
                throw new ResponseMessageErrorException(((ResponseMessageError)requestValidationStatus).ToString());

            ResponseMessage cloudResponse = await SendRequestMessage(requestMessage);
            await HandleResponse((RequestMessage)requestMessage, cloudResponse);

            return cloudResponse;
        }

        /// <summary>
        /// Send request message to Firebase connection server 
        /// </summary>
        /// <param name="requestMessage">This is RequestMessage parameter that contains neccessary information for sending request to Firebase.</param>
        /// <returns>Returns a ResponseMessage instance from the request ('requestMessage') send to Firebase.</returns>
        protected async Task<ResponseMessage> SendRequestMessage(IMessage requestMessage)
        {
            var httpClient = new HttpClient();
            var fcmRequestMessage = (RequestMessage)requestMessage;
            var jsonBody = fcmRequestMessage.BodyToJSON;
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, this.FirebaseConnectionEndpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            //httpRequest.Version = new Version("2.0");
            httpRequest.Headers.TryAddWithoutValidation("Authorization", "key=" + this.FirebaseServerKey);

            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);
            var responsePayload = await httpResponse.Content.ReadAsStringAsync();
            ResponseMessageBody cloudResponseBody = null;
            try
            {
                cloudResponseBody = JsonConvert.DeserializeObject<ResponseMessageBody>(responsePayload, _jsonSerializerSettings);
            }
            catch (Exception) { }

            ResponseMessage cloudResponse = new ResponseMessage();
            cloudResponse.Header.ResponseStatusCode = httpResponse.StatusCode;
            cloudResponse.Header.ReasonPhrase = httpResponse.ReasonPhrase;
            if (httpResponse.Headers.RetryAfter != null)
                cloudResponse.Header.RetryAfter = httpResponse.Headers.RetryAfter.Delta.Value.Seconds;
            cloudResponse.Body = cloudResponseBody;

            return cloudResponse;
        }

        protected async Task<IMessage> HandleResponse(RequestMessage requestMessage, ResponseMessage responseMessage)
        {
            if (responseMessage.Header.ResponseStatusCode == HttpStatusCode.OK &&
                responseMessage.Body.Failure == 0 && responseMessage.Body.CanonicalIds == 0)
                return null;

            //Body(FCM response content) can be null when Satus Code is Unauthorized (401) or BadRequest (400)
            //because FCM response content contains reason phrase as html
            //and as such is invalid json and Body is null. In this case, there is nothing to handle for unauthorized responses
            //and the only cause is the wrong server key or missing registration token or request containing invalid data not proper with FCM protocol. 
            //The API consumer must make sure the right server key is provided in their AppSettings or the request is valid
            if (responseMessage.Body == null || (responseMessage.Body.Failure == 0 && responseMessage.Body.CanonicalIds == 0))
                return null;

            List<string> tokensForBackOffPush = new List<string>();
            for (var i = 0; i < responseMessage.Body.Results.Length; i++)
            {
                var result = responseMessage.Body.Results[i];
                if (result.Error == null && result.RegistrationId == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(result.MessageId) && !string.IsNullOrWhiteSpace(result.RegistrationId))
                {
                    if (requestMessage.IsBulk)
                        updateTokenFunc?.Invoke(requestMessage.Body.RegistrationIds[i], result.RegistrationId);
                    else
                        updateTokenFunc?.Invoke(requestMessage.Body.To, result.RegistrationId);
                }
                else if (result.Error == ResponseMessageError.NotRegistered)
                {
                    if (requestMessage.IsBulk)
                        deleteTokenFunc?.Invoke(requestMessage.Body.RegistrationIds[i]);
                    else
                        deleteTokenFunc?.Invoke(requestMessage.Body.To);
                }
                else if (result.Error == ResponseMessageError.Unavailable ||
                         result.Error == ResponseMessageError.InternalServerError ||
                         result.Error == ResponseMessageError.DeviceMessageRateExceeded ||
                         result.Error == ResponseMessageError.TopicsMessageRateExceeded)
                {
                    if (requestMessage.IsBulk)
                        tokensForBackOffPush.Add(requestMessage.Body.RegistrationIds[i]);
                    else
                    {
                        tokensForBackOffPush.Add(requestMessage.Body.To);
                        break;
                    }
                }
                //else {} = check for more response errors and act accordingly
                //BadRequest or Unauthorized - there is nothing to handle here, check the status code and reason phrase from CloudResponseMessage object
                //but the API consumer must make sure, it provides correct and valid data
                //as of now all possible errors are checked through CloudRequestMessageBody property inforcements and 
                //CloudRequestMessage ValidateRequest() method
            }

            ResponseMessage responseMessageFromBackOffPush = new ResponseMessage();
            if (tokensForBackOffPush.Count > 0)
            {
                RequestMessage newRequestMessage = (RequestMessage)requestMessage.DeepCopy();
                if (tokensForBackOffPush.Count == 1)
                {
                    newRequestMessage.Body.RegistrationIds = null;
                    newRequestMessage.Body.To = tokensForBackOffPush[0];
                }
                else
                {
                    newRequestMessage.Body.To = null;
                    newRequestMessage.Body.RegistrationIds = tokensForBackOffPush.ToArray();
                }

                var status = await InitExponentialBackOffPush(newRequestMessage, responseMessage.Header.RetryAfterInMilliSeconds);
                if (status != null)
                    responseMessageFromBackOffPush = (ResponseMessage)status;
                else
                    responseMessageFromBackOffPush = null;
            }

            if (responseMessageFromBackOffPush != null)
                return responseMessageFromBackOffPush;
            else
                return responseMessage;
        }

        /// <summary>
        /// Initiates an exponential backoff push for failed pushes on these response errors:
        /// ResponseMessageError.Unavailable, 
        /// ResponseMessageError.InternalServerError, 
        /// ResponseMessageError.DeviceMessageRateExceeded, 
        /// ResponseMessageError.TopicsMessageRateExceeded.
        /// </summary>
        /// <returns>It returns a ResponseMessage</returns>
        public async Task<ResponseMessage> InitExponentialBackOffPush(IMessage requestMessage, int? retryAfterInMilliseconds)
        {
            RetryProvider retryProvider = new RetryProvider();
            Func<IMessage, Task<ResponseMessage>> sendMessage = new Func<IMessage, Task<ResponseMessage>>(SendRequestMessage);
            ResponseMessage responseMessage = await retryProvider.ExecFuncWithRetry(sendMessage, requestMessage,
                                                                                         MAX_RETRIES, WAIT_BETWEEN_RETRIES,
                                                                                         BackoffType.Exponential, retryAfterInMilliseconds);

            return responseMessage;
        }
    }
}
