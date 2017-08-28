package com.etiennebaudoux.clipboardzanager.componentmodel.io;

import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

/**
 * Provides a set of functions designed to help using Stream.
 */
public final class IoUtils {
    private static final int BUFFER_SIZE = 1024 * 2;

    /**
     * Reads the bytes from the current stream and writes them to another stream.
     *
     * @param input  The stream from which the contents of the current stream will be copied.
     * @param output The stream to which the contents of the current stream will be copied.
     * @throws IOException
     */
    public static void copy(InputStream input, OutputStream output) throws IOException {
        byte[] buffer = new byte[BUFFER_SIZE];
        BufferedInputStream in = new BufferedInputStream(input, BUFFER_SIZE);
        BufferedOutputStream out = new BufferedOutputStream(output, BUFFER_SIZE);
        int n = 0;
        try {
            while ((n = in.read(buffer, 0, BUFFER_SIZE)) != -1) {
                out.write(buffer, 0, n);
            }
            out.flush();
        } finally {
            try {
                out.close();
            } catch (IOException e) {
                e.printStackTrace();
            }
            try {
                in.close();
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }
}
