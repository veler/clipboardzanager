package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers;

import com.dropbox.core.DbxDownloader;
import com.dropbox.core.DbxRequestConfig;
import com.dropbox.core.android.Auth;
import com.dropbox.core.http.HttpRequestor;
import com.dropbox.core.http.StandardHttpRequestor;
import com.dropbox.core.v2.DbxClientV2;
import com.dropbox.core.v2.files.FileMetadata;
import com.dropbox.core.v2.files.ListFolderResult;
import com.dropbox.core.v2.files.Metadata;
import com.dropbox.core.v2.files.UploadBuilder;
import com.dropbox.core.v2.files.UploadUploader;
import com.dropbox.core.v2.files.WriteMode;
import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudFile;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudFolder;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudStorageProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudTokenProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.CoreHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.Requires;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.SecurityHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.StringUtils;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.Event;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.EventArgs;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks.Task;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotAuthenticatedException;
import com.etiennebaudoux.clipboardzanager.componentmodel.io.IoUtils;

import java.io.InputStream;
import java.io.OutputStream;
import java.util.concurrent.Callable;
import java.util.concurrent.TimeUnit;

/**
 * Provides a set of properties and methods designed to let the user connects to his DropBox account.
 */
public class DropBoxProvider implements CloudStorageProvider {
    //region Fields

    private String _appKey;
    private DbxClientV2 _client;
    private Event<EventArgs> _onResumeAfterAuthentication;

    //endregion

    //region Properties

    //region CloudServiceName

    @Override
    public String getCloudServiceName() {
        return "DropBox";
    }

    //endregion

    //region IsAuthenticated

    private boolean _isAuthenticated;

    @Override
    public boolean isAuthenticated() {
        return _isAuthenticated;
    }

    private void setIsAuthenticated(boolean value) {
        _isAuthenticated = value;
    }

    //endregion

    //region CredentialExists

    @Override
    public boolean credentialExists() {
        return !StringUtils.isEmptyOrWhiteSpace(getAccessToken());
    }

    //endregion

    //region CloudTokenProvider

    private CloudTokenProvider _tokenProvider;

    @Override
    public CloudTokenProvider getTokenProvider() {
        return _tokenProvider;
    }

    private void setTokenProvider(CloudTokenProvider value) {
        _tokenProvider = value;
    }

    //endregion

    //region AuthenticationWithUiInProgress

    private boolean _authenticationWithUiInProgress;

    public boolean isAuthenticationWithUiInProgress() {
        return _authenticationWithUiInProgress;
    }

    private void setAuthenticationWithUiInProgress(boolean value) {
        _authenticationWithUiInProgress = value;
    }

    //endregion

    //endregion

    //region Events

    @Override
    public Event<EventArgs> getCallbackOnResumeAfterAuthentication() {
        if (_onResumeAfterAuthentication == null) {
            _onResumeAfterAuthentication = new Event<>();
        }

        return _onResumeAfterAuthentication;
    }

    //endregion

    //region Constructors

    /**
     * Initialize a new instance of the {@link DropBoxProvider} class.
     *
     * @param tokenProvider The token provider linked to this cloud storage provider.
     */
    public DropBoxProvider(CloudTokenProvider tokenProvider) {
        Requires.notNull(tokenProvider, "tokenProvider");
        setTokenProvider(tokenProvider);
        _appKey = SecurityHelper.decryptString(getTokenProvider().getToken("AppKey"));
    }

    //endregion

    //region Methods

    @Override
    public Task<Boolean> tryAuthenticateAsync() {
        return new Task<>(new Callable<Boolean>() {
            @Override
            public Boolean call() throws Exception {
                String accessToken = getAccessToken();
                if (StringUtils.isEmptyOrWhiteSpace(accessToken)) {
                    setIsAuthenticated(false);
                    return false;
                }

                try {
                    DbxRequestConfig config = getRequestConfig();

                    _client = new DbxClientV2(config, SecurityHelper.decryptString(accessToken));
                    _client.users().getCurrentAccount();
                    setIsAuthenticated(true);
                } catch (Exception exception) {
                    setIsAuthenticated(false);
                    exception.printStackTrace();
                }
                return isAuthenticated();
            }
        });
    }

    @Override
    public Task<Boolean> tryAuthenticateWithUiAsync(Boolean isResuming) {
        return new Task<>(new Callable<Boolean>() {
            @Override
            public Boolean call() throws Exception {
                if (!isResuming && !isAuthenticationWithUiInProgress()) {
                    setAuthenticationWithUiInProgress(true);
                    signOutAsync().await();
                    Auth.startOAuth2Authentication(App.getContext(), _appKey);
                    return false;
                }

                if (!isAuthenticationWithUiInProgress()) {
                    return isAuthenticated(); // probably a wrong call after resuming the app.
                }

                setAuthenticationWithUiInProgress(false);
                String accessToken = Auth.getOAuth2Token(); //generate Access Token
                if (accessToken != null) {
                    getTokenProvider().setToken("AccessToken", SecurityHelper.encryptString(accessToken));
                    return tryAuthenticateAsync().await();
                }

                setIsAuthenticated(false);
                return isAuthenticated();
            }
        });
    }

    @Override
    public Task<Void> signOutAsync() {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                _client.auth().tokenRevoke();
                getTokenProvider().setToken("AccessToken", "");
                setIsAuthenticated(false);
                return null;
            }
        });
    }

    @Override
    public Task<String> getUserNameAsync() {
        return new Task<>(new Callable<String>() {
            @Override
            public String call() throws Exception {
                throwIfNotConnected();
                return _client.users().getCurrentAccount().getName().getDisplayName();
            }
        });
    }

    @Override
    public Task<String> getUserIdAsync() {
        return new Task<>(new Callable<String>() {
            @Override
            public String call() throws Exception {
                throwIfNotConnected();
                return _client.users().getCurrentAccount().getAccountId();
            }
        });
    }

    @Override
    public Task<CloudFolder> getAppFolderAsync() {
        return new Task<>(new Callable<CloudFolder>() {
            @Override
            public CloudFolder call() throws Exception {
                throwIfNotConnected();

                ListFolderResult listFolder = null;
                QueryableArrayList<CloudFile> files = new QueryableArrayList<>();

                do {
                    if (listFolder == null) {
                        listFolder = _client.files().listFolder("");
                    } else {
                        listFolder = _client.files().listFolderContinue(listFolder.getCursor());
                    }

                    for (Metadata entry : listFolder.getEntries()) {
                        if (entry instanceof FileMetadata) {
                            CloudFile file = new CloudFile();
                            file.setName(entry.getName());
                            file.setFullPath(entry.getPathLower());
                            file.setLastModificationUtcDate(((FileMetadata) entry).getServerModified());
                            files.add(file);
                        }
                    }
                } while (listFolder.getHasMore());

                CloudFolder result = new CloudFolder();
                result.setName("ClipboardZanager");
                result.setSize(0);
                result.setFullPath("/");
                result.setFiles(files);
                return result;
            }
        });
    }

    @Override
    public Task<CloudFile> uploadFileAsync(InputStream baseStream, String remotePath) {
        return new Task<>(new Callable<CloudFile>() {
            @Override
            public CloudFile call() throws Exception {
                throwIfNotConnected();

                UploadBuilder builder = _client.files().uploadBuilder(remotePath);
                builder.withMode(WriteMode.OVERWRITE);

                try (UploadUploader uploader = builder.start()) {
                    FileMetadata fileInfo = uploader.uploadAndFinish(baseStream);

                    CloudFile result = new CloudFile();
                    result.setName(fileInfo.getName());
                    result.setFullPath(fileInfo.getPathDisplay());
                    result.setLastModificationUtcDate(fileInfo.getServerModified());
                    return result;
                }
            }
        });
    }

    @Override
    public Task<Void> downloadFileAsync(String remotePath, OutputStream targetStream) {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                throwIfNotConnected();

                try (DbxDownloader response = _client.files().download(remotePath);
                     InputStream stream = response.getInputStream()) {
                    IoUtils.copy(stream, targetStream);
                }
                return null;
            }
        });
    }

    @Override
    public Task<Void> deleteFileAsync(String remotePath) {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                throwIfNotConnected();
                _client.files().permanentlyDelete(remotePath);
                return null;
            }
        });
    }

    /**
     * Retrieves the refresh token for the DropBox account.
     *
     * @return A {@link String} that represents the token
     */

    private String getAccessToken() {
        return getTokenProvider().getToken("AccessToken");
    }

    /**
     * Generates a new request configuration for the DropBox client.
     *
     * @return a {@link DbxRequestConfig}
     */
    private DbxRequestConfig getRequestConfig() {
        StandardHttpRequestor.Config.Builder configBuilder = StandardHttpRequestor.Config.builder();
        configBuilder.withConnectTimeout(20, TimeUnit.MINUTES);
        configBuilder.withReadTimeout(20, TimeUnit.MINUTES);
        HttpRequestor httpClient = new StandardHttpRequestor(configBuilder.build());

        DbxRequestConfig.Builder config = DbxRequestConfig.newBuilder(CoreHelper.getApplicationName());
        config.withHttpRequestor(httpClient);
        return config.build();
    }

    /**
     * Throw an exception if the user is not authenticated.
     */
    private void throwIfNotConnected() {
        if (!isAuthenticated()) {
            throw new NotAuthenticatedException();
        }
    }

    //endregion

}
