## Welcome to Firebase.NET

![Firebase.NET Logo](https://raw.githubusercontent.com/UrimKurtishi/Firebase.NET/master/FirebaseNET.jpg) ![FCM Logo](https://firebase.google.com/_static/74dcb9f23a/images/firebase/lockup.png)  &nbsp  ![.NET Logo](https://raw.githubusercontent.com/UrimKurtishi/Firebase.NET/master/docs/NET.jpg)

Firebase.NET implements Firebase Cloud Messaging HTTP Protocol that enables sending notifications to Android, iOS and Web clients through Firebase Cloud Messaging. 

It is written entirely in C# and can be used in any c# or .net projects.


## Firebase Cloud Messaging (FCM) HTTP Protocol
The FCM service enables developers to push notifications to their client apps to Android, iOS and Web clients. To use it, create a project in the [FCM console](https://console.firebase.google.com)


## Firebase.NET Library

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

### Header 3

- Bulleted
- List

1. Numbered
2. List

**Bold** and _Italic_ and `Code` text

[Link](url) and ![Image](src)

For more details see [GitHub Flavored Markdown](https://guides.github.com/features/mastering-markdown/).

### Jekyll Themes

Your Pages site will use the layout and styles from the Jekyll theme you have selected in your [repository settings](https://github.com/urimkurtishi/Firebase.NET/settings). The name of this theme is saved in the Jekyll `_config.yml` configuration file.

### Support or Contact

Having trouble with Pages? Check out our [documentation](https://help.github.com/categories/github-pages-basics/) or [contact support](https://github.com/contact) and we’ll help you sort it out.
