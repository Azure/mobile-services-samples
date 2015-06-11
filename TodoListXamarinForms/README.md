# Xamarin.Forms and Azure Mobile Services

This Xamarin.Forms solution contains an iOS, Android, and Windows Phone (Silverlight) project. All code to connect to Azure is contained in the shared portable class library. 

To run this sample, you'll have to do two things. First, create a mobile service and a table. You can follow the guidance in this section to do that: https://azure.microsoft.com/en-us/documentation/articles/mobile-services-android-get-started/#create-a-new-mobile-service. You only have to complete the steps in that section. The rest of that topic does not apply to this sample.

Next, open the Constants.cs file in the portable class library project of the solution and replace the value of the ApplicationURL and ApplicationKey fields with the URL and Key of your Azure Mobile Service.



