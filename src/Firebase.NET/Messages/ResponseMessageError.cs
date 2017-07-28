namespace Firebase.NET.Messages
{
    /// <summary>
    /// The link on Firebase documentation provides a detail description of each error,
    /// https://firebase.google.com/docs/cloud-messaging/http-server-ref#table9
    /// </summary>
    public enum ResponseMessageError
    {
        MissingRegistration, // client app receiver (registration token) missing
        InvalidRegistration, //
        NotRegistered,       // registration token not registered, should be DELETED from database
        InvalidPackageName,  // 
        Unauthorized,        // 401,
        MismatchSenderId,
        BadRequest,          // 400 Invalid Json
        InvalidParameters,
        MessageTooBig,       // max 4kb and for ios 2kb
        InvalidDataKey,
        InvalidTtl,
        Unavailable,
        InternalServerError,
        DeviceMessageRateExceeded,
        TopicsMessageRateExceeded,
        InvalidApnsCredential
    }
}
