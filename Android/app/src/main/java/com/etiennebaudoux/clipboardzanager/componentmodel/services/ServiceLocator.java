package com.etiennebaudoux.clipboardzanager.componentmodel.services;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.Requires;

/**
 * Provides a set of functions designed to manage the services.
 */
public final class ServiceLocator {
    //region Fields

    private final static QueryableArrayList<Service> _services = new QueryableArrayList<>();

    //endregion

    //region Properties

    private static ServiceSettingProvider _settingProvider = null;

    /**
     * Gets the provider of settings.
     *
     * @return
     */
    public static ServiceSettingProvider getSettingProvider() {
        return _settingProvider;
    }

    /**
     * Sets the provider of settings.
     *
     * @param value
     */
    public static void setSettingProvider(ServiceSettingProvider value) {
        _settingProvider = value;
    }

    //endregion

    //region Methods

    /**
     * Get an instance of the specified service.
     *
     * @param type The service type to get.
     * @param <T>  The service type to get.
     * @return The current instance of the service.
     */
    public static <T extends Service> T getService(Class<T> type) {
        Requires.notNull(getSettingProvider(), "settingProvider");

        T service = null;
        QueryableArrayList<T> serviceFound = _services.ofType(type);

        if (serviceFound.any()) {
            service = serviceFound.single();
        } else {
            try {
                service = type.newInstance();
                Requires.notNull(service, "service");

                _services.add(service);
                service.initialize(getSettingProvider());
            } catch (InstantiationException | IllegalAccessException e) {
                e.printStackTrace();
            }
        }

        return service;
    }

    /**
     * Reset the state of all services. This method must be used in the unit test.
     */
    public static void resetAll() {
        for (Service service : _services) {
            service.reset();
        }
    }

    //endregion
}
