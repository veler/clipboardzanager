package com.etiennebaudoux.clipboardzanager.componentmodel.services;

import android.content.Context;

import com.android.internal.util.Predicate;
import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.R;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.Consts;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.DataHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.Requires;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.SecurityHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.StringUtils;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.SystemInfoHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.Event;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.events.EventArgs;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks.Task;
import com.etiennebaudoux.clipboardzanager.componentmodel.io.AesInputStream;
import com.etiennebaudoux.clipboardzanager.componentmodel.io.AesOutputStream;
import com.etiennebaudoux.clipboardzanager.enums.DataEntryStatus;
import com.etiennebaudoux.clipboardzanager.enums.ThumbnailDataType;
import com.etiennebaudoux.clipboardzanager.models.ClipboardData;
import com.etiennebaudoux.clipboardzanager.models.DataEntry;
import com.etiennebaudoux.clipboardzanager.models.DataEntryCache;
import com.etiennebaudoux.clipboardzanager.models.DataIdentifier;
import com.etiennebaudoux.clipboardzanager.models.Link;
import com.etiennebaudoux.clipboardzanager.models.Thumbnail;

import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.Serializable;
import java.security.NoSuchAlgorithmException;
import java.security.spec.InvalidKeySpecException;
import java.util.Date;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.Callable;
import java.util.concurrent.TimeUnit;
import java.util.regex.Pattern;

/**
 * Provides a service that to manage the clipboard entry on the hard drive.
 */
public class DataService implements Service {
    //region Fields

    private final Pattern _creditCardRegex = Pattern.compile("^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\\d{3})\\d{11})$");
    private final Pattern _hasNumber = Pattern.compile("^(?=.*\\d).+$");
    private final Pattern _hasUpperChar = Pattern.compile("^(?=.*[A-Z]).+$");
    private final Pattern _uriPattern = Pattern.compile("\\b(https?|ftp)://[-a-zA-Z0-9+&@#/%?=~_|!:,.;]*[-a-zA-Z0-9+&@#/%=~_|]");

    private boolean _lastCopiedDataWasCreditCard;
    private boolean _lastCopiedDataWasPassword;
    private String _detectedPasswordOrCreditCard;
    private String _dataEntryFilePassword;
    private ServiceSettingProvider _settingProvider;

    //endregion

    //region Properties

    //region DataEntries

    private QueryableArrayList<DataEntry> _dataEntries;

    public QueryableArrayList<DataEntry> getDataEntries() {
        return _dataEntries;
    }

    //endregion

    //region Cache

    private QueryableArrayList<DataEntryCache> _cache;

    public QueryableArrayList<DataEntryCache> getCache() {
        return _cache;
    }

    //endregion

    //endregion

    //region Events

    /**
     * Raised when a credit card number is detected.
     */
    public Event<EventArgs> CreditCardNumberDetected = new Event<>();

    /**
     * Raised when a credit card number is kept.
     */
    public Event<EventArgs> CreditCardNumberSaved = new Event<>();

    /**
     * Raised when a password is detected.
     */
    public Event<EventArgs> PasswordDetected = new Event<>();

    /**
     * Raised when a password is kept.
     */
    public Event<EventArgs> PasswordSaved = new Event<>();

    //endregion

    //region Methods

    @Override
    public void initialize(ServiceSettingProvider settingProvider) {
        _settingProvider = settingProvider;

        _dataEntries = new QueryableArrayList<>();
        _cache = new QueryableArrayList<>();

        _dataEntryFilePassword = SecurityHelper.encryptString(App.getContext().getString(R.string.DropBoxAppKey) + App.getContext().getString(R.string.OneDriveClientId));
    }

    @Override
    public void reset() {
        _lastCopiedDataWasCreditCard = false;
        _lastCopiedDataWasPassword = false;
        _detectedPasswordOrCreditCard = null;
    }

    /**
     * Determines whether the input string looks like a credit card number or not.
     *
     * @param input The string to test
     * @return Returns True is the string looks like a credit card number
     */
    public boolean isCreditCard(String input) {
        input = input.replaceAll("[^\\d]", "");
        return _creditCardRegex.matcher(input).matches();
    }

    /**
     * Determines whether the input string looks like a password or not.
     *
     * @param input The string to test
     * @return Returns True is the string looks like a password.
     */
    public boolean isPassword(String input) {
        boolean hasNumber = _hasNumber.matcher(input).matches();
        boolean hasUpperChar = _hasUpperChar.matcher(input).matches();
        boolean hasMinimum8Chars = input.length() >= 8;
        boolean hasMaximum32Chars = input.length() <= 32;
        boolean hasMaximum2Whitespaces = (input.length() - input.replace(" ", "").length()) <= 2;

        return hasNumber && hasUpperChar && hasMinimum8Chars && hasMaximum32Chars & hasMaximum2Whitespaces;
    }

    /**
     * One time on two, if the passed text is exactly equals to the last call to this method, returns true and raise event to notify that a credit card will be kept by the application.
     *
     * @param text The supposed credit card number.
     * @return True if the passed text is exactly equals to the last call to this method and that the application's settings defines that the software must avoid the credit card numbers.
     */
    public boolean keepOrIgnoreCreditCard(String text) {
        boolean ignored = false;

        text = SecurityHelper.encryptString(text);

        if (_lastCopiedDataWasCreditCard && _detectedPasswordOrCreditCard.equals(text)) {
            CreditCardNumberSaved.invoke(this, EventArgs.Empty);
        } else {
            if (Boolean.parseBoolean(_settingProvider.getSetting("AvoidCreditCard"))) {
                ignored = true;
                _lastCopiedDataWasCreditCard = true;
                _detectedPasswordOrCreditCard = text;
            }

            CreditCardNumberDetected.invoke(this, EventArgs.Empty);
        }

        return ignored;
    }

    /**
     * One time on two, if the passed text is exactly equals to the last call to this method, returns true and raise event to notify that a password will be kept by the application.
     *
     * @param text The supposed password.
     * @return True if the passed text is exactly equals to the last call to this method and that the application's settings defines that the software must avoid the passwords.
     */
    public boolean keepOrIgnorePassword(String text) {
        boolean ignored = false;

        text = SecurityHelper.encryptString(text);

        if (_lastCopiedDataWasPassword && _detectedPasswordOrCreditCard.equals(text)) {
            PasswordSaved.invoke(this, EventArgs.Empty);
        } else {
            if (Boolean.parseBoolean(_settingProvider.getSetting("AvoidPasswords"))) {
                ignored = true;
                _lastCopiedDataWasPassword = true;
                _detectedPasswordOrCreditCard = text;
            }

            PasswordDetected.invoke(this, EventArgs.Empty);
        }

        return ignored;
    }

    /**
     * Generates a list of {@link DataIdentifier} for the given formats.
     *
     * @return A list of {@link DataIdentifier}.
     */
    public QueryableArrayList<DataIdentifier> getDataIdentifiers() {
        QueryableArrayList<DataIdentifier> identifiers = new QueryableArrayList<>();

        DataIdentifier identifier = new DataIdentifier();
        identifier.setFormatName("Text");
        identifier.setIdentifier(generateNewUUID());
        identifiers.add(identifier);

        return identifiers;
    }

    /**
     * Sort the data. The favorites will be placed on top of the list.
     *
     * @param saveDataEntryFile Defines whether the data entry file must be saved
     * @return A {@link Task} representing the asynchronous operation.
     */
    public Task<Void> reorganizeAsync(boolean saveDataEntryFile) {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                int indexOfFirstNonFavorite = getDataEntries().indexOf(getDataEntries().firstOrDefault(
                        new Predicate<DataEntry>() {
                            @Override
                            public boolean apply(DataEntry dataEntry) {
                                return !dataEntry.isFavorite();
                            }
                        })
                );

                if (indexOfFirstNonFavorite > -1) {
                    for (int i = indexOfFirstNonFavorite; i < getDataEntries().size(); i++) {
                        DataEntry item = getDataEntries().get(i);
                        if (!item.isFavorite()) {
                            continue;
                        }

                        getDataEntries().remove(i);
                        getDataEntries().add(0, item);

                        DataEntryCache cacheItem = getCache().singleOrDefault(
                                new Predicate<DataEntryCache>() {
                                    @Override
                                    public boolean apply(DataEntryCache dataEntryCache) {
                                        return dataEntryCache.getIdentifier().equals(item.getIdentifier());
                                    }
                                }
                        );
                        if (cacheItem != null) {
                            getCache().remove(cacheItem);
                            getCache().add(0, cacheItem);
                        }
                    }
                }

                if (saveDataEntryFile) {
                    saveDataEntryFileAsync().await();
                }
                return null;
            }
        });
    }

    /**
     * Add the specific clipboard data to the data entries.
     *
     * @param data         The clipboard data.
     * @param identifiers  The list of identifiers for each data format.
     * @param isCreditCard Determines whether the data is a credit card number.
     * @param isPassword   Determines whether the data is a password.
     */
    public void addDataEntry(ClipboardData data, QueryableArrayList<DataIdentifier> identifiers, boolean isCreditCard, boolean isPassword) throws IOException, ClassNotFoundException {
        Requires.notNull(data, "data");
        Requires.notNull(identifiers, "identifiers");

        DataEntry entry = new DataEntry();
        entry.setIdentifier(generateNewUUID());
        entry.setThumbnail(generateThumbnail(data.getData(), isCreditCard, isPassword));
        entry.setDate(data.getDate());
        entry.setIsFavorite(false);
        entry.setCanSynchronize(true);
        entry.setIconIsFromWindowStore(false);
        entry.setDataIdentifiers(identifiers);

        DataEntryCache cache = new DataEntryCache();
        cache.setIdentifier(entry.getIdentifier());
        cache.setStatus(DataEntryStatus.ADDED);

        getDataEntries().add(0, entry);
        getCache().add(0, cache);

        entry = getDataEntries().get(0);
        if (entry.getThumbnail().getType() == ThumbnailDataType.LINK) {
            // We doing it here to avoid blocking the part that runs on the UI thread.
            Link value = DataHelper.fromBase64(entry.getThumbnail().getValue(), Link.class);
            value.setTitle(SystemInfoHelper.getWebPageTitle(value.getUri()).await());
            entry.getThumbnail().setValue(DataHelper.toBase64(value));
        }

        purgeCacheAsync().await();
    }

    /**
     * Remove a data from the data service. By default, the data entry file will be saved.
     *
     * @param identifier  The {@link UUID} that represents the data entry.
     * @param identifiers The list of {@link DataIdentifier} that represents the data.
     * @return A {@link Task} representing the asynchronous operation.
     */
    public Task<Void> removeDataAsync(UUID identifier, List<DataIdentifier> identifiers) {
        return removeDataAsync(identifier, identifiers, true);
    }

    /**
     * Remove a data from the data service.
     *
     * @param identifier        The {@link UUID} that represents the data entry.
     * @param identifiers       The list of {@link DataIdentifier} that represents the data.
     * @param saveDataEntryFile Defines whether the data entry file must be saved.
     * @return A {@link Task} representing the asynchronous operation.
     */
    public Task<Void> removeDataAsync(UUID identifier, List<DataIdentifier> identifiers, boolean saveDataEntryFile) {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                Requires.notNull(identifier, "identifier");
                Requires.notNull(identifiers, "identifiers");

                getDataEntries().remove(getDataEntries().single(
                        new Predicate<DataEntry>() {
                            @Override
                            public boolean apply(DataEntry dataEntry) {
                                return dataEntry.getIdentifier().equals(identifier);
                            }
                        }
                ));

                if (ServiceLocator.getService(CloudStorageService.class).isLinkedToAService()) {
                    getCache().single(
                            new Predicate<DataEntryCache>() {
                                @Override
                                public boolean apply(DataEntryCache dataEntryCache) {
                                    return dataEntryCache.getIdentifier().equals(identifier);
                                }
                            }
                    ).setStatus(DataEntryStatus.DELETED);
                } else {
                    getCache().remove(getCache().single(
                            new Predicate<DataEntryCache>() {
                                @Override
                                public boolean apply(DataEntryCache dataEntryCache) {
                                    return dataEntryCache.getIdentifier().equals(identifier);
                                }
                            }
                    ));
                }

                for (DataIdentifier dataIdentifier : identifiers) {
                    String dataFilePath = dataIdentifier.getIdentifier().toString() + ".dat";

                    if (App.getContext().getFileStreamPath(dataFilePath).exists()) {
                        App.getContext().deleteFile(dataFilePath);
                    }
                }

                if (saveDataEntryFile) {
                    saveDataEntryFileAsync().await();
                }
                return null;
            }
        });
    }

    /**
     * Remove all the data from the data service
     *
     * @return A {@link Task} representing the asynchronous operation.
     */
    public Task<Void> removeAllDataAsync() {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                getDataEntries().clear();

                for (DataEntryCache dataEntryCache : getCache()) {
                    dataEntryCache.setStatus(DataEntryStatus.DELETED);
                }

                clearCache();
                return null;
            }
        });
    }

    /**
     * Save the data entry to the internal storage.
     *
     * @return A {@link Task} representing the asynchronous operation.
     */
    private Task<Void> loadDataEntryFileAsync() {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                if (!Boolean.parseBoolean(_settingProvider.getSetting("KeepDataAfterReboot"))) {
                    clearCache();
                }

                if (App.getContext().getFileStreamPath(Consts.DataEntryFileName).exists()) {
                    try {
                        QueryableArrayList<DataEntry> entries;

                        try (FileInputStream fileStream = App.getContext().openFileInput(Consts.DataEntryFileName);
                             AesInputStream aesStream = new AesInputStream(fileStream, _dataEntryFilePassword, SecurityHelper.getSaltKeys(_dataEntryFilePassword).getEncoded())) {
                            byte[] data = new byte[aesStream.getLength()];
                            aesStream.read(data, 0, data.length);
                            entries = DataHelper.fromByteArray(data, new QueryableArrayList<DataEntry>().getClass());
                        }

                        getDataEntries().addAll(entries);
                    } catch (Exception ex) {
                        clearCache();
                    }
                }

                if (App.getContext().getFileStreamPath(Consts.CacheFileName).exists()) {
                    try {
                        QueryableArrayList<DataEntryCache> entries;

                        try (FileInputStream fileStream = App.getContext().openFileInput(Consts.CacheFileName);
                             AesInputStream aesStream = new AesInputStream(fileStream, _dataEntryFilePassword, SecurityHelper.getSaltKeys(_dataEntryFilePassword).getEncoded())) {
                            byte[] data = new byte[aesStream.getLength()];
                            aesStream.read(data, 0, data.length);
                            entries = DataHelper.fromByteArray(data, new QueryableArrayList<DataEntryCache>().getClass());
                        }

                        getCache().addAll(entries);
                    } catch (Exception ex) {
                        clearCache();
                    }
                }
                return null;
            }
        });
    }

    /**
     * Save the data entry to the internal storage.
     *
     * @return A {@link Task} representing the asynchronous operation.
     */
    private Task<Void> saveDataEntryFileAsync() {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                saveDataFile(Consts.DataEntryFileName, getDataEntries());

                saveDataFile(Consts.CacheFileName, getCache());
                return null;
            }
        });
    }

    /**
     * Encrypt and save the specified data on the internal storage.
     *
     * @param filePath   The full path to the file to save.
     * @param dataToSave The data to save.
     */
    private void saveDataFile(String filePath, Serializable dataToSave) throws IOException, InvalidKeySpecException, NoSuchAlgorithmException {
        if (App.getContext().getFileStreamPath(filePath).exists()) {
            App.getContext().deleteFile(filePath);
        }

        try (FileOutputStream fileStream = App.getContext().openFileOutput(filePath, Context.MODE_PRIVATE);
             AesOutputStream aesStream = new AesOutputStream(fileStream, _dataEntryFilePassword, SecurityHelper.getSaltKeys(_dataEntryFilePassword).getEncoded())) {
            byte[] data = DataHelper.toByteArray(dataToSave);
            aesStream.write(data, 0, data.length);
        }
    }

    /**
     * Clean the data by applying the limit count of data and the expire date.
     *
     * @return A {@link Task} representing the asynchronous operation.
     */
    private Task<Void> purgeCacheAsync() {
        return new Task<>(new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                QueryableArrayList<DataEntry> dataToRemove = new QueryableArrayList<>();
                int maxDataToKeep = Integer.parseInt(_settingProvider.getSetting("MaxDataToKeep"));
                long expireLimit = TimeUnit.DAYS.toMillis(Integer.parseInt(_settingProvider.getSetting("DateExpireLimit")));

                reorganizeAsync(false).await();

                Date now = new Date(System.currentTimeMillis());
                if (getDataEntries().size() > maxDataToKeep) {
                    dataToRemove = getDataEntries().skip(maxDataToKeep).where(
                            new Predicate<DataEntry>() {
                                @Override
                                public boolean apply(DataEntry dataEntry) {
                                    return !dataEntry.isFavorite();
                                }
                            }
                    );
                }

                dataToRemove = dataToRemove.union(getDataEntries().where(
                        new Predicate<DataEntry>() {
                            @Override
                            public boolean apply(DataEntry dataEntry) {
                                return (now.getTime() - dataEntry.getDate().getTime()) > expireLimit && !dataEntry.isFavorite();
                            }
                        }
                ));

                for (DataEntry data : dataToRemove) {
                    removeDataAsync(data.getIdentifier(), data.getDataIdentifiers(), false).await();
                }

                saveDataEntryFileAsync().await();
                return null;
            }
        });
    }

    /**
     * Remove all data from the software cache.
     */
    private void clearCache() {
        for (String file : App.getContext().fileList()) {
            if (!file.equals(Consts.CacheFileName)) {
                App.getContext().deleteFile(file);
            }
        }
    }

    /**
     * Generate a {@link Thumbnail} from the clipboard's data
     *
     * @param text         The clipboard data
     * @param isCreditCard Determines whether the data is a credit card number.
     * @param isPassword   Determines whether the data is a password.
     * @return A {@link Thumbnail} that represent a small part of the clipboard's data
     */
    private Thumbnail generateThumbnail(String text, boolean isCreditCard, boolean isPassword) throws IOException {
        @ThumbnailDataType int type = ThumbnailDataType.UNKNOWN;

        if (isCreditCard) {
            if (!StringUtils.isNullOrEmpty(text)) {
                text = text.replace("-", "");
                text = text.replace("_", "");
                text = text.replace(" ", "");
            }

            if (text.length() == 16) {
                text = text.substring(0, 4) + '-' + new String(new char[4]).replace("\0", Consts.PasswordMask) + '-' + new String(new char[4]).replace("\0", Consts.PasswordMask) + '-' + text.substring(12);
            } else {
                text = new String(new char[text.length()]).replace("\0", Consts.PasswordMask);
            }
        } else if (isPassword) {
            text = text.substring(0, 1) + new String(new char[text.length() - 2]).replace("\0", Consts.PasswordMask) + text.substring(text.length() - 1);
        } else if (text.length() > 253) {
            text = text.substring(0, Math.min(text.length(), 250));
            text += "...";
        }

        String value;
        boolean isUri = _uriPattern.matcher(text).matches();
        if (isUri) {
            type = ThumbnailDataType.LINK;
            Link link = new Link();
            link.setUri(text);
            value = DataHelper.toBase64(link);
        } else {
            type = ThumbnailDataType.STRING;
            value = DataHelper.toBase64(text);
        }

        Thumbnail thumbnail = new Thumbnail();
        thumbnail.setType(type);
        thumbnail.setValue(value);

        return thumbnail;
    }

    /**
     * Generate a new unique {@link UUID} not used by the service
     *
     * @return The new {@link UUID}
     */
    private UUID generateNewUUID() {
        UUID uuid;
        String uuidString;
        boolean match = false;

        do {
            uuid = UUID.randomUUID();
            uuidString = uuid.toString();

            final String uuidStringFinal = uuidString;
            match = getDataEntries().any(
                    new Predicate<DataEntry>() {
                        @Override
                        public boolean apply(DataEntry dataEntry) {
                            return dataEntry.getIdentifier().toString().equals(uuidStringFinal) ||
                                    dataEntry.getDataIdentifiers().any(new Predicate<DataIdentifier>() {
                                        @Override
                                        public boolean apply(DataIdentifier identifier) {
                                            return identifier.getIdentifier().toString().equals(uuidStringFinal);
                                        }
                                    });
                        }
                    }
            );
        }
        while (match);

        return uuid;
    }

    //endregion
}
