package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import java.nio.charset.StandardCharsets;
import java.security.NoSuchAlgorithmException;
import java.security.spec.InvalidKeySpecException;
import java.security.spec.KeySpec;

import javax.crypto.Cipher;
import javax.crypto.SecretKey;
import javax.crypto.SecretKeyFactory;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.PBEKeySpec;
import javax.crypto.spec.SecretKeySpec;

/**
 * Provides a set of functions designed to manage the security
 */

public final class SecurityHelper {
    /**
     * Encrypt a {@link String}
     *
     * @param input The {@link String} to encrypt.
     * @return An encrypted {@link String}
     */
    public static String encryptString(String input) {
        return encryptString(input, CoreHelper.getApplicationVersion());
    }

    /**
     * Encrypt a {@link String}
     *
     * @param input    The {@link String} to encrypt.
     * @param password The {@link String} that represents the password
     * @return An encrypted {@link String}
     */
    public static String encryptString(String input, String password) {
        try {
            byte[] salt = getSaltKeys(password).getEncoded();
            byte[] key = new byte[8];
            byte[] iv = new byte[8];

            System.arraycopy(salt, 0, key, 0, 8);
            System.arraycopy(salt, 8, iv, 0, 8);
            Cipher cipher = Cipher.getInstance("DES/CBC/PKCS5Padding");
            cipher.init(Cipher.ENCRYPT_MODE, new SecretKeySpec(key, "DES"), new IvParameterSpec(iv));

            byte[] inputBuffer = input.getBytes(StandardCharsets.UTF_16LE);
            byte[] outputBuffer = cipher.doFinal(inputBuffer, 0, inputBuffer.length);
            return DataHelper.toBase64(outputBuffer);
        } catch (Exception ex) {
            return "";
        }
    }

    /**
     * Decrypt an encrypted {@link String}
     *
     * @param encryptedData The encrypted {@link String}
     * @return A {@link String} that corresponds to the {@link String}'s value
     */
    public static String decryptString(String encryptedData) {
        return decryptString(encryptedData, CoreHelper.getApplicationVersion());
    }

    /**
     * Decrypt an encrypted {@link String}
     *
     * @param encryptedData The encrypted {@link String}
     * @param password      The {@link String} that represents the password
     * @return A {@link String} that corresponds to the {@link String}'s value
     */
    public static String decryptString(String encryptedData, String password) {
        try {
            byte[] salt = getSaltKeys(password).getEncoded();
            byte[] key = new byte[8];
            byte[] iv = new byte[8];

            System.arraycopy(salt, 0, key, 0, 8);
            System.arraycopy(salt, 8, iv, 0, 8);
            Cipher cipher = Cipher.getInstance("DES/CBC/PKCS5Padding");
            cipher.init(Cipher.DECRYPT_MODE, new SecretKeySpec(key, "DES"), new IvParameterSpec(iv));

            byte[] inputBuffer = DataHelper.byteArrayFromBase64(encryptedData);
            byte[] outputBuffer = cipher.doFinal(inputBuffer, 0, inputBuffer.length);
            return new String(outputBuffer, StandardCharsets.UTF_16LE);
        } catch (Exception ex) {
            return "";
        }
    }

    /**
     * Generate a {@link SecretKey} that we can use to retrieve the KEY and IV.
     *
     * @param password The password
     * @return a {@link SecretKey}
     * @throws NoSuchAlgorithmException
     * @throws InvalidKeySpecException
     */
    public static SecretKey getSaltKeys(String password) throws InvalidKeySpecException, NoSuchAlgorithmException {
        byte[] salt = new byte[]{0x53, 0x69, 0x75, 0x6f, 0x65, 0x20, 0x43, 0x69, 0x61, 0x68, 0x6d, 0x6c, 0x6f, 0x72, 0x64, 0x69, 0x64};
        return getSaltKeys(password, salt);
    }

    /**
     * Generate a {@link SecretKey} that we can use to retrieve the KEY and IV.
     *
     * @param password The password
     * @param salt     The salt key
     * @return a {@link SecretKey}
     * @throws NoSuchAlgorithmException
     * @throws InvalidKeySpecException
     */
    public static SecretKey getSaltKeys(String password, byte[] salt) throws NoSuchAlgorithmException, InvalidKeySpecException {
        Requires.notNullOrEmpty(password, "password");

        SecretKeyFactory secretKeyFactory = SecretKeyFactory.getInstance("PBKDF2WithHmacSHA1");
        KeySpec keySpec = new PBEKeySpec(password.toCharArray(), salt, 1000, 128);
        return secretKeyFactory.generateSecret(keySpec);
    }
}
