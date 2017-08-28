package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import android.content.Context;
import android.content.SharedPreferences;

import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.BuildConfig;

/**
 * Provides a set of methods used to get information about the application.
 */

public final class CoreHelper {
    //region Fields

    private static SharedPreferences _settings;

    //endregion

    //region Properties

    private static boolean _isUnitTesting;

    /**
     * Gets a value that defines whether a unit test is in progress.
     * @return a value that defines whether a unit test is in progress.
     */
    public static boolean isUnitTesting() {
        return _isUnitTesting;
    }

    /**
     * Defines that the current app is under unit test.
     */
    public static void setIsUnitTestingTrue() {
        _isUnitTesting = true;
    }

    //endregion

    //region Constructors

    static {
        Context context = App.getContext();

        if (context != null) {
            _settings = context.getSharedPreferences("com.etiennebaudoux.clipboardzanager", Context.MODE_PRIVATE);
        }
    }

    //endregion

    //region Methods

    /**
     * Retrieves an application's setting's value.
     *
     * @param settingName The name of the setting.
     * @return Returns null if the setting does not exist.
     */
    public static String getSetting(String settingName) {
        return _settings.getString(settingName, null);
    }

    /**
     * Set an application's setting
     *
     * @param settingName The name of the setting.
     * @param value       The value of the setting.
     */
    public static void setSetting(String settingName, String value) {
        _settings.edit().putString(settingName, value).apply();
    }

    /**
     * Returns the version of the executable
     *
     * @return A {@link String} that corresponds to the version of the application.
     */
    public static String getApplicationVersion() {
        return BuildConfig.VERSION_NAME;
    }

    /**
     * Gets application name.
     *
     * @return The application name.
     */
    public static String getApplicationName() {
        return BuildConfig.APPLICATION_ID;
    }

    //endregion
}
