package com.etiennebaudoux.clipboardzanager.componentmodel.io;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.Requires;

import java.io.IOException;
import java.io.OutputStream;

/**
 * Provides a OutputStream encrypted by AES algorithm, supporting both synchronous and asynchronous read and write operations.
 */
public class AesOutputStream extends OutputStream {
    //region Fields

    private OutputStream _baseStream;
    private AesCrypt _aes;

    //endregion

    //region Properties

    //region AutoDisposeBaseStream

    private boolean _autoDisposeBaseStream;
    private int _length;

    /**
     * Initialize a new instance of the {@link AesOutputStream} class.
     *
     * @param baseStream The {@link OutputStream} to read or write with encryption.
     * @param password   The password used to encrypt or decrypt the data.
     * @param salt       Must be unique for each stream otherwise there is NO security.
     */
    public AesOutputStream(OutputStream baseStream, String password, byte[] salt) {
        Requires.notNull(baseStream, "baseStream");
        Requires.notNull(password, "password");
        Requires.notNull(salt, "salt");

        try {
            _baseStream = baseStream;
            _aes = new AesCrypt(password, salt);
        } catch (Exception exception) {
            exception.printStackTrace();
        }

        setAutoDisposeBaseStream(true);
    }

    //endregion

    //region Length

    /**
     * Gets a value that defines whether the base stream must be disposed when the {@link AesOutputStream} is disposing.
     *
     * @return True if the base stream must be auto disposed.
     */
    public boolean isAutoDisposeBaseStream() {
        return _autoDisposeBaseStream;
    }

    /**
     * Sets a value that defines whether the base stream must be disposed when the {@link AesOutputStream} is disposing.
     *
     * @param value
     */
    public void setAutoDisposeBaseStream(boolean value) {
        _autoDisposeBaseStream = value;
    }

    //endregion

    //endregion

    //region Constructors

    /**
     * Gets the length of the stream.
     *
     * @return The length of the stream.
     */
    public int getLength() {
        return _length;
    }

    //endregion

    //region Methods

    @Override
    public void write(byte[] buffer, int offset, int count) {
        try {
            long streamPos = getLength();
            _aes.cipher(buffer, offset, count, streamPos);
            _baseStream.write(buffer, offset, count);
            _length += count;
        } catch (Exception exception) {
            exception.printStackTrace();
        }
    }

    @Override
    public void write(int b) throws IOException {
        throw new UnsupportedOperationException();
    }

    @Override
    public void flush() throws IOException {
        _baseStream.flush();
        super.flush();
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
