Azure Mobile Service TodoSPA Angular JS
====================

This sample demonstrates the use of Azure Mobile Service JavaScript SDK, together with AngularJS, and it's inspired by this sample:

[Azure AD - ADAL JS Single Page App using AngularJS](https://github.com/AzureADSamples/SinglePageApp-AngularJS-DotNet)

Authentication with different authentication provider is supported in login page  
Supports CRUD operations on a TodoItem table

Dependencies:  
- [AngularJS](http://www.angularjs.org)
- [Mobile Service JS SDK](https://github.com/Azure/azure-mobile-services/blob/master/CHANGELOG.javascript.md)
- [angular-azure-mobile-service module](https://github.com/TerryMooreII/angular-azure-mobile-service)

How to run this sample:

In order to use this sample an instance of Azure Mobile Services is needed, for simplicity using JavaScript backend. 
_TodoItem_ table can be automatically generated, from Management Portal dashboard, as well as its controller in this case. 

- Create a _TodoItem_ table in your Mobile Service backend. This can be created automatically from the Azure Management Portal start page of your Mobile Service.  
[Get started with Mobile Services](https://azure.microsoft.com/en-us/documentation/articles/mobile-services-html-get-started/) article provides a good walkthrough on starting up and creating table.
- Optional: you can [Add authentication to your Mobile Services app](https://azure.microsoft.com/en-us/documentation/articles/mobile-services-html-get-started-users/). Different authentication provider are supported in login page.
- Set values for `API_URL` and `API_KEY` in _App/Scripts/app.js_ file, with the URL and the application key respectively; for your Azure Mobile Service. 
