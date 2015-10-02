package com.example.zumoappname;

import java.net.MalformedURLException;

import android.app.Activity;
import android.app.AlertDialog;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.ProgressBar;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceList;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
import com.microsoft.windowsazure.notifications.NotificationsManager;

public class ToDoActivity extends Activity {

    public static final String SENDER_ID = "754190079999";

	/**
	 * Mobile Service Client reference
	 */
	public static MobileServiceClient mClient;

	/**
	 * Mobile Service Table used to access data
	 */
	private MobileServiceTable<ToDoItem> mToDoTable;

	/**
	 * Adapter to sync the items list with the view
	 */
	private ToDoItemAdapter mAdapter;

	/**
	 * EditText containing the "New ToDo" text
	 */
	private EditText mTextNewToDo;

	/**
	 * Progress spinner to use for table operations
	 */
	private ProgressBar mProgressBar;

	/**
	 * Initializes the activity
	 */
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_to_do);
		
		mProgressBar = (ProgressBar) findViewById(R.id.loadingProgressBar);

		// Initialize the progress bar
		mProgressBar.setVisibility(ProgressBar.GONE);
		
		try {
			// Create the Mobile Service Client instance, using the provided
			// Mobile Service URL and key
			mClient = new MobileServiceClient(
                    "MobileServiceUrl",
                    "AppKey",
					this).withFilter(new ProgressFilter());

            NotificationsManager.handleNotifications(this, SENDER_ID, MyHandler.class);

			// Get the Mobile Service Table instance to use
			mToDoTable = mClient.getTable(ToDoItem.class);

			mTextNewToDo = (EditText) findViewById(R.id.textNewToDo);

			// Create an adapter to bind the items with the view
			mAdapter = new ToDoItemAdapter(this, R.layout.row_list_to_do);
			ListView listViewToDo = (ListView) findViewById(R.id.listViewToDo);
			listViewToDo.setAdapter(mAdapter);
		
			// Load the items from the Mobile Service
			refreshItemsFromTable();

		} catch (MalformedURLException e) {
			createAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
		}
	}
	
	/**
	 * Initializes the activity menu
	 */
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_main, menu);
		return true;
	}
	
	/**
	 * Select an option from the menu
	 */
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		if (item.getItemId() == R.id.menu_refresh) {
			refreshItemsFromTable();
		}
		
		return true;
	}

	/**
	 * Mark an item as completed
	 * 
	 * @param item
	 *            The item to mark
	 */
	public void checkItem(final ToDoItem item) {
		if (mClient == null) {
			return;
		}

		// Set the item as completed and update it in the table
		item.setComplete(true);
		
		new AsyncTask<Void, Void, Void>() {

            @Override
            protected Void doInBackground(Void... params) {
                try {
                    mToDoTable.update(item).get();
                    runOnUiThread(new Runnable() {
                        public void run() {
                            if (item.isComplete()) {
                                mAdapter.remove(item);
                            }
                            refreshItemsFromTable();
                        }
                    });
                } catch (Exception exception) {
                    createAndShowDialog(exception, "Error");
                }
                return null;
            }
        }.execute();
	}

	/**
	 * Add a new item
	 * 
	 * @param view
	 *            The view that originated the call
	 */
	public void addItem(View view) {
		if (mClient == null) {
			return;
		}

		// Create a new item
		final ToDoItem item = new ToDoItem();

		item.setText(mTextNewToDo.getText().toString());
		item.setComplete(false);
		
		// Insert the new item
        new AsyncTask<Void, Void, Void>() {

            @Override
            protected Void doInBackground(Void... params) {
                try {
                    mToDoTable.insert(item).get();
                    if (!item.isComplete()) {
                        runOnUiThread(new Runnable() {
                            public void run() {
                                mAdapter.add(item);
                            }
                        });
                    }
                } catch (Exception exception) {
                    createAndShowDialog(exception, "Error");
                }
                return null;
            }
        }.execute();

		mTextNewToDo.setText("");
	}

	/**
	 * Refresh the list with the items in the Mobile Service Table
	 */
	private void refreshItemsFromTable() {

		// Get the items that weren't marked as completed and add them in the
		// adapter
        new AsyncTask<Void, Void, Void>() {

            @Override
            protected Void doInBackground(Void... params) {
                try {
                    final MobileServiceList<ToDoItem> result = mToDoTable.where().field("complete").eq(false).execute().get();
                    runOnUiThread(new Runnable() {

                        @Override
                        public void run() {
                            mAdapter.clear();

                            for (ToDoItem item : result) {
                                mAdapter.add(item);
                            }
                        }
                    });
                } catch (Exception exception) {
                    createAndShowDialog(exception, "Error");
                }
                return null;
            }
        }.execute();
	}

	/**
	 * Creates a dialog and shows it
	 * 
	 * @param exception
	 *            The exception to show in the dialog
	 * @param title
	 *            The dialog title
	 */
	private void createAndShowDialog(Exception exception, String title) {
		Throwable ex = exception;
		if(exception.getCause() != null){
			ex = exception.getCause();
		}
		createAndShowDialog(ex.getMessage(), title);
	}

	/**
	 * Creates a dialog and shows it
	 * 
	 * @param message
	 *            The dialog message
	 * @param title
	 *            The dialog title
	 */
	private void createAndShowDialog(String message, String title) {
		AlertDialog.Builder builder = new AlertDialog.Builder(this);

		builder.setMessage(message);
		builder.setTitle(title);
		builder.create().show();
	}
	
	private class ProgressFilter implements ServiceFilter {

        @Override
        public ListenableFuture<ServiceFilterResponse> handleRequest(
                ServiceFilterRequest request, NextServiceFilterCallback next) {

            runOnUiThread(new Runnable() {

                @Override
                public void run() {
                    if (mProgressBar != null) mProgressBar.setVisibility(ProgressBar.VISIBLE);
                }
            });

            SettableFuture<ServiceFilterResponse> result = SettableFuture.create();
            try {
                ServiceFilterResponse response = next.onNext(request).get();
                result.set(response);
            } catch (Exception exc) {
                result.setException(exc);
            }

          dismissProgressBar();
          return result;
        }

      private void dismissProgressBar() {
          runOnUiThread(new Runnable() {

              @Override
              public void run() {
                  if (mProgressBar != null) mProgressBar.setVisibility(ProgressBar.GONE);
              }
          });
        }
    }
}
