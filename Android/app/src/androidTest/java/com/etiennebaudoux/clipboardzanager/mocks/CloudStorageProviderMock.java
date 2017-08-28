package com.etiennebaudoux.clipboardzanager.mocks;

import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudFile;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudFolder;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudStorageProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudTokenProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.StringUtils;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.Event;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.EventArgs;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks.Task;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotAuthenticatedException;

import java.io.FileNotFoundException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Date;

public class CloudStorageProviderMock implements CloudStorageProvider {

    private boolean _isAuthenticated;

    @Override
    public String getCloudServiceName() {
        return "MockCloudStorageProvider";
    }

    @Override
    public boolean isAuthenticated() {
        return _isAuthenticated;
    }

    @Override
    public boolean credentialExists() {
        String token = getTokenProvider().getToken("MyToken");
        return !StringUtils.isNullOrEmpty(token);
    }

    @Override
    public CloudTokenProvider getTokenProvider() {
        return new CloudTokenProviderMock();
    }

    @Override
    public Event<EventArgs> getCallbackOnResumeAfterAuthentication() {
        return null;
    }

    @Override
    public Task<Boolean> tryAuthenticateAsync() {
        return new Task<>(() -> {
            if (isAuthenticated()) {
                signOutAsync().await();
            }

            _isAuthenticated = false;
            return isAuthenticated();
        });
    }

    @Override
    public Task<Boolean> tryAuthenticateWithUiAsync(Boolean isResuming) {
        return new Task<>(() -> {
            if (isAuthenticated()) {
                signOutAsync().await();
            }

            _isAuthenticated = isResuming; //!authentication.authenticateAsync("http://authenticationUri", "http://exceptedUi").await().isCanceled();
            return isAuthenticated();
        });
    }

    @Override
    public Task<Void> signOutAsync() {
        return new Task<>(() -> {
            _isAuthenticated = false;
            getTokenProvider().setToken("MyToken", "");
            return null;
        });
    }

    @Override
    public Task<String> getUserNameAsync() {
        thowIfNotConnected();

        return new Task<>(() -> "John DOE");
    }

    @Override
    public Task<String> getUserIdAsync() {
        thowIfNotConnected();

        return new Task<>(() -> "{1df5a5312fa35f3ae1df35e1869}");
    }

    @Override
    public Task<CloudFolder> getAppFolderAsync() {
        return new Task<>(() -> {
            thowIfNotConnected();

            CloudFolder result = new CloudFolder();
            result.setFullPath("/path/MyApp");
            result.setName("MyApp");
            result.setSize(0);
            return result;
        });
    }

    @Override
    public Task<CloudFile> uploadFileAsync(InputStream baseStream, String remotePath) {
        return new Task<>(() -> {
            thowIfNotConnected();

            if (baseStream == null) {
                throw new NullPointerException();
            }

            CloudFile result = new CloudFile();
            result.setFullPath(remotePath);
            result.setName("myFile.txt");

            Date currentDate = new Date(System.currentTimeMillis());
            result.setLastModificationUtcDate(currentDate);
            return result;
        });
    }

    @Override
    public Task<Void> downloadFileAsync(String remotePath, OutputStream targetStream) {
        return new Task<>(() -> {
            thowIfNotConnected();

            if (targetStream == null) {
                throw new NullPointerException();
            }

            if (remotePath.equals("/path/MyApp/myFile.txt")) {
                throw new FileNotFoundException();
            }

            byte[] data = new byte[]{0, 1, 2, 3, 4};
            targetStream.write(data, 0, data.length);

            return null;
        });
    }

    @Override
    public Task<Void> deleteFileAsync(String remotePath) {
        return new Task<>(() -> {
            if (remotePath.equals("/path/MyApp/myFile.txt")) {
                throw new FileNotFoundException();
            }

            return null;
        });
    }

    private void thowIfNotConnected() {
        if (!isAuthenticated()) {
            throw new NotAuthenticatedException();
        }
    }
}
