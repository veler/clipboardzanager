package com.etiennebaudoux.clipboardzanager.componentmodel.services;

import com.android.internal.util.Predicate;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudStorageProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;

/**
 * Provides a set of functions designed to work with the cloud storage providers.
 */
public class CloudStorageService implements Service {
    //region Fields

    private QueryableArrayList<CloudStorageProvider> _cloudStorageProvider;
    private ServiceSettingProvider _settingProvider;

    //endregion

    //region Properties

    //region IsLinkedToAService

    /**
     * Gets a {@link Boolean} that defines if the software is connected to at least one cloud storage service.
     *
     * @return A {@link Boolean} that defines if the software is connected to at least one cloud storage service.
     */
    public boolean isLinkedToAService() {
        return _cloudStorageProvider.any(new Predicate<CloudStorageProvider>() {
                                             @Override
                                             public boolean apply(CloudStorageProvider cloudStorageProvider) {
                                                 return cloudStorageProvider.credentialExists();
                                             }
                                         }
        );
    }

    //endregion

    //endregion

    //region Methods

    @Override
    public void initialize(ServiceSettingProvider settingProvider) {
        _settingProvider = settingProvider;
        _cloudStorageProvider = new QueryableArrayList<>();

        QueryableArrayList<CloudStorageProvider> providers = settingProvider.getCloudStorageProviders();

        for (CloudStorageProvider provider : providers) {
            _cloudStorageProvider.add(provider);
        }
    }

    @Override
    public void reset() {

    }

    //endregion
}
