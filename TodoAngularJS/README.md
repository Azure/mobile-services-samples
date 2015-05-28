TodoSPA
====================

This sample demonstrates the use of Azure Mobile Service JavaScript SDK, together with AngularJS, and it's inspired from this sample:

[Azure AD - ADAL JS Single Page App using AngularJS](https://github.com/AzureADSamples/SinglePageApp-AngularJS-DotNet)

Works:
- Get Items
- Delete Item
- Insert Item
- Edit Item
- Authentication with Azure AD (hardcoded 'aad' provider name for now)
- Mobile Serice User information (uid, and access_token)

Dependencies:  
- [AngularJS](http://www.angularjs.org)
- [Mobile Service JS SDK](https://github.com/Azure/azure-mobile-services/blob/master/CHANGELOG.javascript.md)
- [angular-azure-mobile-service module](https://github.com/TerryMooreII/angular-azure-mobile-service)


Usage:
- Create a table named _TodoItem_ in your Mobile Service backend. This can be created automatically from the Azure Management Portal configuration page.
- Set values for `API_URL` and `API_KEY` in _app.js_ file, with the URL and the application key respectively; for your Azure Mobile Service. 

More details about this sample will come soon!
