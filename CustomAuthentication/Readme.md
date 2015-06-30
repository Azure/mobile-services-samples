#Mobile Services custom authentication with Azure Table storage

This sample demonstrates adding custom authentication to the TodoList quickstart mobile service .NET backend project. By default, this project uses Azure SQL Database for data storage. This solution includes a Universal Windows 8 project that is used to test the custom authentication in the mobile service.

This sample is based on and supports the following tutorials:

+ [Get started with custom authentication](https://azure.microsoft.com/en-us/documentation/articles/mobile-services-dotnet-backend-get-started-custom-authentication/)  <br/>This sample project extends the custom authentication tutorial to add Universal Windows 8 client projects to the solution that login to the custom authentication provider.
 
+ [Build a .NET backend Mobile Service that uses Table storage instead of a SQL Database](https://azure.microsoft.com/en-us/documentation/articles/mobile-services-dotnet-backend-store-data-table-storage/)<br/>By following the TODO instructions in code comments in the mobile service project, you can switch from using SQL Database to using Azure Table storage. 
##Prerequisites 

You will need the following to run the full mobile service .NET backend project in Azure:

+ Visual Studio 2013 Update 3, or a later edition
+ A Microsoft Azure account
+ A .NET backend mobile service
+ An Azure storage account

##Important considerations
Please be aware of the following best practice and security considerations:

+ The client sample app shortcuts the registration process by hard-coding the user inputs needed to register for custom authentication. In a real-world app, you *must never hard-code credentials*. Instead, you must add a new UI that collects user input to create the registration.

+ The registration process should, at a minimum, also include some verification process to prevent spam or malicious registrations. Integration with SendGid in Azure is one way to achieve this. While important, this kind of registration verification system is outside the scope of this sample.

##Configure the sample solution to run on Azure

This .NET backend project should run on your local computer without any changes. You need to complete the following steps to configure the projects to run on Azure using SQL Database.

1. If you haven't already done so, create your new .NET backend mobile service in one of the following ways:
	+ [Directly from Visual Studio](https://github.com/Azure/azure-content/blob/master/includes/mobile-services-dotnet-backend-create-new-service-vs2013.md) 
	+ [At the Azure Management Portal](https://github.com/Azure/azure-content/blob/master/includes/mobile-services-dotnet-backend-create-new-service.md)  

2. In the mobile service project, open the Web.config file , locate the **appSettings** key `MS_MobileServiceName` and change the value to the name of your mobile service.

3. Publish your mobile service to Azure, as detailed [here](https://github.com/Azure/azure-content/blob/master/includes/mobile-services-dotnet-backend-publish-service.md).

4. Open the App.xaml.cs file in the Shared project folder comment-out the constructor with localhost, uncomment the constructor with the remote URL, and replace  `YOUR_MOBILE_SERVICE_URL` and `YOUR_APPLICATION_KEY` with the values for your mobile service. You can find these in the **Dashboard** tab for your mobile service in the [Azure Management Portal].

5. Open the shared MainPage.cs file, expand the `custom_auth_fields` region and notice the hard-coded user registration values. In a real-world app, you must remove these and implement UI that collects user-supplied values at runtime.

At this point, the client app will run against the remote service and sign-in accounts are created in the **Accounts** table in the SQL Database. 

##Run the client app

1. Build and run either the Windows Store or Windows Phone app. Notice that the registration is created using the hard-coded info.

2. Click the **Sign in** button, notice that the stored credentials are pre-populated to make it easy to test. You must also remove this is your real-world app.

3. Click **Sign in** again to complete the authentication. Data is returned from the data store after authentication succeeds. 

##Switch data storage from SQL Database to Azure Table storage 

1. If you haven't already done so, [create an Azure storage account](https://azure.microsoft.com/en-us/documentation/articles/storage-create-storage-account/).

2. In the [Azure Management Portal], click **Storage**, click the storage account, then click **Manage Keys** and make a note of the **Storage Account Name** and **Access Key**.

2. Open the Web.config file in the mobile service project, locate the connection string named `StorageConnectionString` and replace `YOUR_STORAGE_ACCOUNT` and `YOUR_STORAGE_ACCOUNT_KEY` with the values from the portal.

3. In the portal page for the mobile service, click **Configure** and create a new connection string named `StorageConnectionString` that is the same string as in the Web.config file. This makes sure that our service can connect to you Azure Table storage account both when running locally and when running in Azure.

4. In the TodoItem.cs and Account.cs service files, change the object inheritance from **EntityData** to **StorageData**.

5. In the TodoItemController.cs, CustomRegistrationController.cs, and CustomLoginController.cs files, follow the *TODO* prompts in code comments to switch the data access from Entity Framework to the storage client. 

6. Republish the project. 

>**NOTE**<br/>At this point when you test the client, no data is returned after successful authentication. This is because unlike when using Entity Framework Code First, there is no mechanism to populate the store when a new table is created.

[Azure Management Portal]: https://manage.windowsazure.com/ 

