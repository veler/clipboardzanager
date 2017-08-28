package com.etiennebaudoux.clipboardzanager.componentmodel.io;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.SecurityHelper;

import org.junit.Test;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.security.NoSuchAlgorithmException;
import java.security.spec.InvalidKeySpecException;

import static org.junit.Assert.assertArrayEquals;
import static org.junit.Assert.assertEquals;

public class AesStreamTest {
    @Test
    public void aesStreamRead() throws InvalidKeySpecException, NoSuchAlgorithmException, IOException {
        byte[] cryptedData = new byte[] { (byte)238, 75, 117, (byte)248, 55 };
        String password = "MyPassword";

        AesInputStream aesStream = new AesInputStream(new ByteArrayInputStream(cryptedData), password, SecurityHelper.getSaltKeys(password).getEncoded());
        byte[] data = new byte[aesStream.getLength()];
        aesStream.read(data, 0, data.length);

        aesStream.close();

        assertArrayEquals(new byte[] { 72, 101, 108, 108, 111 }, data);
        assertEquals(new String(data, "ASCII"), "Hello");
    }

    @Test
    public void aesStreamWrite() throws InvalidKeySpecException, NoSuchAlgorithmException, IOException {
        String password = "MyPassword";
        byte[] data = "Hello".getBytes("US-ASCII");
        assertArrayEquals(new byte[] { 72, 101, 108, 108, 111 }, data);

        ByteArrayOutputStream baseStream = new ByteArrayOutputStream();
        AesOutputStream aesStream = new AesOutputStream(baseStream, password, SecurityHelper.getSaltKeys(password).getEncoded());

        aesStream.write(data, 0, data.length);

        byte[] cryptedData = baseStream.toByteArray();
        assertArrayEquals(new byte[] { (byte)238, 75, 117, (byte)248, 55 }, cryptedData);
    }
}
