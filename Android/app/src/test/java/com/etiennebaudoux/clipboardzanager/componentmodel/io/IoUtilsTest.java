package com.etiennebaudoux.clipboardzanager.componentmodel.io;

import org.junit.Test;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;

import static org.junit.Assert.assertArrayEquals;

public class IoUtilsTest {
    @Test
    public void copy() throws Exception {
        byte[] data = new byte[] { 72, 101, 108, 108, 111 }; // Hello
        ByteArrayInputStream input = new ByteArrayInputStream(data);
        ByteArrayOutputStream output = new ByteArrayOutputStream();

        IoUtils.copy(input, output);

        assertArrayEquals(new byte[] { 72, 101, 108, 108, 111 }, output.toByteArray());
    }
}
