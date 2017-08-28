package com.etiennebaudoux.clipboardzanager.componentmodel.io;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.Requires;

import java.io.IOException;
import java.io.InputStream;

/**
 * Provides a InputStream encrypted by AES algorithm, supporting both synchronous and asynchronous read and write operations.
 */
public class AesInputStream extends InputStream {
    //region Fields

    private InputStream _baseStream;
    private AesCrypt _aes;

    //endregion

    //region Properties

    //region AutoDisposeBaseStream

    private boolean _autoDisposeBaseStream;
    private int _length;

    /**
     * Initialize a new instance of the {@link AesInputStream} class.
     *
     * @param baseStream The {@link InputStream} to read or write with encryption.
     * @param password   The password used to encrypt or decrypt the data.
     * @param salt       Must be unique for each stream otherwise there is NO security.
     */
    public AesInputStream(InputStream baseStream, String password, byte[] salt) {
        Requires.notNull(baseStream, "baseStream");
        Requires.notNull(password, "password");
        Requires.notNull(salt, "salt");

        try {
            _baseStream = baseStream;
            _length = baseStream.available();
            _aes = new AesCrypt(password, salt);
        } catch (Exception exception) {
            exception.printStackTrace();
        }

        setAutoDisposeBaseStream(true);
    }

    //endregion

    //region Length

    /**
     * Gets a value that defines whether the base stream must be disposed when the {@link AesInputStream} is disposing.
     *
     * @return True if the base stream must be auto disposed.
     */
    public boolean isAutoDisposeBaseStream() {
        return _autoDisposeBaseStream;
    }

    /**
     * Sets a value that defines whether the base stream must be disposed when the {@link AesInputStream} is disposing.
     *
     * @param value
     */
    public void setAutoDisposeBaseStream(boolean value) {
        _autoDisposeBaseStream = value;
    }

    //endregion

    //region Position

    /**
     * Gets the length of the stream.
     *
     * @return The length of the stream.
     */
    public int getLength() {
        return _length;
    }

    /**
     * Gets the position within the current stream.
     *
     * @return The position within the current stream.
     */
    public long getPosition() throws IOException {
        return getLength() - _baseStream.available();
    }

    //endregion

    //endregion

    //region Constructors

    /**
     * Sets the position within the current stream.
     *
     * @param value
     */
    public void setPosition(long value) throws IOException {
        _baseStream.reset();
        if (_baseStream.skip(value) != value) {
            throw new IOException("Unable to skip these values in the AesInputStream.");
        }
    }

    //endregion

    //region Methods

    @Override
    public synchronized void reset() throws IOException {
        _baseStream.reset();
        super.reset();
    }

    @Override
    public int available() throws IOException {
        return _baseStream.available();
    }

    @Override
    public int read(byte[] buffer, int offset, int count) {
        int ret = -1;
        try {
            long streamPos = getPosition();
            ret = _baseStream.read(buffer, offset, count);
            _aes.cipher(buffer, offset, count, streamPos);
        } catch (Exception exception) {
            exception.printStackTrace();
        }
        return ret;
    }

    @Override
    public int read() throws IOException {
        throw new UnsupportedOperationException();
    }

    @Override
    public void close() throws IOException {
        if (isAutoDisposeBaseStream()) {
            _baseStream.close();
        }

        super.close();
    }

    //endregion
}
