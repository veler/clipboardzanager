package com.etiennebaudoux.clipboardzanager.mocks;

import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.AuthenticationResult;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudAuthentication;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.StringUtils;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks.Task;

/**
 * Created by ebaud on 2/11/2017.
 */

public class CloudAuthenticationMock implements CloudAuthentication {
    @Override
    public Task<AuthenticationResult> authenticateAsync(String authenticationUri, String redirectUri) {
        return new Task<AuthenticationResult>(() -> {
            if (StringUtils.isNullOrEmpty(authenticationUri))
            {
                return new AuthenticationResult(true, redirectUri);
            }

            return new AuthenticationResult(false, redirectUri);
        });
    }
}
