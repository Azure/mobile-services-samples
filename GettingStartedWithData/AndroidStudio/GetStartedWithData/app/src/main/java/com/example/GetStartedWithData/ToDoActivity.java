package com.example.GetStartedWithData;

// TODO uncomment these lines when using Mobile Services
//import java.net.MalformedURLException;
//import com.google.common.util.concurrent.ListenableFuture;
//import com.google.common.util.concurrent.SettableFuture;
//import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
//import com.microsoft.windowsazure.mobileservices.MobileServiceList;
//import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
//import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
//import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
//import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
//import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
//import android.os.AsyncTask;

import android.app.Activity;
import android.app.AlertDialog;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.ProgressBar;

//TODO comment out these lines
import java.util.ArrayList;
import java.util.List;

public class ToDoActivity extends Activity {

//	TODO Uncomment these lines to create references to the mobile service client and table
//	private MobileServiceClient mClient;
//	private MobileServiceTable<ToDoItem> mToDoTable;

//  TODO Comment out this line to remove the in-memory store
	public List<ToDoItem> toDoItemList = new ArrayList<ToDoItem>();
	
	private ToDoItemAdapter mAdapter;
	private EditText mTextNewToDo;
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

//		TODO Uncomment the the following code to create the mobile services client
//		try {
//			// Create the Mobile Service Client instance, using the provided
//			// Mobile Service URL and key
//			mClient = new MobileServiceClient(
//					"MobileServiceUrl",
//					"AppKey", 
//					this).withFilter(new ProgressFilter());

			// Get the Mobile Service Table instance to use
//			mToDoTable = mClient.getTable(ToDoItem.class);
//		} catch (MalformedURLException e) {
//			createAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
//		}
		
		mTextNewToDo = (EditText) findViewById(R.id.textNewToDo);

		// Create an adapter to bind the items with the view
		mAdapter = new ToDoItemAdapter(this, R.layout.row_list_to_do);
		ListView listViewToDo = (ListView) findViewById(R.id.listViewToDo);
		listViewToDo.setAdapter(mAdapter);
	
		// Load the items from the Mobile Service
		refreshItemsFromTable();
		
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
		
		// Set the item as completed and update it in the table
		item.setComplete(true);

//		TODO Uncomment the the following code when using a mobile service
//	    new AsyncTask<Void, Void, Void>() {
//
//	        @Override
//	        protected Void doInBackground(Void... params) {
//	            try {
//	                mToDoTable.update(item).get();
//	                runOnUiThread(new Runnable() {
//	                    public void run() {
//	                        if (item.isComplete()) {
//	                            mAdapter.remove(item);
//	                        }
//	                        refreshItemsFromTable();
//	                    }
//	                });
//	            } catch (Exception exception) {
//	                createAndShowDialog(exception, "Error");
//	            }
//	            return null;
//	        }
//	    }.execute();

//		TODO Comment out these lines to remove the in-memory store
		toDoItemList.add(item);
		if (item.isComplete()) {
			mAdapter.remove(item);
		}
//		End of lines to comment out
		
	}

	/**
	 * Add a new item
	 * 
	 * @param view
	 *            The view that originated the call
	 */
	public void addItem(View view) {
		
		// Create a new item
		final ToDoItem item = new ToDoItem();

		item.setText(mTextNewToDo.getText().toString());
		item.setComplete(false);

//		TODO Uncomment the the following code when using a mobile service
//		// Insert the new item
//		new AsyncTask<Void, Void, Void>() {
//
//	        @Override
//	        protected Void doInBackground(Void... params) {
//	            try {
//	                mToDoTable.insert(item).get();
//	                if (!item.isComplete()) {
//	                    runOnUiThread(new Runnable() {
//	                        public void run() {
//	                            mAdapter.add(item);
//	                        }
//	                    });
//	                }
//	            } catch (Exception exception) {
//	                createAndShowDialog(exception, "Error");
//	            }
//	            return null;
//	        }
//	    }.execute();

//	    TODO Comment out these lines to remove the in-memory store
		toDoItemList.add(item);
		mAdapter.add(item);
//		End of lines to comment out
		
		mTextNewToDo.setText("");
	}

	/** 
	 * Refresh the list with the items in the Mobile Service Table
	 */
	private void refreshItemsFromTable() {

//		TODO Uncomment the the following code when using a mobile service
//		// Get the items that weren't marked as completed and add them in the adapter
//	    new AsyncTask<Void, Void, Void>() {
//
//	        @Override
//	        protected Void doInBackground(Void... params) {
//	            try {
//	                final MobileServiceList<ToDoItem> result = mToDoTable.where().field("complete").eq(false).execute().get();
//	                runOnUiThread(new Runnable() {
//
//	                    @Override
//	                    public void run() {
//	                        mAdapter.clear();
//
//	                        for (ToDoItem item : result) {
//	                            mAdapter.add(item);
//	                        }
//	                    }
//	                });
//	            } catch (Exception exception) {
//	                createAndShowDialog(exception, "Error");
//	            }
//	            return null;
//	        }
//	    }.execute();
		
//		TODO Comment out these lines to remove the in-memory store	
		mAdapter.clear();
		for (ToDoItem item : toDoItemList) 
		{
			if (item.isComplete() == false)
				mAdapter.add(item);
		}		
//		End of lines to comment out

		
		
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
		createAndShowDialog(exception.toString(), title);
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
	

//	TODO Uncomment the the following code when using a mobile service
//	private class ProgressFilter implements ServiceFilter {
//
//        @Override
//        public ListenableFuture<ServiceFilterResponse> handleRequest(
//                ServiceFilterRequest request, NextServiceFilterCallback next) {
//
//            runOnUiThread(new Runnable() {
//
//                @Override
//                public void run() {
//                    if (mProgressBar != null) mProgressBar.setVisibility(ProgressBar.VISIBLE);
//                }
//            });
//
//            SettableFuture<ServiceFilterResponse> result = SettableFuture.create();
//            try {
//                ServiceFilterResponse response = next.onNext(request).get();
//                result.set(response);
//            } catch (Exception exc) {
//                result.setException(exc);
//            }
//
//          dismissProgressBar();
//          return result;
//        }
//
//      private void dismissProgressBar() {
//          runOnUiThread(new Runnable() {
//
//              @Override
//              public void run() {
//                  if (mProgressBar != null) mProgressBar.setVisibility(ProgressBar.GONE);
//              }
//          });
//        }
//    }
}
