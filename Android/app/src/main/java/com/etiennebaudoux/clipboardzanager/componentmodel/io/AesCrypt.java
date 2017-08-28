package com.etiennebaudoux.clipboardzanager.componentmodel.io;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.Requires;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.SecurityHelper;

import java.security.InvalidAlgorithmParameterException;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.security.spec.InvalidKeySpecException;

import javax.crypto.BadPaddingException;
import javax.crypto.Cipher;
import javax.crypto.IllegalBlockSizeException;
import javax.crypto.NoSuchPaddingException;
import javax.crypto.ShortBufferException;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

import static java.lang.System.arraycopy;

/**
 * Provides a class designed to crypt/decrypt a block of bytes.
 */

class AesCrypt {
    //region Fields

    private Cipher _cipher;

    //endregion

    //region Constructors

    /**
     * Initialize a new instance of the {@link AesCrypt} class.
     *
     * @param password The password used to encrypt or decrypt the data.
     * @param salt     Must be unique for each stream otherwise there is NO security.
     * @throws InvalidKeySpecException
     * @throws NoSuchAlgorithmException
     * @throws NoSuchPaddingException
     * @throws InvalidAlgorithmParameterException
     * @throws InvalidKeyException
     */
    AesCrypt(String password, byte[] salt) throws InvalidKeySpecException, NoSuchAlgorithmException, NoSuchPaddingException, InvalidAlgorithmParameterException, InvalidKeyException {
        Requires.notNull(password, "password");
        Requires.notNull(salt, "salt");

        byte[] key = SecurityHelper.getSaltKeys(password, salt).getEncoded();
        byte[] iv = new byte[16];
        _cipher = Cipher.getInstance("AES/CBC/NoPadding");
        _cipher.init(Cipher.ENCRYPT_MODE, new SecretKeySpec(key, "AES"), new IvParameterSpec(iv));
    }

    //endregion

    /**
     * Cipher or decipher a {@link Byte} array.
     *
     * @param buffer    The {@link Byte} array to cipher or decipher
     * @param offset    The offset
     * @param count     The buffer size
     * @param streamPos The position of the buffer in the base stream
     * @throws BadPaddingException
     * @throws IllegalBlockSizeException
     */
    void cipher(byte[] buffer, int offset, int count, long streamPos) throws BadPaddingException, IllegalBlockSizeException, ShortBufferException {
        // find block number
        int blockSizeInByte = _cipher.getBlockSize();
        long blockNumber = streamPos / blockSizeInByte + 1;
        long keyPos = streamPos % blockSizeInByte;

        // buffer
        byte[] outBuffer = new byte[blockSizeInByte];
        byte[] nonce = new byte[blockSizeInByte];
        boolean init = false;

        for (int i = offset; i < count; i++) {
            // encrypt the nonce to form next xor buffer (unique key)
            if (!init || keyPos % blockSizeInByte == 0) {
                byte[] blockArray = getBytes((int) blockNumber);
                arraycopy(blockArray, 0, nonce, 0, blockArray.length);
                _cipher.doFinal(nonce, 0, nonce.length, outBuffer, 0);
                if (init) {
                    keyPos = 0;
                }
                init = true;
                blockNumber++;
            }
            buffer[i] ^= outBuffer[(int) keyPos]; // simple XOR with generated unique key
            keyPos++;
        }
    }

    /**
     * Convert a integer to a block of 4 {@link Byte}.
     *
     * @param value The integer to convert
     * @return A block of 4 {@link Byte}.
     */
    private byte[] getBytes(int value) {
        byte[] bytes = new byte[4];
        bytes[0] = (byte) (value);
        bytes[1] = (byte) (value >> 8);
        bytes[2] = (byte) (value >> 16);
        bytes[3] = (byte) (value >> 24);
        return bytes;
    }
}
