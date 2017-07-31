## Welcome to Firebase.NET

![Firebase.NET Logo](https://raw.githubusercontent.com/UrimKurtishi/Firebase.NET/master/FirebaseNET.jpg) 

Firebase.NET implements Firebase Cloud Messaging HTTP Protocol that enables sending notifications to Android, iOS and Web clients through Firebase Cloud Messaging. 

It is a small and robust .NET client library written entirely in C# with high performance and reliability. 
It provides:
* asynchronous pushes
* handling mechanism for response messages returned from FCM servers
* retry provider (implementing exponential backoff algorithm)
* serialization/deserialization etc.

It has been tested and it's used by different companies and projects.


### Firebase Cloud Messaging (FCM) HTTP Protocol
The FCM service enables developers to push notifications to their client apps in Android, iOS and Web. To use it, create a project in the [FCM console](https://console.firebase.google.com) and retrieve your servey key.


### Firebase.NET Library

Three main classes are **RequestMessage**, **ResponseMessage** and **PushNotificationService**.

#### RequestMessage
This class has two important properties: RequestMessageHeader and RequestMessageBody which contain all [FCM settings](https://firebase.google.com/docs/cloud-messaging/http-server-ref#downstream-http-messages-json) as properties. RequestMessageBody contains:
1. Notification <br/>
	This property contains pre-defined set of values as specified by the [FCM settings](https://firebase.google.com/docs/cloud-messaging/http-server-ref#downstream-http-messages-json). <br/>
	The library provides four notification types that can be pushed to client apps:
	* **AndroidNotification**
	* **IosNotification**
	* **WebNotification**
	* **CrossPlatformNotification**
2. Data 
    * This property is a payload (class **Payload** that inherits **Dictionary<string, string>**) that can be attached to the notification and can contain any key-value data.

#### ResponseMessage
This class contains information about each registration token's push status and retry status for tokens that failed to be pushed the first time.

#### PushNotificationService
This service has an async **PushMessage** method that receives a RequestMessage as a parameter to push to client apps and returns a ResponseMessage that contains information about each registration token's push status.
<br/>
PushnotificationService has several constructors available:<br/>
1. new PushnotificationService() <br/>
	This is the default constructor. If you use this, you must store Firebase server key in your app.config or web.config file as *appsettings* child element with the key name **"FirebaseServerKey"**. You can add two other settings: **"FirebaseSenderID"** and **"FirebaseConnectionEndpoint"**, however they are unneccessary, but if you provide them you must make sure to set FirebaseConnectionEndpoint to "https://fcm.googleapis.com/fcm/send" as provided by Google. Otherwise, leave out FirebaseSenderID (it's not needed to send pushes) and FirebaseConnectionEndpoint (is set by the library itself).
2. new PushnotificationService("yourFcmServcrKey")
	This constructor takes your Firebase server key and it's ready to push notifications.
3. **new PushnotificationService("yourFcmServcrKey", updateFunc, deleteFunc)**
	This is the recommended constructor to use for all your use cases. See below for more info about these two func parameters.
4. new PushnotificationService(updateFunc, deleteFunc)
	To use this constructor, you must set "FirebaseServerKey" in your app.config or web.config as stated above on the first constructor. Otherwise, it won't be able to push notifications.
5. new PushnotificationService(updateFunc, deleteFunc, appSettings)
	appSettings is instance of Firebase.NET.Infrastructure.ApplicationSettings
	It's a class that has FirebaseServerKey as property that you can set and send the class instance as parameter to PushNotificationService.
6. new PushnotificationService(appSettings)
	See below for appsettings.

<br/>
##### ApplicationSettings <br/>
This is a [class](https://github.com/UrimKurtishi/Firebase.NET/blob/master/src/Firebase.NET/Infrastructure/ApplicationSettings.cs) that has three properties as stated above by first constructor. This is another method of providing Firebase project data instead of storing them in your project's app.config or web.config file.

##### updateFunc, deleteFunc <br/>
Sometimes registration tokens of client apps can change based on different [scenarios](https://firebase.google.com/docs/cloud-messaging/http-server-ref#error-codes). In that case you need to delete the invalid one or if new one has been generated (which firebase servers will return it on response), it needs to be updated.
Firebase.NET library provides interface to achieve this by allowing the developer to implement those two types of functions and it will call them in case it needs to delete or update the old one respectively.
However it must comply with the signature as below. <br/>
The update function must accept two string parameters and return bool whereas delete function must accept one string parameter and return bool as well.<br/>
Naming of function and parameters doesn't matter only parameter and return type.

```csharp
/// <summary>
/// This method updates the old registration token with new registration token
/// Firebase.NET will call this method with appropriate tokens if Firebase servers return new token that should change old one
/// </summary>
public static bool Update(string oldToken, string newToken) 
{ 
	//update oldToken with newToken in your database
	return true; 
}

/// <summary>
/// This method deletes registration token that is invalid and you should not try to push notifications any longer to it.
/// </summary>
public static bool Delete(string oldToken) 
{ 
	//delete oldToken from your database
	return true; 
}

Func<string, string, bool> updateFunc = new Func<string, string, bool>(Update);
Func<string, bool> deleteFunc = new Func<string, bool>(Delete);
var pushService = new PushNotificationService("firebaseServerKey", updateFunc, deleteFunc);
```


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
	    //other available pre-defined properties such as badge, icon, sound etc
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

As of now, Firebase.NET implements only Firebase Cloud Messaging for push notifications. However, there are plans to develop other Firebase services as well such as Authentication, Realtime Database, Storage etc. <br/>
Contributors are welcome to assist in further extending Firebase.NET

