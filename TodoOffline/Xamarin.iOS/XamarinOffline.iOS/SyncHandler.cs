using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using MonoTouch.UIKit;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace XamarinOffline
{
    class SyncHandler : IMobileServiceSyncHandler
    {
        MobileServiceClient client;
        const string LOCAL_VERSION = "Use local version";
        const string SERVER_VERSION = "Use server version";

        public SyncHandler(MobileServiceClient client)
        {
            this.client = client;
        }

        public virtual Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
        {
            return Task.FromResult(0);
        }
        
        public virtual async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
        {
            MobileServiceInvalidOperationException error;
            Func<Task<JObject>> tryOperation = operation.ExecuteAsync;

            do
            {
                error = null;

                try
                {
                    JObject result = await operation.ExecuteAsync();
                    return result;
                }
                catch (MobileServiceConflictException ex)
                {
                    error = ex;
                }
                catch (MobileServicePreconditionFailedException ex)
                {
                    error = ex;
                }

                if (error != null)
                {
                    var localItem = operation.Item.ToObject<ToDoItem>();
                    var serverValue = error.Value;
                    if (serverValue == null) // 409 doesn't return the server item
                    {
                        serverValue = await operation.Table.LookupAsync(localItem.Id) as JObject;
                    }

                    var serverItem = serverValue.ToObject<ToDoItem>();

                    if (serverItem.Complete == localItem.Complete &&
                        serverItem.Text == localItem.Text)
                    {
                        // items are same so we can ignore the conflict
                        return serverValue;
                    }

                    int command = await ShowConflictDialog(localItem, serverValue);

                    if (command == 1)
                    {
                        // Overwrite the server version and try the operation again by continuing the loop
                        operation.Item[MobileServiceSystemColumns.Version] = serverValue[MobileServiceSystemColumns.Version];
                        if (error is MobileServiceConflictException) // change operation from Insert to Update
                        {
                            tryOperation = async () => await operation.Table.UpdateAsync(operation.Item) as JObject;
                        }
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

        private async Task<int> ShowConflictDialog(ToDoItem localItem, JObject serverValue)
        {
            var dialog = new UIAlertView("Conflict between local and server versions",
                    "How do you want to resolve this conflict?\n\n" + "Local item: \n" + localItem +
                    "\n\nServer item:\n" + serverValue.ToObject<ToDoItem>(), null, "Cancel", LOCAL_VERSION, SERVER_VERSION);

            var clickTask = new TaskCompletionSource<int>();
            dialog.Clicked += (sender, e) =>
            {
                clickTask.SetResult(e.ButtonIndex);
            };

            dialog.Show();

            return await clickTask.Task;
        }
    }
}