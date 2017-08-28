package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import android.util.Base64;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.ObjectInput;
import java.io.ObjectInputStream;
import java.io.ObjectOutput;
import java.io.ObjectOutputStream;
import java.io.Serializable;

/**
 * Provides a set of functions designed to convert data
 */

public final class DataHelper {
    /**
     * Convert any serializable object to a {@link Byte} array
     *
     * @param value The value to convert
     * @param <T>   Represents a type that corresponds to a class
     * @return Returns a {@link Byte} array. If the value is null, returns null.
     * @throws IOException
     */
    public static <T extends Serializable> byte[] toByteArray(T value) throws IOException {
        if (value == null) {
            return null;
        }

        try (ByteArrayOutputStream bos = new ByteArrayOutputStream();
             ObjectOutput out = new ObjectOutputStream(bos)) {
            out.writeObject(value);
            out.flush();
            return bos.toByteArray();
        }
    }

    /**
     * Convert a {@link Byte} array to the specified type
     *
     * @param array The {@link Byte} array
     * @param type  The expected result type
     * @param <T>   The expected data type
     * @return Returns the converted value. If the array is null, or if the data cannot be converted, returns null.
     * @throws IOException
     * @throws ClassNotFoundException
     */
    public static <T> T fromByteArray(byte[] array, Class<T> type) throws IOException, ClassNotFoundException {
        if (array == null) {
            return null;
        }

        try (ByteArrayInputStream bis = new ByteArrayInputStream(array);
             ObjectInput in = new ObjectInputStream(bis)) {
            return type.cast(in.readObject());
        }
    }

    /**
     * Convert a value to a {@link Byte} array and then to a base64 {@link String}.
     *
     * @param value The value to convert
     * @param <T>   Represents a type that corresponds to a class
     * @return Returns a {@link String}. If the value is null, throw an exception.
     * @throws IOException
     */
    public static <T extends Serializable> String toBase64(T value) throws IOException {
        return toBase64(toByteArray(value));
    }

    /**
     * Convert a {@link Byte} array and then to a base64 {@link String}.
     *
     * @param value The value to convert
     * @return Returns a {@link String}. If the value is null, throw an exception.
     */
    static String toBase64(byte[] value) {
        return Base64.encodeToString(value, Base64.NO_WRAP);
    }

    /**
     * Convert a base64 {@link String} to the specified Type.
     *
     * @param base64String The base64 {@link String} to convert
     * @param type         The expected result type
     * @param <T>          Represents a type that corresponds to a class
     * @return Returns the converted value. If the value is null, throw an exception.
     * @throws IOException
     * @throws ClassNotFoundException
     */
    public static <T> T fromBase64(String base64String, Class<T> type) throws IOException, ClassNotFoundException {
        return fromByteArray(byteArrayFromBase64(base64String), type);
    }

    /**
     * Convert a base64 {@link String} to a byte array.
     *
     * @param base64String The base64 {@link String} to convert
     * @return Returns the converted value. If the value is null, throw an exception.
     */
    static byte[] byteArrayFromBase64(String base64String) {
        return Base64.decode(base64String, Base64.NO_WRAP);
    }
}
