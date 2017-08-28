package com.etiennebaudoux.clipboardzanager.componentmodel.services;

/**
 * Provides a set of functions and properties that represents a service.
 */
public interface Service {
    /**
     * Initialize the service.
     *
     * @param settingProvider An object that provides the access to the settings of the application.
     */
    void initialize(ServiceSettingProvider settingProvider);

    /**
     * Reset the state of the service. The service is considered as not initialized.
     */
    void reset();
}
