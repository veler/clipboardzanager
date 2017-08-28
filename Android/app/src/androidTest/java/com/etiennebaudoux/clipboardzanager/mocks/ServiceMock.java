package com.etiennebaudoux.clipboardzanager.mocks;

import com.etiennebaudoux.clipboardzanager.componentmodel.services.Service;
import com.etiennebaudoux.clipboardzanager.componentmodel.services.ServiceSettingProvider;

public class ServiceMock implements Service {
    public boolean Reseted = true;

    @Override
    public void initialize(ServiceSettingProvider settingProvider) {
        Reseted = false;
    }

    @Override
    public void reset() {
        Reseted = true;
    }
}
