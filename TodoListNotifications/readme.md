# TodoList notifications sample for PhoneGap 

This sample shows how to use Azure Mobile Services integrated with Azure Notification Hubs to send push notifications to your PhoneGap app on three client platforms: iOS, Android, and Windows Phone 8. This sample uses a mobile service that is integrated with Notification Hubs, so that you can use Mobile Services REST APIs to create push notification registrations in the Notification Hubs.

## Prerequisites 
To run this PhoneGap sample app on one or more of the supported client platforms, you must have the following:

+ An Active Microsoft Azure subscription. You can sign-up for a trial account [here](http://www.windowsazure.com/en-us/pricing/free-trial/).
+ Completed the topic [Create a new mobile service]. After completing this tutorial, you will have a JavaScript backend mobile service. You can also use the same mobile service that you created when you completed the [PhoneGap quickstart tutorial](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-javascript-backend-phonegap-get-started/) quickstart tutorial. 
+ PhoneGap version 3.4 or a later version.
+ For a given client platform, you must also meet these platform-specific requirements:

	**iOS:**
	+ A physical iOS device, iPhone or iPad.
	+ [Xcode](https://go.microsoft.com/fwLink/p/?LinkID=266532) v4.4 or a later version running on a MacOSX environment.
	+ [iOS Developer Program](https://developer.apple.com/programs/ios/develop.html) registration (required to configure push notifications)

	**Android**
	+ (Optional) A physical Android device (an emulator could work, but it's a bit more complicated and not covered here).
	+ [Android Developer Tools](). Android 4.4.2 (API 19) only has been tested.
	+ Google Play Services and Google APIs must also be installed.
	+ Google account that has a verified email address. To create a new Google account, go to <a href="http://go.microsoft.com/fwlink/p/?linkid=268302&clcid=0x409" target="_blank">accounts.google.com</a>.

	**Windows Phone 8**
	+ A version of Visual Studio that supports Windows Phone development, such as:
		+ [Visual Studio 2013 Express](http://www.visualstudio.com/downloads/download-visual-studio-vs#d-express-windows-8) (or a higher edition)
		+ [Visual Studio 2012 Express for Windows Phone](https://go.microsoft.com/fwLink/p/?LinkID=268374)
	+ A [Windows developer account](http://msdn.microsoft.com/library/windows/apps/jj193592) (required to configure push notifications)

##Configure platform-specific push notifications 

1. Complete the steps to register your app to authenticate for push notifications with each platform-specific push service:

	+ **iOS:** Apple Push Notification Services (APNS)
	
		Complete the topic [How to enable Apple push notifications](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-how-to-enable-apple-push-notifications/).

	+ **Android:** Google Cloud Messaging (GCM)

		Complete the topic [How to enable Google Cloud Messaging](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-how-to-enable-google-cloud-messaging/). 
		
		<!---To add Google Play Services to your Android app, you will also need to follow the steps in the **Add Google Play Services to the project** section of [Get started with Notification Hubs](http://azure.microsoft.com/en-us/documentation/articles/notification-hubs-android-get-started/).-->

	+ **Windows Phone 8:** Microsoft Push Notification Service (MPNS)
	 
		Since we are doing unauthenticated push to Windows Phone 8 devices, we don't need to configure authentication with MPNS.  
		
2. Log on to the [Azure Management Portal](https://manage.windowsazure.com/), click **Mobile Services**, click your app, then click the **Push** tab. 

3. If integration with Notification Hubs has not been completed, click **Enable Enhanced Push**. 

4. Supply the authentication credentials for the supported push notification platforms:

	+ **APNS**
		
		Upload the .p12 certificate obtained in step 1. 


	+ **GCM**
			
		Set the API key value obtained in step 1.  

	+ **MPNS**
			
		Check **Enable unauthenticated push notifications**.
 
	Now, Notification Hubs can send push notifications on behalf of your app.

5. Click the **Data** tab, click the **TodoItem** table, click **Script** and replace the existing insert script with the following code from the `\service\table\todoitem.insert.js` project file:

		function insert(item, user, request) {
		    // Execute the request and send notifications.
		   request.execute({
		       success: function() {                      
		           // Create a template-based payload.
		           var payload = '{ "message" : "New item added: ' + item.text + '" }';            
		           // Write the default response and send a notification
		           // to all platforms.            
		           push.send(null, payload, {               
		               success: function(pushResponse){
		               console.log("Sent push:", pushResponse);
		               // Send the default response.
		               request.respond();
		               },              
		               error: function (pushResponse) {
		                   console.log("Error Sending push:", pushResponse);
		                    // Send the an error response.
		                   request.respond(500, { error: pushResponse });
		                   }           
		            });                 
		       }
		   });   
		}

	This sends a push notification to all registered devices when a new item is inserted. 
		
 
##Update the PhoneGap project for your Azure services

Now that you have push notifications configured, you need to update the sample app to point to your mobile service and notification hub.

1. In the management portal, click the **Dashboard** tab and make a note of the value of your **Mobile Service URL**.

2. Click **Manage Keys** and make a note of the **Application Key** for your mobile service. 

3. In the sample project, navigate to the `\phonegap\www` subfolder, open the config.xml file, locate the **access** element and replace the value of the **origin** attribute with the URL of your mobile service.  

	This allows your app to access data from the mobile service.

4. Navigate to the `\phonegap\www\js` subfolder and open the index.js file in your favorite JavaScript or text editor.

5. Replace the values of the `MOBILE_SERVICE_URL` and `MOBILE_SERVICE_APP_KEY` variables with the values you just obtained for your mobile service.

6. For Android, replace the value of the `GCM_SENDER_ID` variable with the project number value assigned when you registered your app in the Google Developers Console.

##Build and test the app

1. Verify that all of the target platform tools are accessible in the system path. 

2. Open a command prompt in the root project directory, and run one of the following platform-specific commands: 

	+ **iOS**
 
		Open terminal and run the following command:

    		phonegap local build ios

	+ **Android**

		Open a command prompt or terminal window and run the following command. 

		    phonegap local build android

	+ **Windows Phone**

		Run the following command from the Visual Studio Developer command prompt:

    		phonegap local build wp8

3.	Open and run the project on the device according to the instructions below for each platform:

	+ **Windows Phone 8**

		1. Windows Phone 8: Open the .sln file in the **\phonegap\platforms\wp8** folder in Visual Studio 2012 Express for Windows Phone.
		
		2. Press the **F5** key to rebuild the project and start the app.

	+ **iOS**

		1. Open the project in the **\phonegap\platforms\ios** folder in Xcode.
		
		2. Press the **Run** button to build the project and start the app in the iPhone emulator, which is the default for this project.

	+ **Android**

		1. In Eclipse, click **File** then **Import**, expand **Android**, click **Existing Android Code into Workspace**, and then click **Next.** 
				
		2. Click **Browse**, browse to the **\phonegap\platforms\android** folder, click **OK**, make sure that the todoitem project is checked, then click **Finish**. 
		 
			This imports the project files into the current workspace.
		
		3. From the **Run** menu, click **Run** to start the project in the Android emulator.
			
	After launching the app in one of the mobile emulators above, you will see a registration success alert. 

6. Type some text into the textbox and then click **Add**.

	This sends a POST request to the new mobile service hosted in Azure. Data from the request is inserted into the **TodoItem** table and a push notification is generated and sent to all registered devices. 

[Create a new mobile service]: http://azure.microsoft.com/en-us/documentation/articles/mobile-services-how-to-create-new-service/
