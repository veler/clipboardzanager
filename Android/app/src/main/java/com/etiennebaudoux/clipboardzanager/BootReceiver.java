package com.etiennebaudoux.clipboardzanager;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

/**
 * Class instantiated when Android booted and is ready. This allows us to start listening to the clipboard without requiring to start the application.
 */

public class BootReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        Intent newIntent = new Intent(context, BootService.class);
        context.startService(newIntent);
    }
}
