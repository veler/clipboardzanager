package com.etiennebaudoux.clipboardzanager.componentmodel.services;

import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudStorageProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;

/**
 * Provides a set of functions designed to provide application settings for a {@link Service}.
 */
public interface ServiceSettingProvider {
    /**
     * Returns the specified setting.
     *
     * @param settingName The setting's name to get.
     * @return The setting corresponding to the given name.
     */
    String getSetting(String settingName);

    /**
     * Returns the list of available cloud storage providers.
     *
     * @return The list of available cloud storage providers.
     */
    QueryableArrayList<CloudStorageProvider> getCloudStorageProviders();

    /**
     * Set the specified setting.
     *
     * @param settingName The setting's name to set.
     * @param value       The setting.
     */
    void setSetting(String settingName, String value);
}
