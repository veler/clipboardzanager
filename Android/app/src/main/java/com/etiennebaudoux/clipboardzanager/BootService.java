package com.etiennebaudoux.clipboardzanager;

import android.app.Service;
import android.content.Intent;
import android.os.IBinder;
import android.support.annotation.Nullable;

import com.etiennebaudoux.clipboardzanager.componentmodel.services.ClipboardService;
import com.etiennebaudoux.clipboardzanager.componentmodel.services.DataService;
import com.etiennebaudoux.clipboardzanager.componentmodel.services.ServiceLocator;

/**
 * Basic service started with Android.
 */

public class BootService extends Service {
    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public void onCreate() {
        super.onCreate();

        ServiceLocator.getService(DataService.class);
        ServiceLocator.getService(ClipboardService.class);
    }
}
