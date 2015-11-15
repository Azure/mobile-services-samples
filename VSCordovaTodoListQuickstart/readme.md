#Mobile Services quickstart app for Apache Cordova Tools for Visual Studio

Mobile Services gets you started faster by providing a quickstart TodoList sample project for your mobile service, which you can download from the [Azure Management portal](https://manage.windowsazure.com). Sample projects are supported for most mobile device platforms, including PhoneGap/Cordova. These downloaded sample project are pre-configured to connect to your mobile service so that you can simply run the app and store items in Azure.  

[Apache Cordova Tools for Visual Studio](http://www.visualstudio.com/en-us/explore/cordova-vs.aspx) let you work with Cordova and PhoneGap projects directly in the Visual Studio IDE. However, the current PhoneGap quickstart project download from the portal currently can't be opened directly in Visual Studio. This project contains an Apache Cordova Tools for Visual Studio project that is an equivalent quickstart app.

## Prerequisites

You need the following to be able to run this quickstart project:

+ An Active Microsoft Azure subscription. You can sign-up for a trial account [here](http://www.windowsazure.com/en-us/pricing/free-trial/).
+ Visual Studio 2015 including Tools for Apache Cordova. 

## Configure the project

You can use the Add Connected Service wizard in Visual Studio to connect your project to a mobile service, and even create a new mobile service. For this sample project, use the following steps:

1. Log on to the [Azure Management Portal](https://manage.windowsazure.com/), click **Mobile Services**. 

	If you already created your mobile service and TodoItem table, skip down to step 5.

2. (Optional) If you haven't already created a Mobile Service, you can follow the steps at [How to create a new mobile service](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-how-to-create-new-service/). 

3. (Optional) If you don't already have a TodoItem table, click the **Data** tab, click the **Create** button, supply a **Table Name** of `TodoItem`, then click the check button.

4. Click the **Dashboard** tab and make a note of the value of your **Mobile Service URL**.

5. Click **Manage Keys** and make a note of the **Application Key** for your mobile service. 

6. In the Solution Explorer in Visual Studio, navigate to the `\scripts` project subfolder and open the index.js file.

7. Locate the **MobileServiceClient** constructor and replace the values of the `AppUrl` and `AppKey` variables with the values you just obtained for your mobile service.

8. Because the sample implements authentication, you must also complete the Azure configuration steps in [Add authentication to you Mobile Services app](https://azure.microsoft.com/en-us/documentation/articles/mobile-services-html-get-started-users/).

Now, your app is connected to your mobile service and you can sign-in users and store data in Azure.

##Build and test the app

1. Make sure that Visual Studio debugging is configured for the desired platforms.

2. Follow the instructions to run your app on one of the supported devices or emulators:
 
	+ [iOS project](http://msdn.microsoft.com/en-us/library/dn757056.aspx)

	+ [Android project](http://msdn.microsoft.com/en-us/library/dn757059.aspx)
	
	+ [Windows Phone project](https://msdn.microsoft.com/en-us/library/dn757055.aspx)
			 
	+ [Windows project](https://msdn.microsoft.com/en-us/library/dn771547.aspx)
	
	+ [Apache Ripple Emulator](https://msdn.microsoft.com/en-us/library/dn757052.aspx)

3. Once the app loads, type some text into the textbox and then click **Add**.

	This sends a POST request to the new mobile service hosted in Azure. Data from the request is inserted into the **TodoItem** table. 

4. Stop and then restart the app, or run it on a separate device, to verify that the previously inserted data is returned from the mobile service and displayed.
