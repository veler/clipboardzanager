package com.etiennebaudoux.clipboardzanager.mocks;

import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudTokenProvider;

/**
 * Created by ebaud on 2/11/2017.
 */

public class CloudTokenProviderMock implements CloudTokenProvider {
    @Override
    public String getToken(String tokenName) {
        if (tokenName.equals("MyToken"))
        {
            return "123456789abc";
        }

        throw new RuntimeException();
    }

    @Override
    public void setToken(String tokenName, String value) {
        if (!tokenName.equals("MyToken"))
        {
            throw new RuntimeException();
        }
    }
}
