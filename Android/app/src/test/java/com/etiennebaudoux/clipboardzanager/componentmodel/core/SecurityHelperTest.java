package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.robolectric.RobolectricTestRunner;

import java.security.NoSuchAlgorithmException;
import java.security.spec.InvalidKeySpecException;

import static org.junit.Assert.assertArrayEquals;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotEquals;

@RunWith(RobolectricTestRunner.class)
public class SecurityHelperTest {

    @Test
    public void encryptDecryptString() {
        String str = "My unsecured string";
        String encryptedString = SecurityHelper.encryptString(str);
        String encryptedString2 = SecurityHelper.encryptString(str);

        assertNotEquals(encryptedString, "");
        assertEquals(encryptedString, encryptedString2);

        String str2 = SecurityHelper.decryptString(encryptedString);
        String str3 = SecurityHelper.decryptString(encryptedString2);

        assertEquals(str, str2);
        assertEquals(str2, str3);

        String password = "MyPassword";
        encryptedString = SecurityHelper.encryptString(str, password);
        encryptedString2 = SecurityHelper.encryptString(str, password);

        assertEquals("NWp2ALQXrKNtf4ZDVaZx7CA+M7DEXBnt/+0wXCN7Ez6eDXT5gZ9RZg==", encryptedString);
        assertEquals(encryptedString, encryptedString2);

        str2 = SecurityHelper.decryptString(encryptedString, password);
        str3 = SecurityHelper.decryptString(encryptedString2, password);

        assertEquals(str, str2);
        assertEquals(str2, str3);
    }

    @Test
    public void encryptDecryptEmptyString() {
        String str = "";
        String encryptedString = SecurityHelper.encryptString(str);
        String encryptedString2 = SecurityHelper.encryptString(str);

        assertEquals(encryptedString, encryptedString2);

        String str2 = SecurityHelper.decryptString(str);
        String str3 = SecurityHelper.decryptString(str);

        assertEquals(str, str2);
        assertEquals(str2, str3);
    }

    @Test
    public void saltKeys() throws InvalidKeySpecException, NoSuchAlgorithmException {
        byte[] key = SecurityHelper.getSaltKeys("MyPassword").getEncoded();
        assertArrayEquals(new byte[]{6, 82, (byte) 254, 48, 47, (byte) 165, 77, 86, 53, 94, 25, 125, (byte) 168, (byte) 237, (byte) 149, 23}, key);
    }
}
