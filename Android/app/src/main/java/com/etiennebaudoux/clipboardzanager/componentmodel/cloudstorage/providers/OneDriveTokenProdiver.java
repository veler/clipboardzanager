package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers;

import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.R;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudTokenProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.CoreHelper;

/**
 * Provides a set of functions designed to provide tokens for a {@link OneDriveProvider}.
 */

public class OneDriveTokenProdiver implements CloudTokenProvider {

    private static final String OneDriveRefreshToken = "OneDriveRefreshToken";

    @Override
    public String getToken(String tokenName) {
        switch (tokenName)
        {
            case "ClientID":
                return App.getContext().getString(R.string.OneDriveClientId);

            case "RefreshToken":
                return CoreHelper.getSetting(OneDriveRefreshToken);

            default:
                return "";
        }
    }

    @Override
    public void setToken(String tokenName, String value) {
        switch (tokenName) {
            case "RefreshToken":
                CoreHelper.setSetting(OneDriveRefreshToken, value);
        }
    }
}
