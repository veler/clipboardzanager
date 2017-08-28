package com.etiennebaudoux.clipboardzanager;

import android.app.Activity;
import android.app.Application;
import android.content.Context;
import android.content.Intent;

import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudStorageProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers.DropBoxProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers.DropBoxTokenProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers.OneDriveProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers.OneDriveTokenProdiver;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.CoreHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;
import com.etiennebaudoux.clipboardzanager.componentmodel.services.ServiceLocator;
import com.etiennebaudoux.clipboardzanager.componentmodel.services.ServiceSettingProvider;

public class App extends Application {
    private static Context _context;
    private static Activity _activity;

    @Override
    public void onCreate() {
        super.onCreate();
        _context = this;

        try {
            Class.forName("com.etiennebaudoux.clipboardzanager.mocks.ServiceMock");
            CoreHelper.setIsUnitTestingTrue();
        } catch (ClassNotFoundException e) {
        }

        if (!CoreHelper.isUnitTesting()) {
            //Temp bu required.
            CoreHelper.setSetting("MaxDataToKeep", "25");
            CoreHelper.setSetting("DateExpireLimit", "30");
            CoreHelper.setSetting("KeepDataAfterReboot", "true");

            ServiceLocator.setSettingProvider(new SettingProvider());

            Intent newIntent = new Intent(this, BootService.class);
            startService(newIntent);
        }
    }

    public static Context getContext() {
        return _context;
    }

    public static void setCurrentActivity(Activity activity) {
        _activity = activity;
    }

    public static Activity getCurrentActivity() {
        return _activity;
    }

    private class SettingProvider implements ServiceSettingProvider {
        @Override
        public String getSetting(String settingName) {
            return CoreHelper.getSetting(settingName);
        }

        @Override
        public QueryableArrayList<CloudStorageProvider> getCloudStorageProviders() {
            QueryableArrayList<CloudStorageProvider> result = new QueryableArrayList<>();
            result.add(new DropBoxProvider(new DropBoxTokenProvider()));
            result.add(new OneDriveProvider(new OneDriveTokenProdiver()));
            return result;
        }

        @Override
        public void setSetting(String settingName, String value) {
            CoreHelper.setSetting(settingName, value);
        }
    }
}
