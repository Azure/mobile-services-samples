package com.example.todolist_auth_rs;

// authentication with 1.5 SDK
// works
//

import java.net.MalformedURLException;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.atomic.AtomicBoolean;

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

import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceList;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;

public class ToDoActivity extends Activity {

	public static final String SHAREDPREFFILE = "temp"; 
	public static final String USERIDPREF = "uid";  
	public static final String TOKENPREF = "tkn";   

	public boolean bAuthenticating = false;
	public final Object mAuthenticationLock = new Object();
	
	/**
	 * Mobile Service Client reference
	 */
	private MobileServiceClient mClient;

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
					this)	
						.withFilter(new ProgressFilter())
            			.withFilter(new RefreshTokenCacheFilter());

			authenticate(false);
			
		} catch (MalformedURLException e) {
			createAndShowDialog(new Exception("Error creating the Mobile Service. Verify the URL"), "Error");
		}
	}

	/**
	 * Authenticates with the desired login provider. Also caches the token. 
	 * 
	 * If a local token cache is detected, the token cache is used instead of an actual 
	 * login unless bRefresh is set to true forcing a refresh.
	 * 
	 * @param bRefreshCache
	 *            Indicates whether to force a token refresh. 
	 */
	private void authenticate(boolean bRefreshCache) {

	    bAuthenticating = true;

	    if (bRefreshCache || !loadUserTokenCache(mClient))
	    {
	        // New login using the provider and update the token cache.
				ListenableFuture<MobileServiceUser> mLogin = mClient.login(MobileServiceAuthenticationProvider.Google);
				synchronized(mAuthenticationLock)
				{
			    	Futures.addCallback(mLogin, new FutureCallback<MobileServiceUser>() {
			    		@Override
			    		public void onFailure(Throwable exc) {
			    			createAndShowDialog( exc.getMessage(), "Login Error");
			                bAuthenticating = false;
			                mAuthenticationLock.notifyAll();

			    		}   		
			    		@Override
			    		public void onSuccess(MobileServiceUser user) {
			    			createAndShowDialog(String.format(
			                        "You are now logged in - %1$2s",
			                        user.getUserId()), "Success");
			    			cacheUserToken(mClient.getCurrentUser());
			    			createTable();
			                bAuthenticating = false;
			                mAuthenticationLock.notifyAll();

			    		}
			    	});
				}
		    
    }
    else
    {
        // Other threads may be blocked waiting to be notified when 
        // authentication is complete.
        synchronized(mAuthenticationLock)
        {
            bAuthenticating = false;
            mAuthenticationLock.notifyAll();
        }
        createTable();
	    }
	}

	private void cacheUserToken(MobileServiceUser user)
	{
	    SharedPreferences prefs = getSharedPreferences(SHAREDPREFFILE, Context.MODE_PRIVATE);
	    Editor editor = prefs.edit();
	    editor.putString(USERIDPREF, user.getUserId());
	    editor.putString(TOKENPREF, user.getAuthenticationToken());
	    editor.commit();
	}

	private boolean loadUserTokenCache(MobileServiceClient client)
	{
	    SharedPreferences prefs = getSharedPreferences(SHAREDPREFFILE, Context.MODE_PRIVATE);
	    String userId = prefs.getString(USERIDPREF, "undefined"); 
	    if (userId == "undefined")
	        return false;
	    String token = prefs.getString(TOKENPREF, "undefined"); 
	    if (token == "undefined")
	        return false;

	    MobileServiceUser user = new MobileServiceUser(userId);
	    user.setAuthenticationToken(token);
	    client.setCurrentUser(user);

	    return true;
	}
	
	/**
	 * Detects if authentication is in progress and waits for it to complete. 
	 * Returns true if authentication was detected as in progress. False otherwise.
	 */
	public boolean detectAndWaitForAuthentication()
	{
	    boolean detected = false;
	    synchronized(mAuthenticationLock)
	    {
	        do
	        {
	            if (bAuthenticating == true)
	                detected = true;
	            try
	            {
	                mAuthenticationLock.wait(1000);
	            }
	            catch(InterruptedException e)
	            {}
	        }
	        while(bAuthenticating == true);
	    }
	    if (bAuthenticating == true)
	        return true;

	    return detected;
	}
	
	/**
	 * Waits for authentication to complete then adds or updates the token 
	 * in the X-ZUMO-AUTH request header.
	 * 
	 * @param request
	 *            The request that receives the updated token.
	 */
	private void waitAndUpdateRequestToken(ServiceFilterRequest request)
	{
	    MobileServiceUser user = null;
	    if (detectAndWaitForAuthentication())
	    {
	        user = mClient.getCurrentUser();
	        if (user != null)
	        {
	            request.removeHeader("X-ZUMO-AUTH");
	            request.addHeader("X-ZUMO-AUTH", user.getAuthenticationToken());
	        }
	    }
	}
	
	
	private void createTable() {

	    // Get the Mobile Service Table instance to use
	    mToDoTable = mClient.getTable(ToDoItem.class);

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
	/**
	* The RefreshTokenCacheFilter class filters responses for HTTP status code 401. 
	 * When 401 is encountered, the filter calls the authenticate method on the 
	 * UI thread. Out going requests and retries are blocked during authentication. 
	 * Once authentication is complete, the token cache is updated and 
	 * any blocked request will receive the X-ZUMO-AUTH header added or updated to 
	 * that request.   
	 */
	private class RefreshTokenCacheFilter implements ServiceFilter {
	 
        AtomicBoolean mAtomicAuthenticatingFlag = new AtomicBoolean();                     
        
        @Override
        public ListenableFuture<ServiceFilterResponse> handleRequest(
        		final ServiceFilterRequest request, 
        		final NextServiceFilterCallback nextServiceFilterCallback
        		)
        {
            // In this example, if authentication is already in progress we block the request
            // until authentication is complete to avoid unnecessary authentications as 
            // a result of HTTP status code 401. 
            // If authentication was detected, add the token to the request.
            waitAndUpdateRequestToken(request);
 
            // Send the request down the filter chain
            // retrying up to 5 times on 401 response codes.
            ListenableFuture<ServiceFilterResponse> future = null;
            ServiceFilterResponse response = null;
            int responseCode = 401;
            for (int i = 0; (i < 5 ) && (responseCode == 401); i++)
            {
                future = nextServiceFilterCallback.onNext(request);
                try {
                    response = future.get();
                    responseCode = response.getStatus().getStatusCode();
                } catch (InterruptedException e) {
                   e.printStackTrace();
                } catch (ExecutionException e) {
                    if (e.getCause().getClass() == MobileServiceException.class)
                    {
                        MobileServiceException mEx = (MobileServiceException) e.getCause();
                        responseCode = mEx.getResponse().getStatus().getStatusCode();
                        if (responseCode == 401)
                        {
                            // Two simultaneous requests from independent threads could get HTTP status 401. 
                            // Protecting against that right here so multiple authentication requests are
                            // not setup to run on the UI thread.
                            // We only want to authenticate once. Requests should just wait and retry 
                            // with the new token.
                            if (mAtomicAuthenticatingFlag.compareAndSet(false, true))                                                                                                      
                            {
                                // Authenticate on UI thread
                                runOnUiThread(new Runnable() {
                                    @Override
                                    public void run() {
                                        // Force a token refresh during authentication.
                                        authenticate(true);
//                                        ToDoActivity.mMainActivity.authenticate(true);

                                    }
                                });
                            }

			                // Wait for authentication to complete then update the token in the request. 
			                waitAndUpdateRequestToken(request);
			                mAtomicAuthenticatingFlag.set(false);                                                  
                        }
                    }
                }
            }
            return future;
        }
	}
	
	
	
//	/**
//	 * The RefreshTokenCacheFilter class filters responses for HTTP status code 401. 
//	 * When 401 is encountered, the filter calls the authenticate method on the 
//	 * UI thread. Out going requests and retries are blocked during authentication. 
//	 * Once authentication is complete, the token cache is updated and 
//	 * any blocked request will receive the X-ZUMO-AUTH header added or updated to 
//	 * that request.   
//	 */
//	private class RefreshTokenCacheFilter implements ServiceFilter {
//
//	    AtomicBoolean mAtomicAuthenticatingFlag = new AtomicBoolean();
//
//	    public ListenableFuture<ServiceFilterResponse> handleRequest(
//	            final ServiceFilterRequest request, 
//	            final NextServiceFilterCallback nextServiceFilterCallback) {
//
//	        // In this example, if authentication is already in progress we block the request
//	        // until authentication is complete to avoid unnecessary authentications as 
//	        // a result of HTTP status code 401. 
//	        // If authentication was detected, add the token to the request.
//	        waitAndUpdateRequestToken(request);
//
//	        // Wrap the request in a callback object that will facilitate retries.
//	        AuthenticationRetryFilterCallback retryCallbackObject = 
//		            new AuthenticationRetryFilterCallback(request, nextServiceFilterCallback); 
//
//		        // Send the request down the filter chain.
//	        ListenableFuture<ServiceFilterResponse> result = nextServiceFilterCallback.onNext(request);
//	        
//	        Futures.addCallback(result, new FutureCallback<ServiceFilterResponse>() {
//	            @Override
//	            public void onFailure(Throwable exc) {
//	                exc.printStackTrace();
//	            }
//
//	            @Override
//	            public void onSuccess(ServiceFilterResponse response) {
//		            // Filter out the 401 responses to update the token cache and 
//		            // retry the request
//		            if ((response != null) && (response.getStatus().getStatusCode() == 401))
//		            { 
//		                // Two simultaneous requests from independent threads could get HTTP 
//		                // status 401. Protecting against that right here so multiple 
//		                // authentication requests are not setup to run on the UI thread.
//		                // We only want to authenticate once. Other requests should just wait 
//		                // and retry with the new token.
//		                if (mAtomicAuthenticatingFlag.compareAndSet(false, true))                           
//		                {
//		                    // Authenticate on UI thread
//		                    runOnUiThread(new Runnable() {
//		                        @Override
//		                        public void run() {
//		                            // Force a token refresh during authentication.
//		                            authenticate(true);
//		                        }
//		                    });
//		                }
//
//		                // Wait for authentication to complete then 
//		                // update the token in the request.
//		                waitAndUpdateRequestToken(this.request);
//		                mAtomicAuthenticatingFlag.set(false);                       
//
//		                // Retry recursively with a new token as long as we get a 401.
//		                mNextServiceFilterCallback.onNext(this.mRequest, this);
//		            }
//
//		            // Responses that do not have 401 status codes just pass through.
//		            else if (this.mResponseCallback != null)  
//		              mResponseCallback.onResponse(response, exception);
//		        }
//
//	            }
//
//	        		);
//
//	        return result;
//
//
//	    
//	    /**
//	     * The AuthenticationRetryFilterCallback class is a wrapper around the response 
//	     * callback that encapsulates the request and other information needed to enable 
//	     * a retry of the request when HTTP status code 401 is encountered. 
//	     */
//	    private class AuthenticationRetryFilterCallback implements ServiceFilterResponseCallback
//	    {
//	        // Data members used to retry the request during the response.
//	        ServiceFilterRequest mRequest;
//	        NextServiceFilterCallback mNextServiceFilterCallback;
//	        ServiceFilterResponseCallback mResponseCallback;
//
//	        public AuthenticationRetryFilterCallback(ServiceFilterRequest request, 
//	                NextServiceFilterCallback nextServiceFilterCallback, 
//	                ServiceFilterResponseCallback responseCallback)
//	        {
//	            mRequest = request;
//	            mNextServiceFilterCallback = nextServiceFilterCallback;
//	            mResponseCallback = responseCallback;
//	        }
//
//	        @Override
//	        public void onResponse(ServiceFilterResponse response, Exception exception) {
//
//	            // Filter out the 401 responses to update the token cache and 
//	            // retry the request
//	            if ((response != null) && (response.getStatus().getStatusCode() == 401))
//	            { 
//	                // Two simultaneous requests from independent threads could get HTTP 
//	                // status 401. Protecting against that right here so multiple 
//	                // authentication requests are not setup to run on the UI thread.
//	                // We only want to authenticate once. Other requests should just wait 
//	                // and retry with the new token.
//	                if (mAtomicAuthenticatingFlag.compareAndSet(false, true))                           
//	                {
//	                    // Authenticate on UI thread
//	                    runOnUiThread(new Runnable() {
//	                        @Override
//	                        public void run() {
//	                            // Force a token refresh during authentication.
//	                            authenticate(true);
//	                        }
//	                    });
//	                }
//
//	                // Wait for authentication to complete then 
//	                // update the token in the request.
//	                waitAndUpdateRequestToken(this.mRequest);
//	                mAtomicAuthenticatingFlag.set(false);                       
//
//	                // Retry recursively with a new token as long as we get a 401.
//	                mNextServiceFilterCallback.onNext(this.mRequest, this);
//	            }
//
//	            // Responses that do not have 401 status codes just pass through.
//	            else if (this.mResponseCallback != null)  
//	              mResponseCallback.onResponse(response, exception);
//	        }
//	    }
//
//	    @Override
//	    public void handleRequest(final ServiceFilterRequest request, 
//	        final NextServiceFilterCallback nextServiceFilterCallback,
//	        ServiceFilterResponseCallback responseCallback) {
//
//	        // In this example, if authentication is already in progress we block the request
//	        // until authentication is complete to avoid unnecessary authentications as 
//	        // a result of HTTP status code 401. 
//	        // If authentication was detected, add the token to the request.
//	        waitAndUpdateRequestToken(request);
//
//	        // Wrap the request in a callback object that will facilitate retries.
//	        AuthenticationRetryFilterCallback retryCallbackObject = 
//	            new AuthenticationRetryFilterCallback(request, nextServiceFilterCallback,
//	                  responseCallback); 
//
//	        // Send the request down the filter chain.
//	        nextServiceFilterCallback.onNext(request, retryCallbackObject);         
//	    }
//	}
	
	public void corruptToken()	
	{
	   MobileServiceUser user = mClient.getCurrentUser();
	   String token = user.getAuthenticationToken();
	   
	   byte[] buffer = token.getBytes();
	   buffer[0] += 1;
	   token = new String(buffer);
	   
	   user.setAuthenticationToken(token);
	   mClient.setCurrentUser(user);       
	}

	public void corruptTokenCache(View view)
	{
	   MobileServiceUser user = new MobileServiceUser(mClient.getCurrentUser().getUserId());
	   String token = mClient.getCurrentUser().getAuthenticationToken();
	   
	   byte[] buffer = token.getBytes();
	   buffer[0] += 1;
	   token = new String(buffer);
	   
	   user.setAuthenticationToken(token);
	   cacheUserToken(user);       
	}
	
	
}
