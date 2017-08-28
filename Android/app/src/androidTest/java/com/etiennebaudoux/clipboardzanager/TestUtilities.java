package com.etiennebaudoux.clipboardzanager;

import com.etiennebaudoux.clipboardzanager.componentmodel.services.ServiceLocator;
import com.etiennebaudoux.clipboardzanager.mocks.ServiceSettingProviderMock;

public final class TestUtilities {
    private static boolean _initialized;

    public static void initialize() {
        if (!_initialized) {
            _initialized = true;

            ServiceLocator.setSettingProvider(new ServiceSettingProviderMock());
        }

        getSettingProvider().resetSettings();
        ServiceLocator.resetAll();
    }

    public static ServiceSettingProviderMock getSettingProvider() {
        if (ServiceLocator.getSettingProvider() == null || !(ServiceLocator.getSettingProvider() instanceof ServiceSettingProviderMock)) {
            ServiceLocator.setSettingProvider(new ServiceSettingProviderMock());
        }

        return (ServiceSettingProviderMock) ServiceLocator.getSettingProvider();
    }
}
