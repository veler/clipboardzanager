package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.robolectric.RobolectricTestRunner;

import java.io.IOException;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

@RunWith(RobolectricTestRunner.class)
public class DataHelperTest {
    @Test
    public void stringsToByte() throws IOException, ClassNotFoundException {
        String entry = "Hello";
        byte[] bytes = DataHelper.toByteArray(entry);
        String result = DataHelper.fromByteArray(bytes, String.class);

        assertEquals(entry, result);
    }

    @SuppressWarnings({"unchecked", "InstantiatingObjectToGetClassObject"})
    @Test
    public void objectToBase64() throws IOException, ClassNotFoundException {
        QueryableArrayList<String> entry = new QueryableArrayList<>();
        entry.add("Hello");
        entry.add("World");

        String base64 = DataHelper.toBase64(entry);
        QueryableArrayList<String> result = DataHelper.fromBase64(base64, (Class<QueryableArrayList<String>>) new QueryableArrayList<String>().getClass());

        assertTrue(entry.equals(result));
    }
}