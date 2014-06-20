#TicTacToe Mobile Services sample

Using this sample as a starting point, you can create a variety of turn-based games that users play over a network.  The TicTacToe sample app is a simple Windows Store app that multiple users can play on various devices that are running Windows 8.1. The app stores game state by using mobile services and communicates updates by using push notifications.

There are two versions of the mobile services backend, the JavaScript backend (in the folder service-js), and the .NET backend (in the folder service-net) which is based on the Web API mobile services backend.  The same Windows 8.1 front-end is used for both.  The .NET backend and the JavaScript backend have the same functionality, so you can compare how the same task is done in both backends.

# Installation and Setup

## Setting up Permissions and Authentication
1. In Solution Explorer, open the shortcut menu for the TicTacToeCSharp project, and then choose **Associate App With the Store**.
2. Specify an app name that isn't already in use. Visual Studio updates the Package.appxmanifest file and adds a store key file with the .pfx extension to your project. 
3. Navigate to the **My Applications** page in the [Live Connect Developer Center](http://go.microsoft.com/fwlink/p/?LinkId=262039), log on with your Microsoft account, if required, then click the app that you just registered with the Store. 
4. Click **Edit Settings**, then click **API settings**, and in the **Redirect domain** text box, enter the URL of the mobile service, http://YourMobileServiceName.azure-mobile.net/signin-microsoft, then click **Save**. 
5. Make a note of the values of Client ID, Client Secret, and Package SID, which you'll need in the next steps. 
6. In the Windows Azure management portal, click the **Identity** tab and under **Microsoft Account** enter the Client ID and Client Secret values obtained from your identity provider, then click Save. 
7. Click the **Push** tab, and enter the Package SID value (the Client Secret is already there), then click Save. Now, you are ready to build and run the sample. 
8. On the Dashboard tab, choose the **Manage Keys** button. 
9. Choose the **Copy** button to copy the application key for your mobile service to the clipboard. 
10. In the TicTacToeCSharp solution, open App.xaml.cs, and locate the first line of the App class. 
11. In the call to the constructor for the MobileServiceClient, insert the name of your mobile service, and paste the application key where requested. 

## Building the Windows 8.1 Sample

1. In Visual Studio, create a solution and add the two projects, TicTacToeCSharp and TicTacToeLibraryCSharp.
2. Build the solution. This step also downloads and restores the packages on which the sample depends. 
2. Start on the local machine, and then sign in with a Microsoft account when requested. 
3. In the top-right corner, enter a user name in the box to create a player, and then choose the **Register** button. When you sign in to TicTacToe with the same Microsoft account, you'll be signed in automatically as this player. 
4. Before you play a game, create another player by performing one of the following sets of steps:

	* Close the app [Keyboard: Alt+F4], restart the app, sign in using a different Microsoft account, and then repeat the previous step with a different user name.
 
	* Install the sample on another computer or virtual machine that's running Windows 8.1, and then sign in using a different Microsoft account. If the computer or virtual machine isn't running Visual Studio 2013, you must sideload the app. See [Sideload Windows Store Apps](http://go.microsoft.com/fwlink/?LinkId=330355).

	* You might want to start by creating both players on the same computer and, later, install the app on a different computer to test simultaneous play. 

5. Perform the following steps to create a list of friends and then start a game with one of them:

	1. In the Search for user text box, enter a user name that you created, and then choose the Search button. 
	2. In the **Search Results** box, choose a player, and then choose the **Add Friend** button. 
	3. (Optional) Repeat the previous two steps to add more players. 
	4. In the **Opponent** text box, choose a player in the list that you just created, choose **New Game**, make a move, and then choose the **Submit Move** button. 



## Source Code Files

win8\TicTacToeCSharp - the Windows Store 8.1 Project

* Script folder - mobile service scripts. These are uploaded to the server when you set up the sample, so are not built with the project. 
* push.register.cs - registers for push notifications and handles push notification events 
* App.xaml.cs - the core App class 
* MainPage.xaml.cs - the page layout, authentication of the user 

win8\TicTacToeLibraryCSharp - most of the TicTacToe game mechanics, needed by the TicTacToeCSharp project.

* GameProcessor.cs - contains most of the game code, including the ITurnGame interface, the TicTacToe game class, and the GameProcessor class, which handle most of the game mechanics. 
* BindableBase.cs - this is part of the databinding model that this app uses, and is part of standard templates 
* Cell.cs, Player.cs, etc. - various game elements 
* TicTacToeMobileServiceClient.cs - all the code that calls into mobile services is here 

service-net\TicTacToeMobileService - A Visual Studio project that contains the .NET backend mobile service. In Visual Studio, use the Publish menu item to publish to the mobile service in Azure.

* App_Start\WebApiConfig.cs - standard for Web API projects, this file contains startup and initialization code for the Web API backend 
* Controllers - these are the main REST endpoints for the mobile service, including a controller for each mobile service table and one for the custom API GetGamesForUser. 
* DataObjects - these are the classes that represent the data types used in the mobile service 
* Models - this includes the Entity Framework data context that is standard in Web API projects that use Entity Framework 

service-js - the JavaScript backend service

* api - Includes all the custom API script getgamesforuser.js
* tables - Includes all the table scripts for the JavaScript backend mobile services

The standard structure of the service-js folder matches the git repository for a mobile service so you can easily create a mobile service and push the sample to its repo to install it.