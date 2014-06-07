This sample repo is organized with the folder structure below. This structure allows samples that demonstrate multiple client platforms, multiple backends, or both. 

Note that the root of the sample should contain a readme.md. The readme should contain information on all clients and backends for the sample.

When adding a new sample, add a description with a link on the main readme for the repo. This should include a description of each sample and what it does. We can optionally also categorize samples by features that they demonstrate.

    + readme.md [main repo readme]
	+ DescriptiveSampleName1/
        + readme.md [required]
		+ service-js/
			+ package.json
			+ table/
		        + todoitem.insert.js
		        + todoitem.read.js
	        + api/  
	        + scheduler/
	        + shared/
	        + extensions/
		+ service-net/
        + android/
        + ios/
        + winphone8/
        + win-universal/
        + win8/
        + ... (etc)

	+ DescriptiveSampleName2/
        ... (etc)

For the JavaScript backend, use the file `package.json`. See this [blog post](http://blogs.msdn.com/b/azuremobile/archive/2014/01/20/support-for-package-json-in-azure-mobile-services.aspx) for more information.

For the .NET backend, the project (or solution) file will be located in the folder `service-net`.

If a sample contains multiple client platforms, make sure they are all tested against the particular backend(s). 
