package com.etiennebaudoux.clipboardzanager.mocks;

import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudStorageProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;
import com.etiennebaudoux.clipboardzanager.componentmodel.services.ServiceSettingProvider;

public class ServiceSettingProviderMock implements ServiceSettingProvider {
    public String KeepDataAfterReboot;
    public String AvoidPasswords;
    public String AvoidCreditCard;
    public String MaxDataToKeep;
    public String DateExpireLimit;

    @Override
    public String getSetting(String settingName) {
        String value;
        switch (settingName) {
            case "DropBoxAppKey":
                value = "foo";
                break;

            case "OneDriveClientId":
                value = "bar";
                break;

            case "SynchronizationInterval":
                value = "10";
                break;

            case "KeepDataAfterReboot":
                value = KeepDataAfterReboot;
                break;

            case "AvoidPasswords":
                value = AvoidPasswords;
                break;

            case "AvoidCreditCard":
                value = AvoidCreditCard;
                break;

            case "MaxDataToKeep":
                value = MaxDataToKeep;
                break;

            case "DateExpireLimit":
                value = DateExpireLimit;
                break;

            default:
                throw new RuntimeException("Unable to find the setting " + settingName);
        }

        return value;
    }

    @Override
    public QueryableArrayList<CloudStorageProvider> getCloudStorageProviders() {
        QueryableArrayList<CloudStorageProvider> result = new QueryableArrayList<>();
        result.add(new CloudStorageProviderMock());
        return result;
    }

    @Override
    public void setSetting(String settingName, String value) {
        throw new RuntimeException("Not implemented");
    }

    public void resetSettings() {
        KeepDataAfterReboot = "true";
        AvoidPasswords = "true";
        AvoidCreditCard = "true";
        MaxDataToKeep = "25";
        DateExpireLimit = "30";
    }
}
