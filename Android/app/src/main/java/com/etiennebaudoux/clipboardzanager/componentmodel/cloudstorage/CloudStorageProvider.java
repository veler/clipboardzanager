package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.Event;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.EventArgs;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks.Task;

import java.io.InputStream;
import java.io.OutputStream;

/**
 * Provides a set of properties and methods used to interact with a cloud storage service.
 */
public interface CloudStorageProvider {
    /**
     * Gets the name of the cloud service.
     *
     * @return the name of the cloud service.
     */
    String getCloudServiceName();

    /**
     * Gets a value that defines whether the user is authenticated.
     *
     * @return True if the user is authenticated.
     */
    boolean isAuthenticated();

    /**
     * Gets a value that defines whether credential exists for this provider in the application.
     *
     * @return True if credentials are registered.
     */
    boolean credentialExists();

    /**
     * Gets the {@link CloudTokenProvider} associated to this storage provider.
     *
     * @return the {@link CloudTokenProvider} associated to this storage provider.
     */
    CloudTokenProvider getTokenProvider();

    /**
     * Gets the method that must be executed when the application resume after a tentative of authentication with the UI.
     *
     * @return The method to run.
     */
    Event<EventArgs> getCallbackOnResumeAfterAuthentication();

    /**
     * Try to authenticate to the cloud service with the information in cache, without any UI.
     *
     * @return True is the user is authenticated.
     */
    Task<Boolean> tryAuthenticateAsync();

    /**
     * Try to authenticate to the cloud service without any data from the cache. Usually, the authentication need to access to display an authentication page in the {@link CloudAuthentication}.
     *
     * @param isResuming A {@link Boolean} that specify whether the app is resuming after the authentication has been done or canceled.
     * @return True is the user is authenticated.
     */
    Task<Boolean> tryAuthenticateWithUiAsync(Boolean isResuming);

    /**
     * Sign the user out.
     *
     * @return A {@link Task} representing the asynchronous operation.
     */
    Task<Void> signOutAsync();

    /**
     * Gets the display name of the user.
     *
     * @return The display name of the user.
     */
    Task<String> getUserNameAsync();

    /**
     * Gets the unique ID of the user.
     *
     * @return The ID of the user.
     */
    Task<String> getUserIdAsync();

    /**
     * Retrieves information about the application folder.
     *
     * @return A {@link CloudFolder} that contains information about the application folder.
     */
    Task<CloudFolder> getAppFolderAsync();

    /**
     * Upload a file to the specified remote path and overwrite if it already exists.
     *
     * @param baseStream The stream that contains the data to upload.
     * @param remotePath The destination path on the could service.
     * @return A {@link CloudFile} that contains information about the uploaded file.
     */
    Task<CloudFile> uploadFileAsync(InputStream baseStream, String remotePath);

    /**
     * Download a file to the specified local path and overwrite if it already exists.
     *
     * @param remotePath   The remote file to download.
     * @param targetStream The stream where the data will be saved.
     * @return A {@link Task} representing the asynchronous operation.
     */
    Task<Void> downloadFileAsync(String remotePath, OutputStream targetStream);

    /**
     * Delete a file on the server.
     *
     * @param remotePath The remote file to delete.
     * @return A {@link Task} representing the asynchronous operation.
     */
    Task<Void> deleteFileAsync(String remotePath);
}