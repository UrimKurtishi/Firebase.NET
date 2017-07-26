## Welcome to Firebase.NET

![Firebase.NET Logo](https://raw.githubusercontent.com/UrimKurtishi/Firebase.NET/master/FirebaseNET.jpg) ![FCM Logo](https://firebase.google.com/_static/74dcb9f23a/images/firebase/lockup.png)     ![.NET Logo](https://raw.githubusercontent.com/UrimKurtishi/Firebase.NET/master/docs/NET.jpg)

Firebase.NET implements Firebase Cloud Messaging HTTP Protocol that enables sending notifications to Android, iOS and Web clients through Firebase Cloud Messaging. 

It is written entirely in C# and can be used in any c# or .net projects.


### Firebase Cloud Messaging (FCM) HTTP Protocol
The FCM service enables developers to push notifications to their client apps to Android, iOS and Web clients. To use it, create a project in the [FCM console](https://console.firebase.google.com)


### Firebase.NET Library

The library provides for models that can be pushed to client apps:
* AndroidNotification
* IosNotification
* WebNotification
* CrossPlatformNotification

### How to use Firebase.NET

Below is a sample on pushing notifications to your client apps through Firebase.NET

```csharp
using Firebase.NET.Messages;
using Firebase.NET.Notifications;
using Firebase.NET.Infrastructure;

RequestMessage requestMessage = new RequestMessage();
string[] ids = {
    //registration tokens here
};

requestMessage.Body.RegistrationIds = ids;
CrossPlatformNotification notification = new CrossPlatformNotification();
notification.Title = "Important News";
notification.Body = String.Format("This is a notification send from Firebase on {0} {1}", 
                                   DateTime.Now.ToShortDateString(),
                                   DateTime.Now.ToShortTimeString());
requestMessage.Body.Notification = notification;

Payload payload = new Payload();
payload["leage"] = "UEFA";
payload["game"] = "Albania vs Kosovo";
payload["score"] = "1:1";
requestMessage.Body.Data = payload;

Func<string, string, bool> updateFunc = new Func<string, string, bool>(Update);
Func<string, bool> deleteFunc = new Func<string, bool>(Delete);

PushNotificationService pushService = new PushNotificationService(updateFunc, deleteFunc);

var responseMessage = await pushService.PushMessage(requestMessage);

```


### Installation & Setup

Clone or download the source file and include & build the source within your project. An advantage factor is the ability to change or add new functionality based on the needs of the application.
Also, it can be install through NuGet Package Manager.

Check out our [nuget package](https://help.github.com/categories/github-pages-basics/).
