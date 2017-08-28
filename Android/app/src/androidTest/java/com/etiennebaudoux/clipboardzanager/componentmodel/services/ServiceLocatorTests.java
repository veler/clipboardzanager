package com.etiennebaudoux.clipboardzanager.componentmodel.services;

import com.etiennebaudoux.clipboardzanager.mocks.ServiceMock;
import com.etiennebaudoux.clipboardzanager.mocks.ServiceSettingProviderMock;

import org.junit.Test;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertSame;
import static org.junit.Assert.assertTrue;

public class ServiceLocatorTests {
    @Test
    public void serviceLocator() throws Exception {
        ServiceLocator.setSettingProvider(new ServiceSettingProviderMock());

        ServiceMock service1 = ServiceLocator.getService(ServiceMock.class);
        ServiceMock service2 = ServiceLocator.getService(ServiceMock.class);

        assertSame(service1, service2);
        assertFalse(service1.Reseted);

        ServiceLocator.resetAll();
        assertTrue(service1.Reseted);
    }
}