package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers;

import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.R;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudTokenProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.CoreHelper;

/**
 * Provides a set of functions designed to provide tokens for a {@link DropBoxProvider}.
 */
public class DropBoxTokenProvider implements CloudTokenProvider {

    private static final String DropBoxAccessToken = "DropBoxAccessToken";

    @Override
    public String getToken(String tokenName) {
        switch (tokenName) {
            case "AppKey":
                return App.getContext().getString(R.string.DropBoxAppKey);

            case "AccessToken":
                return CoreHelper.getSetting(DropBoxAccessToken);

            default:
                return "";
        }
    }

    @Override
    public void setToken(String tokenName, String value) {
        switch (tokenName) {
            case "AccessToken":
                CoreHelper.setSetting(DropBoxAccessToken, value);
        }
    }
}
