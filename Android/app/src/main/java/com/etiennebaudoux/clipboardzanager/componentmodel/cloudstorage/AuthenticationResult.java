package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage;

/**
 * Represents the result of a CloudAuthentication.
 */

public class AuthenticationResult {
    //region Properties

    //region IsCanceled

    private boolean _isCanceled;

    /**
     * Gets a value the defines whether the operation has been canceled or not.
     *
     * @return True if the authentication has been canceled.
     */
    public boolean isCanceled() {
        return _isCanceled;
    }

    //endregion

    //region RedirectedUri

    private String _redirectedUri;

    /**
     * Gets the Uri returned after an authentication tentative.
     *
     * @return The Uri returned after an authentication tentative.
     */
    public String getRedirectedUri() {
        return _redirectedUri;
    }

    //endregion

    //endregion

    //region Constructors

    /**
     * Initialize the instance of the {@link AuthenticationResult} class.
     *
     * @param isCanceled    Defines whether the operation has been canceled or not.
     * @param redirectedUri The Uri returned after an authentication tentative.
     */
    public AuthenticationResult(boolean isCanceled, String redirectedUri) {
        _isCanceled = isCanceled;
        _redirectedUri = redirectedUri;
    }

    //endregion
}
