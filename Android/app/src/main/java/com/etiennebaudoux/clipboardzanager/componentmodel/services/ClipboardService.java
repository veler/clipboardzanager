package com.etiennebaudoux.clipboardzanager.componentmodel.services;

import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.widget.Toast;

import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.Consts;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.CoreHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.Pausable;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.Requires;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.SecurityHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.StringUtils;
import com.etiennebaudoux.clipboardzanager.componentmodel.io.AesOutputStream;
import com.etiennebaudoux.clipboardzanager.models.ClipboardData;
import com.etiennebaudoux.clipboardzanager.models.DataIdentifier;

import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.security.NoSuchAlgorithmException;
import java.security.spec.InvalidKeySpecException;
import java.util.Arrays;
import java.util.Date;

/**
 * Provides a service that can listen to the clipboard, read an write it.
 */
public class ClipboardService implements Service, Pausable {
    //region Fields

    private ClipboardManager _clipboardManager;
    private boolean _isPaused = true;
    private ServiceSettingProvider _settingProvider;

    //endregion

    //region Handled Methods

    private ClipboardManager.OnPrimaryClipChangedListener onClipboardChangedCallback = new ClipboardManager.OnPrimaryClipChangedListener() {
        @Override
        public void onPrimaryClipChanged() {
            Toast.makeText(App.getContext(), "ClipboardZanager received a data.", Toast.LENGTH_LONG).show();

            try {
                ClipData clipboardData = _clipboardManager.getPrimaryClip();
                Requires.notNull(clipboardData, "clipboardData");
                Requires.isTrue(clipboardData.getItemCount() == 1);
                ClipData.Item clipboardDataItem = clipboardData.getItemAt(0);

                onClipboardChanged(clipboardDataItem);
            } catch (InvalidKeySpecException | NoSuchAlgorithmException | IOException | ClassNotFoundException e) {
                e.printStackTrace();
            }
        }
    };

    public void onClipboardChanged(ClipData.Item clipboardDataItem) throws IOException, ClassNotFoundException, InvalidKeySpecException, NoSuchAlgorithmException {
        Requires.notNull(clipboardDataItem, "clipboardDataItem");

        boolean dataIgnored = false;
        boolean isCreditCard = false;
        boolean isPassword = false;

        DataService dataService = ServiceLocator.getService(DataService.class);

        String text = clipboardDataItem.getText().toString();
        if (StringUtils.isNullOrEmpty(text)) {
            dataIgnored = true;
        } else {
            isCreditCard = dataService.isCreditCard(text);
            if (isCreditCard && dataService.keepOrIgnoreCreditCard(text)) {
                dataIgnored = true;
            }

            isPassword = dataService.isPassword(text);
            if (isPassword && dataService.keepOrIgnorePassword(text)) {
                dataIgnored = true;
            }
        }

        if (!dataIgnored) {
            dataService.reset();

            QueryableArrayList<DataIdentifier> identifiers = dataService.getDataIdentifiers();
            Requires.isTrue(identifiers.size() == 1);

            writeClipboardDataToFile(text.getBytes("UTF-8"), identifiers.get(0));
            dataService.addDataEntry(new ClipboardData(text, new Date(System.currentTimeMillis())), identifiers, isCreditCard, isPassword);
        }
    }

    //endregion

    //region Methods

    @Override
    public void initialize(ServiceSettingProvider settingProvider) {
        _settingProvider = settingProvider;
        if (!CoreHelper.isUnitTesting()) {
            _clipboardManager = (ClipboardManager) App.getContext().getSystemService(Context.CLIPBOARD_SERVICE);
        }
        resume();
    }

    @Override
    public void close() throws IOException {
        pause();
    }

    @Override
    public void pause() {
        if (!_isPaused && _clipboardManager != null) {
            _clipboardManager.removePrimaryClipChangedListener(onClipboardChangedCallback);
        }
        _isPaused = true;
    }

    @Override
    public void reset() {
        pause();
    }

    @Override
    public void resume() {
        if (_isPaused && _clipboardManager != null) {
            _clipboardManager.addPrimaryClipChangedListener(onClipboardChangedCallback);
        }
        _isPaused = false;
    }

    /**
     * Encrypt a data from the clipboard and save it into a file.
     *
     * @param data       The data from the clipboard.
     * @param identifier The data identifier.
     * @throws IOException
     * @throws InvalidKeySpecException
     * @throws NoSuchAlgorithmException
     */
    private void writeClipboardDataToFile(byte[] data, DataIdentifier identifier) throws IOException, InvalidKeySpecException, NoSuchAlgorithmException {
        Requires.notNull(data, "data");
        Requires.notNull(identifier, "identifier");

        String fileName = identifier.getIdentifier().toString() + ".dat";

        if (App.getContext().getFileStreamPath(fileName).exists()) {
            throw new FileNotFoundException(fileName);
        }

        String dataPassword = SecurityHelper.encryptString(identifier.getIdentifier().toString());
        Requires.notNullOrWhiteSpace(dataPassword, "dataPassword");

        try (FileOutputStream fileStream = App.getContext().openFileOutput(fileName, Context.MODE_PRIVATE);
             AesOutputStream aesStream = new AesOutputStream(fileStream, dataPassword, SecurityHelper.getSaltKeys(dataPassword).getEncoded())) {

            int bufferSize = Consts.ClipboardDataBufferSize;
            int dataSize = data.length;
            int readLength = 0;

            while (readLength < dataSize) {
                // resize the buffer if we are at the end of the data and that it remains less than the default buffer size to read.
                if (dataSize - readLength < bufferSize) {
                    bufferSize = dataSize - readLength;
                }

                byte[] buffer = Arrays.copyOfRange(data, readLength, bufferSize);
                aesStream.write(buffer, 0, buffer.length);

                readLength += bufferSize;
            }
        }
    }

    //endregion
}
