package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks.Task;

/**
 * Provides a set of functions designed to manage the authentication to a cloud service.
 */

public interface CloudAuthentication {
    /**
     * Try to authenticate the user and wait for authentication completed.
     *
     * @param authenticationUri The authentication page Uri.
     * @param redirectUri       The expected Uri that we must detect after the authentication. It must be different from the authenticationUri.
     * @return Returns a {@link AuthenticationResult} that describes the result of the authentication.
     */
    Task<AuthenticationResult> authenticateAsync(String authenticationUri, String redirectUri);
}
