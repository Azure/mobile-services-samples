package com.example.blog20140807;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;

public class EditToDoActivity extends Activity {

	protected static final String ITEM_TEXT_KEY = "com.example.blog20140807.item_text";
	protected static final String ITEM_COMPLETE_KEY = "com.example.blog20140807.item_complete";

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_edit_to_do);

		Bundle extras = getIntent().getExtras();
		String itemText = extras.getString(ITEM_TEXT_KEY);
		boolean itemComplete = extras.getBoolean(ITEM_COMPLETE_KEY);

		final EditText itemTextBox = (EditText)findViewById(R.id.textBoxEditItem);
		itemTextBox.setText(itemText);
		final CheckBox completeCheckbox = (CheckBox)findViewById(R.id.checkBoxItemComplete);
		completeCheckbox.setChecked(itemComplete);
		
		Button btnDone = (Button)findViewById(R.id.buttonDoneEditing);
		btnDone.setOnClickListener(new View.OnClickListener() {

			@Override
			public void onClick(View v) {
				Intent i = new Intent();
				i.putExtra(ITEM_TEXT_KEY, itemTextBox.getText().toString());
				i.putExtra(ITEM_COMPLETE_KEY, completeCheckbox.isChecked());
				setResult(RESULT_OK, i);
				finish();
			}
		});
	}
}
