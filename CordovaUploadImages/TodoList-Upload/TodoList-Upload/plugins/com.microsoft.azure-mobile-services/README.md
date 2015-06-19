Apache Cordova plugin for Azure Mobile Services
=============================

With Microsoft Azure Mobile Services you can add a scalable backend to your connected client applications in minutes.

To learn more, visit our [Developer Center](http://azure.microsoft.com/en-us/develop/mobile/).

## Getting Started

If you are new to Mobile Services, you can get started by following our tutorials for connecting your Mobile Services cloud backend to [Cordova apps](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-javascript-backend-phonegap-get-started/),

### Sample usage ###
The following code creates a new client object to access the *todolist* mobile service and create a new proxy object for the *TodoItem* table.

    var mobileService = new WindowsAzure.MobileServiceClient(
            "https://todolist.azure-mobile.net",
            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
        );

    var todoTable = mobileService.getTable('TodoItem');

## Need Help?

Be sure to check out the Mobile Services [Developer Forum](http://social.msdn.microsoft.com/Forums/en-US/azuremobile/) if you are having trouble. The Mobile Services product team actively monitors the forum and will be more than happy to assist you.

## Contribute Code or Provide Feedback

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/Azure/azure-mobile-services/issues) section of the project.

## Learn More
[Microsoft Azure Mobile Services Developer Center](http://azure.microsoft.com/en-us/develop/mobile)