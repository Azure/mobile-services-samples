# Upload images from Cordova apps to Microsoft Azure services

This sample demonstrates how to use Azure Mobile Services to enable your Apache Cordova app to upload and store user-generated images in Azure Storage. Mobile Services uses a SQL Database to store data. However, binary large object (BLOB) data is more efficiently stored in Azure Blob storage service. This sample is an Apache Cordova Tools for Visual Studio 2015 RTM project. The REST APIs are used to access the Blob service.

>**Note:** 2015 RTM is required (the project structure is different in VS 2013 vs. VS 2015 RC/RTM).

To be able to upload an image to the Blob service, this sample generates a Shared Access Signature (SAS) that is returned to the client. The app then uses this temporary credential to upload the image. This SAS can be used for only 5 minutes before it expires. Note that the Azure Storage credentials are securely stored in your mobile service's app settings.  In this example, downloads from the Blob service are public.

>**Note:** You need to create and/or configure the Azure services before you can successfully run this sample.

## Prerequisites 
To run this Apache Cordova for Visual Studio sample app on one or more of the supported client platforms, you must have the following:

+ An Active Microsoft Azure subscription. You can sign-up for a trial account [here](http://www.windowsazure.com/en-us/pricing/free-trial/).
+ The [Visual Studio Tools for Apache Cordova](http://go.microsoft.com/fwlink/p/?LinkId=397606). See the topic  [Install Visual Studio Tools for Apache Cordova](http://msdn.microsoft.com/en-us/library/dn757054.aspx) for the complete list of prerequisites for these tools. 
+ An [Azure Storage account](http://azure.microsoft.com/en-us/documentation/articles/storage-create-storage-account/)
+ An iOS or Android device that has a camera.

>**Note:** The iOS simulator are not supported due to issues with image capture.

##Updates

July, 2015

+ Updated to a Dev14 RTM project (VS 2015 RTM required!).
+ Added support for grabbing images from local file (using FilePicker), and for dynamic switching between Camera and FilePicker.
+ Switched to Camera plugin from Media Capture plugin (the former allows use of FilePicker).
+ Updates to the app to support Win/WinPhone.
+ Updates to Camera plugin 0.3.6 to enable FilePicker support on Windows Phone (comments included).
+ Updates to Camera plugin 0.3.6 to enable ReadAsArrayBuffer support on Windows Phone (comments included).


##Configure the Azure backend services 
		
1. Log on to the [Azure Management Portal](https://manage.windowsazure.com/), click **Mobile Services**. 

	If you already created your mobile service and TodoItem table, skip down to step 4.

2. (Optional) If you haven't already created a Mobile Service, you can follow the steps at [How to create a new mobile service](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-how-to-create-new-service/). 

3. (Optional) If you don't already have a TodoItem table, click the **Data** tab, click the **Create** button, supply a **Table Name** of `TodoItem`, then click the check button.
 
4. (Optional) If you haven't yet created your storage account, see [How To Create a Storage Account](http://azure.microsoft.com/en-us/documentation/articles/storage-create-storage-account/).

5. In the Management Portal, click **Storage**, click the storage account, then click **Manage Keys** and make a note of the **Storage Account Name** and **Access Key**.

7. Back in your mobile service, click the **Configure** tab, scroll down to **App settings** and enter a **Name** and **Value** pair for each of the following that you obtained from the storage account, then click **Save**.

	+ `STORAGE_ACCOUNT_NAME`
	+ `STORAGE_ACCOUNT_ACCESS_KEY`

	![](./readme/mobile-blob-storage-app-settings.png)

	The storage account access key is stored encrypted in app settings. You can access this key from any server script at runtime. 

9. Click the **Data** tab, click the **TodoItem** table, click **Script** and replace the existing insert script with the following code, which you can also find in the \service\table\todoitem.insert.js file:

		var azure = require('azure');
		var qs = require('querystring');
		var appSettings = require('mobileservice-config').appSettings;
		
		function insert(item, user, request) {
		    // Get storage account settings from app settings. 
		    var accountName = appSettings.STORAGE_ACCOUNT_NAME;
		    var accountKey = appSettings.STORAGE_ACCOUNT_ACCESS_KEY;
		    var host = accountName + '.blob.core.windows.net';
		
		    if ((typeof item.containerName !== "undefined") && (
		    item.containerName !== null)) {
		        // Set the BLOB store container name on the item, which must be lowercase.
		        item.containerName = item.containerName.toLowerCase();
		
		        // If it does not already exist, create the container 
		        // with public read access for blobs.        
		        var blobService = azure.createBlobService(accountName, accountKey, host);
		        blobService.createContainerIfNotExists(item.containerName, {
		            publicAccessLevel: 'blob'
		        }, function(error) {
		            if (!error) {
		
		                // Provide write access to the container for the next 5 mins.        
		                var sharedAccessPolicy = {
		                    AccessPolicy: {
		                        Permissions: azure.Constants.BlobConstants.SharedAccessPermissions.WRITE,
		                        Expiry: new Date(new Date().getTime() + 5 * 60 * 1000)
		                    }
		                };
		
		                // Generate the upload URL with SAS for the new image.
		                var sasQueryUrl = 
		                blobService.generateSharedAccessSignature(item.containerName, 
		                item.resourceName, sharedAccessPolicy);
		
		                // Set the query string.
		                item.sasQueryString = qs.stringify(sasQueryUrl.queryString);
		
		                // Set the full path on the new new item, 
		                // which is used for data binding on the client. 
		                item.imageUri = sasQueryUrl.baseUrl + sasQueryUrl.path;
		
		            } else {
		                console.error(error);
		            }
		            request.execute();
		        });
		    } else {
		        request.execute();
		    }
		}

 	![](./media/mobile-services-configure-blob-storage/mobile-insert-script-blob.png)

   	This replaces the function that is invoked when an insert occurs in the TodoItem table with a new script. This new script generates a new SAS for the insert, which is valid for 5 minutes, and assigns the value of the generated SAS to the `sasQueryString` property of the returned item. The `imageUri` property is also set to the resource path of the new BLOB to enable image display during binding in the client UI.
 
##Update the Cordova project for your Azure services

Now that you have the storage code configured in Azure, you need to update the sample app to point to your mobile service.

1. In the management portal, click the **Dashboard** tab and make a note of the value of your **Mobile Service URL**.

2. Click **Manage Keys** and make a note of the **Application Key** for your mobile service. 

3. In the Solution Explorer in Visual Studio, open the scripts\index.js file.

5. Replace the values of the mobile service URL and application key variables in the following constructor with the values you just obtained for your mobile service:

		// Initialize the Mobile Services client here.
        var client = new WindowsAzure.MobileServiceClient(
            'https://MY_MOBILE_SERVICE.azure-mobile.net/',
            'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX');

Now, your app is able to access your mobile service to 

##<a name="test"></a>Test uploading the images in your app

1. Make sure that you Android or iOS device is connected and read for Visual Studio 2013 debugging using the Apache Cordova tools, then choose the platform (Android or iOS), make sure that **Device** is the selected debug location, then press the F5 key to run the app.

2. Enter text in the textbox under **Insert a TodoItem**, then click **Add**.

3. In the camera capture UI, take a picture and accept it. 

	![New image upload displayed in the list](./readme/device1.png)
 
	A new item is inserted, and after that the image is uploaded to Azure Storage. When the items are refreshed, the new image is downloaded and displayed in the items list on the device.

	>**Note:** When you cancel the camera capture, the new item is inserted without uploading an image. Tying the camera capture to the insert operation was done to simplify the UI. You might want to provide a UI that gives your own users more options.
