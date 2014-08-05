package com.example.blog20140807;

import java.net.MalformedURLException;
import java.util.HashMap;
import java.util.Map;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.ProgressBar;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceList;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncContext;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.SQLiteLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.SimpleSyncHandler;

public class ToDoActivity extends Activity {

	/**
	 * Mobile Service Client reference
	 */
	private MobileServiceClient mClient;

	/**
	 * Mobile Service Table used to access data
	 */
	private MobileServiceSyncTable<ToDoItem> mToDoTable;

	/**
	 * The query used to pull data from the remote server
	 */
	private Query mPullQuery;

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
	 * The position of the item which is being edited
	 */
	private int mEditedItemPosition = -1;

	private static final int EDIT_ACTIVITY_REQUEST_CODE = 1234;

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
					"https://blog20140807.azure-mobile.net/",
					"VQdzkFyLODXjByoRgXuMJdIxoZupwA43",
					this).withFilter(new ProgressFilter());

			// Saves the query which will be used for pulling data
			mPullQuery = mClient.getTable(ToDoItem.class).where().field("complete").eq(false);

			SQLiteLocalStore localStore = new SQLiteLocalStore(mClient.getContext(), "ToDoItem", null, 1);
			SimpleSyncHandler handler = new SimpleSyncHandler();
			MobileServiceSyncContext syncContext = mClient.getSyncContext();

			Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
			tableDefinition.put("id", ColumnDataType.String);
			tableDefinition.put("text", ColumnDataType.String);
			tableDefinition.put("complete", ColumnDataType.Boolean);
			tableDefinition.put("__version", ColumnDataType.String);

			localStore.defineTable("ToDoItem", tableDefinition);
			syncContext.initialize(localStore, handler).get();

			// Get the Mobile Service Table instance to use
			mToDoTable = mClient.getSyncTable(ToDoItem.class);

			mTextNewToDo = (EditText) findViewById(R.id.textNewToDo);

			// Create an adapter to bind the items with the view
			mAdapter = new ToDoItemAdapter(this, R.layout.row_list_to_do);
			final ListView listViewToDo = (ListView) findViewById(R.id.listViewToDo);
			listViewToDo.setAdapter(mAdapter);

			listViewToDo.setOnItemClickListener(new OnItemClickListener() {

				@Override
				public void onItemClick(AdapterView<?> parent, View view,
						int position, long id) {
					Intent i = new Intent(getApplicationContext(), EditToDoActivity.class);
					mEditedItemPosition = position;
					ToDoItem item = mAdapter.getItem(position);
					i.putExtra(EditToDoActivity.ITEM_COMPLETE_KEY, item.isComplete());
					i.putExtra(EditToDoActivity.ITEM_TEXT_KEY, item.getText());
					startActivityForResult(i, EDIT_ACTIVITY_REQUEST_CODE);
				}
			});

			// Load the items from the Mobile Service
			refreshItemsFromTable();

		} catch (MalformedURLException e) {
			createAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
		} catch (Exception e) {
			Throwable t = e;
			while (t.getCause() != null) {
				t = t.getCause();
			}
			createAndShowDialog(new Exception("Unknown error: " + t.getMessage()), "Error");
		}
	}

	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent intent) {
		if (requestCode == EDIT_ACTIVITY_REQUEST_CODE && resultCode == RESULT_OK && mEditedItemPosition >= 0) {
			ToDoItem item = mAdapter.getItem(mEditedItemPosition);
			String text = intent.getExtras().getString(EditToDoActivity.ITEM_TEXT_KEY);
			boolean complete = intent.getExtras().getBoolean(EditToDoActivity.ITEM_COMPLETE_KEY);

			if (!item.getText().equals(text) || item.isComplete() != complete) {
				item.setText(text);
				item.setComplete(complete);
				updateItem(item);
			}
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
			new AsyncTask<Void, Void, Void>() {

				@Override
				protected Void doInBackground(Void... params) {
					try {
						mClient.getSyncContext().push().get();
						mToDoTable.pull(mPullQuery).get();
						refreshItemsFromTable();
					} catch (Exception exception) {
						createAndShowDialog(exception, "Error");
					}
					return null;
				}

			}.execute();
			refreshItemsFromTable();
		}
		
		return true;
	}

	/**
	 * Updates an item as completed
	 * 
	 * @param item
	 *            The item to mark
	 */
	private void updateItem(final ToDoItem item) {
		if (mClient == null) {
			return;
		}

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
					final ToDoItem entity = mToDoTable.insert(item).get();
					if (!entity.isComplete()) {
						runOnUiThread(new Runnable() {
							public void run() {
								mAdapter.add(entity);
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
					final MobileServiceList<ToDoItem> result = mToDoTable.read(mPullQuery).get();
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

			ListenableFuture<ServiceFilterResponse> result = next.onNext(request);

			Futures.addCallback(result, new FutureCallback<ServiceFilterResponse>() {
				@Override
				public void onFailure(Throwable exc) {
					dismissProgressBar();
				}
				
				@Override
				public void onSuccess(ServiceFilterResponse resp) {
					dismissProgressBar();
				}

				private void dismissProgressBar() {
					runOnUiThread(new Runnable() {

						@Override
						public void run() {
							if (mProgressBar != null) mProgressBar.setVisibility(ProgressBar.GONE);
						}
					});
				}
			});

			return result;
		}
	}
}
