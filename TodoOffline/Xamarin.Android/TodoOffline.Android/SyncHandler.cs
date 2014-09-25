using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace TodoOffline
{
    class SyncHandler : MobileServiceSyncHandler
    {
        const string LOCAL_VERSION = "Use local version";
        const string SERVER_VERSION = "Use server version";

        Activity activity;

        public SyncHandler(Activity activity)
        {
            this.activity = activity;
        }

        public override async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
        {
            MobileServicePreconditionFailedException error;

            do
            {
                error = null;

                try
                {
                    return await operation.ExecuteAsync();
                }
                catch (MobileServicePreconditionFailedException ex)
                {
                    error = ex;
                }

                if (error != null)
                {
                    var localItem = operation.Item.ToObject<ToDoItem>();
                    var serverValue = error.Value;

                    var builder = new AlertDialog.Builder(this.activity);
                    builder.SetTitle("Conflict between local and server versions");
                    builder.SetMessage("How do you want to resolve this conflict?\n\n" + "Local item: \n" + localItem +
                        "\n\nServer item:\n" + serverValue.ToObject<ToDoItem>());

                    var clickTask = new TaskCompletionSource<int>();

                    builder.SetPositiveButton(LOCAL_VERSION, (which, e) =>
                    {
                        clickTask.SetResult(1);
                    });
                    builder.SetNegativeButton(SERVER_VERSION, (which, e) =>
                    {
                        clickTask.SetResult(2);
                    });
                    builder.SetOnCancelListener(new CancelListener(clickTask));

                    builder.Create().Show();

                    int command = await clickTask.Task;
                    if (command == 1)
                    {
                        // Overwrite the server version and try the operation again by continuing the loop
                        operation.Item[MobileServiceSystemColumns.Version] = serverValue[MobileServiceSystemColumns.Version];
                        continue;
                    }
                    else if (command == 2)
                    {
                        return (JObject)serverValue;
                    }
                    else
                    {
                        operation.AbortPush();
                    }
                }
            } while (error != null);

            return null;
        }

        class CancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            TaskCompletionSource<int> clickTask;

            public CancelListener(TaskCompletionSource<int> clickTask)
            {
                this.clickTask = clickTask;
            }

            public void OnCancel(IDialogInterface dialog)
            {
                clickTask.SetResult(0);
            }
        }
    }
}