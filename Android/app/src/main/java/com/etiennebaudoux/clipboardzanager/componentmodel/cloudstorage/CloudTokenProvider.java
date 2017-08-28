package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage;

/**
 * Provides a set of functions designed to provide tokens for a CloudTokenProvider.
 */
public interface CloudTokenProvider {
    /**
     * Returns the specified token.
     *
     * @param tokenName The token's name to get.
     * @return The token corresponding to the given name.
     */
    String getToken(String tokenName);

    /**
     * Set the specified token.
     *
     * @param tokenName The token's name to set.
     * @param value     The token.
     */
    void setToken(String tokenName, String value);
}
