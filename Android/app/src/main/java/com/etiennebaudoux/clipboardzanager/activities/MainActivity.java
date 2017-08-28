package com.etiennebaudoux.clipboardzanager.activities;

import android.os.Bundle;
import android.view.View;

import com.etiennebaudoux.clipboardzanager.R;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers.DropBoxProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers.OneDriveProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.EventArgs;
import com.etiennebaudoux.clipboardzanager.componentmodel.ui.BaseNavigationDrawerAppCompatActivity;

public class MainActivity extends BaseNavigationDrawerAppCompatActivity {

    private DropBoxProvider _dropBoxProvider;
    private OneDriveProvider _oneDriveProvider;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        //  _dropBoxProvider = new DropBoxProvider(new DropBoxTokenProvider());
        //  _dropBoxProvider.getCallbackOnResumeAfterAuthentication().addHandler(this::onDropBoxAuthenticationWithUi);

        //  if (_dropBoxProvider.tryAuthenticateAsync().await()) {
        //      TextView name = (TextView) findViewById(R.id.name_textView);
        //      name.setText(_dropBoxProvider.getUserNameAsync().await());

        //      _dropBoxProvider.uploadFileAsync(new ByteArrayInputStream(new byte[]{72, 101, 108, 108, 111}), "/test.txt").await();
        //  }

        //  _oneDriveProvider = new OneDriveProvider(new OneDriveTokenProdiver());
        //  _oneDriveProvider.getCallbackOnResumeAfterAuthentication().addHandler(this::onOneDriveAuthenticationWithUi);

        //  if (_oneDriveProvider.tryAuthenticateAsync().await()) {
        //      TextView name = (TextView) findViewById(R.id.name_textView);
        //      name.setText(_oneDriveProvider.getUserNameAsync().await());

        //      _oneDriveProvider.uploadFileAsync(new ByteArrayInputStream(new byte[]{72, 101, 108, 108, 111}), "/test.txt").await();
        //  }
    }

    @Override
    protected void onResume() {
        super.onResume();

        try {
            _dropBoxProvider.getCallbackOnResumeAfterAuthentication().invoke(_dropBoxProvider, EventArgs.Empty);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void signInDropBoxButtonOnClick(View view) {
        boolean result = _oneDriveProvider.tryAuthenticateWithUiAsync(false).await();
        // boolean result = _dropBoxProvider.tryAuthenticateWithUiAsync(false).await();
    }

    private void onDropBoxAuthenticationWithUi(Object sender, EventArgs args) {
        if (_dropBoxProvider.isAuthenticationWithUiInProgress()) {
            if (_dropBoxProvider.tryAuthenticateWithUiAsync(true).await()) {
                //       TextView name = (TextView) findViewById(R.id.name_textView);
                //       name.setText(_dropBoxProvider.getUserNameAsync().await());
            }
        }
    }

    private void onOneDriveAuthenticationWithUi(Object sender, EventArgs args) {
        if (_oneDriveProvider.isAuthenticationWithUiInProgress()) {
            if (_oneDriveProvider.tryAuthenticateWithUiAsync(true).await()) {
                //      TextView name = (TextView) findViewById(R.id.name_textView);
                //      name.setText(_oneDriveProvider.getUserNameAsync().await());
            }
        }
    }
}
