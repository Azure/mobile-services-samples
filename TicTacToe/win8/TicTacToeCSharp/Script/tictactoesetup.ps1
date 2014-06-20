Param(
[string]$subscriptionFile,
[string]$subscriptionId = "",
[string]$serviceName,
[string]$serverName = "",
[string]$databaseName = "",
[string]$serverAdmin,
[string]$serverPassword)

azure account import $subscriptionFile
if ($LastExitCode -ne 0)
{
   "Error: Could not import subscription file."
   break
}

if ($subscriptionId)
{
   azure account set $subscriptionId
}

if ($serverName)
{
    if ($databaseName -eq "")
    {
                    "Error: if you include a serverName, you must specify the databaseName."
                    break
    }

	"Using existing database: " + $serverName + " " + $databaseName
    azure mobile create -r $serverName -d $databaseName $serviceName $serverAdmin $serverPassword
    if ($LastExitCode -ne 0)
    {
                    "Error: Could not create mobile service."
                    break
    }
}
else
{
    azure mobile create $serviceName $serverAdmin $serverPassword
    if ($LastExitCode -ne 0)
    {
                    "Error: Could not create mobile service."
                    break
    }
}
$tables="games","moves","userfriends","users"
foreach ($table in $tables)
{
    azure mobile table create $serviceName $table -p "read=user,insert=user,update=user,delete=user"
    azure mobile script upload $serviceName table/$table.delete.js
    azure mobile script upload $serviceName table/$table.insert.js
    azure mobile script upload $serviceName table/$table.read.js
    azure mobile script upload $serviceName table/$table.update.js
}
azure mobile api create $serviceName getgamesforuser -p "post=user,get=user"
azure mobile script upload $serviceName api/getgamesforuser.js
