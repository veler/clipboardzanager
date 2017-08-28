package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage;

import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotAuthenticatedException;
import com.etiennebaudoux.clipboardzanager.mocks.CloudAuthenticationMock;
import com.etiennebaudoux.clipboardzanager.mocks.CloudStorageProviderMock;
import com.etiennebaudoux.clipboardzanager.mocks.CloudTokenProviderMock;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.robolectric.RobolectricTestRunner;

import java.io.ByteArrayOutputStream;
import java.io.FileNotFoundException;
import java.util.Arrays;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

@RunWith(RobolectricTestRunner.class)
public class CloudStorageTests {
    @Test
    public void cloudAuthentication() {
        CloudAuthenticationMock authentication = new CloudAuthenticationMock();

        AuthenticationResult success = authentication.authenticateAsync("http://authenticationUri", "http://exceptedUi").await();
        AuthenticationResult fail = authentication.authenticateAsync("", "http://exceptedUi").await();

        assertFalse(success.isCanceled());
        assertEquals("http://exceptedUi", success.getRedirectedUri());
        assertTrue(fail.isCanceled());
    }

    @Test
    public void cloudTokenProvider() {
        CloudTokenProviderMock tokenProvider = new CloudTokenProviderMock();

        String token = tokenProvider.getToken("MyToken");
        assertEquals("123456789abc", token);

        try {
            String token2 = tokenProvider.getToken("MyToken2");
            fail();
        } catch (Exception ex) {
        }
    }

    @Test
    public void cloudStorageProvider() {
        CloudStorageProviderMock storageProvider = new CloudStorageProviderMock();
        ByteArrayOutputStream stream = new ByteArrayOutputStream();

        assertEquals("MockCloudStorageProvider", storageProvider.getCloudServiceName());
        assertFalse(storageProvider.isAuthenticated());
        assertTrue(storageProvider.credentialExists());

        try {
            storageProvider.downloadFileAsync("/path/MyApp/myFile.txt", stream).await();
            fail();
        } catch (RuntimeException ex) {
            assertTrue(ex.getCause() instanceof NotAuthenticatedException);
        } catch (Exception ex) {
            fail();
        }

        assertFalse(storageProvider.tryAuthenticateAsync().await());
        assertFalse(storageProvider.isAuthenticated());

        assertTrue(storageProvider.tryAuthenticateWithUiAsync(true).await());
        assertTrue(storageProvider.isAuthenticated());

        try {
            storageProvider.downloadFileAsync("/path/MyApp/myFile.txt", null).await();
            fail();
        } catch (RuntimeException ex) {
            assertTrue(ex.getCause() instanceof NullPointerException);
        } catch (Exception ex) {
            fail();
        }

        try {
            storageProvider.downloadFileAsync("/path/MyApp/myFile2.txt", stream).await();
            fail();
        } catch (RuntimeException ex) {
            assertTrue(ex.getCause() instanceof FileNotFoundException);
        } catch (Exception ex) {
            fail();
        }

        storageProvider.downloadFileAsync("/path/MyApp/myFile.txt", stream).await();
        assertTrue(Arrays.equals(stream.toByteArray(), new byte[] { 0, 1, 2, 3, 4 }));

        assertTrue(storageProvider.isAuthenticated());
        storageProvider.signOutAsync().await();
        assertFalse(storageProvider.isAuthenticated());
    }
}
