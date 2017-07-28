## Welcome to Firebase.NET

![Firebase.NET Logo](https://raw.githubusercontent.com/UrimKurtishi/Firebase.NET/master/FirebaseNET.jpg) 

Firebase.NET implements Firebase Cloud Messaging HTTP Protocol that enables sending notifications to Android, iOS and Web clients through Firebase Cloud Messaging. 

It is a small and lightweight .NET client library written entirely in C# with high performance and reliability. It provides handling mechanism for response messages returned from FCM servers and retry provider (implementing exponential backoff algorithm).
It has been tested and it's used by different companies and projects.


### Firebase Cloud Messaging (FCM) HTTP Protocol
The FCM service enables developers to push notifications to their client apps to Android, iOS and Web clients. To use it, create a project in the [FCM console](https://console.firebase.google.com)


### Firebase.NET Library

Three main classes are **RequestMessage**, **ResponseMessage** and **PushNotificationService**.

#### RequestMessage
This class has two important properties: RequestMessageHeader and RequestMessageBody which contain all [FCM settings](https://firebase.google.com/docs/cloud-messaging/http-server-ref#downstream-http-messages-json) as properties. RequestMessageBody contains:
1. Notification 
    * Notification property contains pre-defined set of values as specified by the [FCM settings](https://firebase.google.com/docs/cloud-messaging/http-server-ref#downstream-http-messages-json).
2. Data 
    * Data property is a payload that can contain custom key-value data as needed.

<br/>

The library provides four notification types that can be pushed to client apps:
* AndroidNotification
* IosNotification
* WebNotification
* CrossPlatformNotification

<br/>

### How to use Firebase.NET

Below is a sample on pushing notifications to your client apps through Firebase.NET

```csharp
using Firebase.NET.Messages;
using Firebase.NET.Notifications;

string[] ids = {
    //registration tokens here
};

var requestMessage = new RequestMessage
{
    Body =
    {
        RegistrationIds = ids,
        Notification = new CrossPlatformNotification
        {
            Title = "Important Notification",
            Body = "This is a notification send from Firebase.NET"
        },
        Data = new Dictionary<string, string>
        {
            { "leage", "UEFA" },
            { "game", "Albania vs Kosovo!" },
	    { "score", "1:1" }
        }
    }
};
       
var pushService = new PushNotificationService("yourFcmServerKey");
var responseMessage = await pushService.PushMessage(requestMessage);

```


### Installation & Setup

Clone or download the source file and include & build the source within your project. An advantage for this approach is the ability to change or add new functionality based on the needs of the application.
Also, it can be installed through NuGet Package Manager.

Check out our [nuget package](https://www.nuget.org/packages/Pantheon.Firebase.NET/1.1.0).


### Future Plans

As of now, Firebase.NET implements only Firebase Cloud Messaging for push notifications. However, there are plans to develop other Firebase services as well such as Authentication, Realtime Database, Storage etc.
Contributors are welcome to assist in further extending Firebase.NET

