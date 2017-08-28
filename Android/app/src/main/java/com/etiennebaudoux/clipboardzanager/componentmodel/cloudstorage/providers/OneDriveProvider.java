package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.providers;

import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudFile;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudFolder;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudStorageProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage.CloudTokenProvider;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.DefaultCallback;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.Requires;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.SecurityHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.StringUtils;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.Event;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.EventArgs;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks.Task;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotAuthenticatedException;
import com.etiennebaudoux.clipboardzanager.componentmodel.io.IoUtils;
import com.onedrive.sdk.authentication.MSAAuthenticator;
import com.onedrive.sdk.core.ClientException;
import com.onedrive.sdk.core.DefaultClientConfig;
import com.onedrive.sdk.core.IClientConfig;
import com.onedrive.sdk.extensions.IItemCollectionPage;
import com.onedrive.sdk.extensions.IItemRequestBuilder;
import com.onedrive.sdk.extensions.IOneDriveClient;
import com.onedrive.sdk.extensions.Item;
import com.onedrive.sdk.extensions.OneDriveClient;
import com.onedrive.sdk.logger.LoggerLevel;

import org.json.JSONObject;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.URL;
import java.util.concurrent.Callable;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;

/**
 * Provides a set of properties and methods designed to let the user connects to his OneDrive account.
 */

public class OneDriveProvider implements CloudStorageProvider {

    //region Fields

    private final String[] _scope = {"wl.signin", "wl.offline_access", "onedrive.appfolder", "onedrive.readwrite"};
    private String _clientId;
    private IOneDriveClient _client;
    private Event<EventArgs> _onResumeAfterAuthentication;

    //endregion

    //region Properties

    //region CloudServiceName

    @Override
    public String getCloudServiceName() {
        return "OneDrive";
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

    //region TokenProvider

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
     * Initialize a new instance of the {@link OneDriveProvider} class.
     *
     * @param tokenProvider The token provider linked to this cloud storage provider.
     */
    public OneDriveProvider(CloudTokenProvider tokenProvider) {
        Requires.notNull(tokenProvider, "tokenProvider");
        setTokenProvider(tokenProvider);
        _clientId = SecurityHelper.decryptString(getTokenProvider().getToken("ClientID"));
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

                _client = new OneDriveClient.Builder().fromConfig(getConfig()).loginAndBuildClient(App.getCurrentActivity());
                MSAAuthenticator authenticator = getAuthenticationProvider();
                authenticator.loginSilent().refresh();
                setIsAuthenticated(!authenticator.getAccountInfo().isExpired());
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

                    final DefaultCallback<IOneDriveClient> callback = new DefaultCallback<IOneDriveClient>() {
                        @Override
                        public void success(final IOneDriveClient result) {
                            _client = result;
                            getTokenProvider().setToken("RefreshToken", SecurityHelper.encryptString(getAccessToken()));
                            try {
                                getCallbackOnResumeAfterAuthentication().invoke(this, EventArgs.Empty);
                            } catch (Exception e) {
                                e.printStackTrace();
                            }
                        }

                        @Override
                        public void failure(final ClientException error) {
                            try {
                                getCallbackOnResumeAfterAuthentication().invoke(this, EventArgs.Empty);
                            } catch (Exception e) {
                                e.printStackTrace();
                            }
                            throw new RuntimeException(error);
                        }
                    };

                    signOutAsync().await();
                    new OneDriveClient.Builder().fromConfig(getConfig()).loginAndBuildClient(App.getCurrentActivity(), callback);
                    return false;
                }

                if (!isAuthenticationWithUiInProgress()) {
                    return isAuthenticated(); // probably a wrong call after resuming the app.
                }

                setAuthenticationWithUiInProgress(false);
                return tryAuthenticateAsync().await();
            }
        });
    }

    @Override
    public Task<Void> signOutAsync() {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                MSAAuthenticator authenticator = getAuthenticationProvider();
                if (authenticator != null) {
                    authenticator.logout();
                }

                getTokenProvider().setToken("RefreshToken", "");
                _client = null;
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
                JSONObject json = getUserInformation().await();

                if (json != null && json.has("name")) {
                    return json.getString("name");
                }

                return "";
            }
        });
    }

    @Override
    public Task<String> getUserIdAsync() {
        return new Task<>(new Callable<String>() {
            @Override
            public String call() throws Exception {
                throwIfNotConnected();
                JSONObject json = getUserInformation().await();

                if (json != null && json.has("id")) {
                    return json.getString("id");
                }

                return "";
            }
        });
    }

    @Override
    public Task<CloudFolder> getAppFolderAsync() {
        return new Task<>(new Callable<CloudFolder>() {
            @Override
            public CloudFolder call() throws Exception {
                throwIfNotConnected();

                Item folder = getAppFolderItem().buildRequest().get();
                String folderPath = "/drive/root:/Applications";

                if (folder.parentReference != null) {
                    folderPath = folder.parentReference.path;
                }

                IItemCollectionPage listFolder = null;
                QueryableArrayList<CloudFile> files = new QueryableArrayList<>();

                do {
                    if (listFolder == null) {
                        listFolder = _client.getDrive().getItems(folder.id).getChildren().buildRequest().get();
                    } else {
                        listFolder = listFolder.getNextPage().buildRequest().get();
                    }

                    for (Item entry : listFolder.getCurrentPage()) {
                        if (entry.file != null) {
                            CloudFile file = new CloudFile();
                            file.setName(entry.name);
                            file.setFullPath(entry.parentReference.path + "/" + entry.name);
                            file.setLastModificationUtcDate(entry.lastModifiedDateTime.getTime());
                            files.add(file);
                        }
                    }
                } while (listFolder.getNextPage() != null);

                CloudFolder result = new CloudFolder();
                result.setName(folder.name);
                result.setSize(folder.size);
                result.setFullPath(folderPath + "/" + folder.name + "/");
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

                try (ByteArrayOutputStream stream = new ByteArrayOutputStream()) {
                    IoUtils.copy(baseStream, stream);
                    Item fileInfo = _client.getDrive().getRoot().getItemWithPath(remotePath).getContent().buildRequest().put(stream.toByteArray());

                    CloudFile result = new CloudFile();
                    result.setName(fileInfo.name);
                    result.setFullPath(fileInfo.parentReference.path + "/" + fileInfo.name);
                    result.setLastModificationUtcDate(fileInfo.lastModifiedDateTime.getTime());
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

                try (InputStream stream = _client.getDrive().getRoot().getItemWithPath(remotePath).getContent().buildRequest().get()) {
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
                _client.getDrive().getRoot().getItemWithPath(remotePath).buildRequest().delete();
                return null;
            }
        });
    }

    /**
     * Retrieves the refresh token for the OneDrive account.
     *
     * @return A {@link String} that represents the token
     */
    private String getAccessToken() {
        MSAAuthenticator authenticator = getAuthenticationProvider();
        if (authenticator == null) {
            return getTokenProvider().getToken("RefreshToken");
        }

        return authenticator.getAccountInfo().getAccessToken();
    }

    /**
     * Gets the configuration for the authentication.
     *
     * @return A {@link IClientConfig}
     */
    private IClientConfig getConfig() {
        final MSAAuthenticator msaAuthenticator = new MSAAuthenticator() {
            @Override
            public String getClientId() {
                return _clientId;
            }

            @Override
            public String[] getScopes() {
                return _scope;
            }
        };

        final IClientConfig config = DefaultClientConfig.createWithAuthenticator(msaAuthenticator);
        config.getLogger().setLoggingLevel(LoggerLevel.Error);
        return config;
    }

    /**
     * Retrieves the {@link MSAAuthenticator} used to authenticate with a token.
     *
     * @return The {@link MSAAuthenticator}
     */
    private MSAAuthenticator getAuthenticationProvider() {
        if (_client == null) {
            return null;
        }

        return (MSAAuthenticator) _client.getAuthenticator();
    }

    /**
     * Retrieves the OneDrive application folder item.
     *
     * @return {@link IItemRequestBuilder}
     */
    private IItemRequestBuilder getAppFolderItem() {
        throwIfNotConnected();
        return _client.getDrive().getSpecial("approot");
    }

    /**
     * Retrieves a json that contains information about the user.
     *
     * @return A {@link JSONObject} that contains information about user.
     */
    private Task<JSONObject> getUserInformation() {
        return new Task<>(new Callable<JSONObject>() {
            @Override
            public JSONObject call() throws Exception {
                String accessToken = getAccessToken();
                URL uri = new URL("https://apis.live.net/v5.0/me?access_token=" + accessToken);

                OkHttpClient client = new OkHttpClient();
                Request request = new Request.Builder().url(uri).build();

                try (Response response = client.newCall(request).execute()) {
                    String jsonUserInfo = response.body().string();
                    if (!StringUtils.isEmptyOrWhiteSpace(jsonUserInfo)) {
                        return new JSONObject(jsonUserInfo);
                    }
                }

                return null;
            }
        });
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
