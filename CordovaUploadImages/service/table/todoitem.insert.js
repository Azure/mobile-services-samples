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
console.log(item.containerName); //debugging
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
              
			// Use this version of sasQueryUrl when uploading mutiple blobs.			  
			/* var sasQueryUrl = 
              blobService.generateSharedAccessSignature(item.containerName, 
              '', sharedAccessPolicy); */

              // Set the query string.
              item.sasQueryString = qs.stringify(sasQueryUrl.queryString);

              // Set the full path on the new new item, 
              // which is used for data binding on the client. 
              item.imageUri = sasQueryUrl.baseUrl + sasQueryUrl.path + '/' + item.resourceName;          

          } else {
              console.error(error);
          }
          request.execute();
      });
  } else {
      request.execute();
  }
}