#define TRACK_FAILED_REQUESTS_ENABLED

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebase.NET.Contracts;
using Firebase.NET.Messages;

namespace Firebase.NET
{
    public enum BackoffType
    {
        Nonstop = 1, // constant retry
        Linear,      // linear retry, every 'x' seconds
        Exponential  // exponential retry, every retry wait time grows by its retry count
    }

    public enum RetryStatus
    {
        Success = 1,
        FailedWithPartialRetries,
        FailedWithMaxRetries
    }

    public class RetryProvider
    {
        public ResponseMessage ResponseMessage { get; private set; }

        /// <summary>
        /// This function provides retry mechanism for Firebase http request messages
        /// </summary>
        /// <param name="func">The provided function to execute</param>
        /// <param name="maxRetries">The maximum number of retries</param>
        /// <param name="waitBetweenRetrySec">The wait time in seconds between retries</param>
        /// <param name="retryType">The retry approach to execute the provided 'func' function</param>
        /// <param name="retryAfter">Start this method after 'retryAfter' which is in milliseconds</param>
        /// <returns></returns>
        public async Task<ResponseMessage> ExecFuncWithRetry(Func<IMessage, Task<ResponseMessage>> func,
                                                                  IMessage funcParamRequestMessage, int maxRetries, int waitBetweenRetrySec,
                                                                  BackoffType retryType, int? retryAfter = null)
        {
            if (func == null || funcParamRequestMessage == null)
                throw new ArgumentNullException("No function provided or its IMessage parameter is null");
            if (maxRetries <= 0)
                throw new ArgumentException("maxRetries is smaller than 1. Please provide maxRetries > 0");

            RequestMessage paramRequestMessage = new RequestMessage();
            List<string> tokensForBackOffPush = new List<string>();

            int retryCount = 1;
            if (retryAfter != null && retryAfter > 0)
                Thread.Sleep((int)retryAfter);
            while (retryCount <= maxRetries)
            {
                ResponseMessage = await func(funcParamRequestMessage);
                paramRequestMessage = (RequestMessage)funcParamRequestMessage;
                tokensForBackOffPush = new List<string>();

                if (ResponseMessage.Header.ResponseStatusCode == System.Net.HttpStatusCode.OK &&
                    ResponseMessage.Body.Failure == 0 && ResponseMessage.Body.CanonicalIds == 0)
                {
                    return ResponseMessage;
                }
                else
                {
                    for (var i = 0; i < ResponseMessage.Body.Results.Length; i++)
                    {
                        var result = ResponseMessage.Body.Results[i];
                        if (result.Error == null && result.RegistrationId == null)
                            continue;

                        if (!string.IsNullOrWhiteSpace(result.MessageId) && !string.IsNullOrWhiteSpace(result.RegistrationId))
                        {
                            if (paramRequestMessage.IsBulk)
                                PushNotificationService.updateTokenFunc?.Invoke(paramRequestMessage.Body.RegistrationIds[i], result.RegistrationId);
                            else
                                PushNotificationService.updateTokenFunc?.Invoke(paramRequestMessage.Body.To, result.RegistrationId);
                        }
                        else if (result.Error == ResponseMessageError.NotRegistered)
                        {
                            if (paramRequestMessage.IsBulk)
                                PushNotificationService.deleteTokenFunc?.Invoke(paramRequestMessage.Body.RegistrationIds[i]);
                            else
                                PushNotificationService.deleteTokenFunc?.Invoke(paramRequestMessage.Body.To);
                        }
                        else if (result.Error == ResponseMessageError.Unavailable ||
                                 result.Error == ResponseMessageError.InternalServerError ||
                                 result.Error == ResponseMessageError.DeviceMessageRateExceeded ||
                                 result.Error == ResponseMessageError.TopicsMessageRateExceeded)
                        {
                            if (paramRequestMessage.IsBulk)
                                tokensForBackOffPush.Add(paramRequestMessage.Body.RegistrationIds[i]);
                            else
                            {
                                tokensForBackOffPush.Add(paramRequestMessage.Body.To);
                                break;
                            }
                        }
                    }

                    if (tokensForBackOffPush.Count > 0)
                    {
                        TimeSpan sleepTime;
                        if (retryType == BackoffType.Linear)
                            sleepTime = TimeSpan.FromSeconds(waitBetweenRetrySec);
                        else
                            sleepTime = TimeSpan.FromSeconds(waitBetweenRetrySec * retryCount);

                        Thread.Sleep(sleepTime);
                        retryCount++;

                        if (tokensForBackOffPush.Count < ResponseMessage.Body.Results.Length && retryCount <= maxRetries)
                        {
                            //deep copy is not neccessary, if remove - do more testing
                            RequestMessage newRequestMessage = (RequestMessage)paramRequestMessage.DeepCopy();
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

                            funcParamRequestMessage = newRequestMessage;
                        }
                    }
                    else
                        break;
                }
            }

            //At this point ResponseMessage == paramRequestMessage (registrationToken-Count wise)
            //if tokensForBackOffPush has elements(registrationTokens), than those failed to be sent a notification
            //add registrationId to ResponseMessage.Body.Results[i].OriginalRegistrationId and RequestRetryStatus, for debugging or logging purposes
            //otherwise all notifications were sent successfully
#if TRACK_FAILED_REQUESTS_ENABLED
            if (tokensForBackOffPush.Count > 0)
            {
                if (paramRequestMessage.IsBulk)
                {
                    for (int i = 0; i < tokensForBackOffPush.Count; i++)
                    {
                        for (int j = 0; j < paramRequestMessage.Body.RegistrationIds.Length; j++)
                        {
                            if (tokensForBackOffPush[i] == paramRequestMessage.Body.RegistrationIds[j])
                            {
                                if (retryCount >= maxRetries)
                                    ResponseMessage.Body.Results[j].RequestRetryStatus = PushMessageStatus.FailedWithMaxRetries;
                                else
                                    ResponseMessage.Body.Results[j].RequestRetryStatus = PushMessageStatus.FailedWithPartialRetries;

                                ResponseMessage.Body.Results[j].OriginalRegistrationId = paramRequestMessage.Body.RegistrationIds[j];
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (retryCount >= maxRetries)
                        ResponseMessage.Body.Results[0].RequestRetryStatus = PushMessageStatus.FailedWithMaxRetries;
                    else
                        ResponseMessage.Body.Results[0].RequestRetryStatus = PushMessageStatus.FailedWithPartialRetries;
                    ResponseMessage.Body.Results[0].OriginalRegistrationId = paramRequestMessage.Body.To;
                }
            }
#endif
            return ResponseMessage;
        }

        /// <summary>
        /// This a generic function that provides retry mechanism based on BackoffType approach
        /// </summary>
        /// <param name="func">The provided function to execute with array of objects as input parameters and an object as output</param>
        /// <param name="maxRetries">The maximum number of retries</param>
        /// <param name="waitBetweenRetrySec">The wait time in seconds between retries</param>
        /// <param name="retryStrategy">The retry approach to execute the provided 'func' function</param>
        /// <param name="retryAfter">Start this method after 'retryAfter' which is in milliseconds</param>
        /// <param name="vals">list of parameters that go as parameters to 'func' method for execution</param>
        /// <returns></returns>
        public object ExecGenericFuncWithRetry(Func<object[], object> func,
                                               int maxRetries, int waitBetweenRetrySec, BackoffType retryStrategy, int? retryAfter = null,
                                               params object[] vals)
        {
            //TO BE IMPLEMENTED FOR FUTURE CASES
            return null;
        }
    }
}
