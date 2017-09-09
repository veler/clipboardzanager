package com.etiennebaudoux.clipboardzanager.componentmodel.services;

import android.support.test.runner.AndroidJUnit4;

import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.TestUtilities;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.DataHelper;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;
import com.etiennebaudoux.clipboardzanager.enums.DataEntryStatus;
import com.etiennebaudoux.clipboardzanager.enums.ThumbnailDataType;
import com.etiennebaudoux.clipboardzanager.models.ClipboardData;
import com.etiennebaudoux.clipboardzanager.models.DataEntry;
import com.etiennebaudoux.clipboardzanager.models.DataEntryCache;
import com.etiennebaudoux.clipboardzanager.models.DataIdentifier;
import com.etiennebaudoux.clipboardzanager.models.Link;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;

import java.util.Date;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.TimeUnit;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotEquals;
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

@RunWith(AndroidJUnit4.class)
public class DataServiceTests {
    private static final String MicrosoftPassword = "M|cr0sof t";
    private static final String CreditCardNumber = "  4974- 0411-3456- 7895 ";

    @Before
    public void testInitialize() throws Exception {
        TestUtilities.initialize();
    }

    @After
    public void testCleanUp() throws Exception {
        getDataService().removeAllDataAsync().await();

        getDataService().getCache().clear();
        getDataService().getDataEntries().clear();

        for (String file : App.getContext().fileList()) {
            App.getContext().deleteFile(file);
        }
    }

    @Test
    public void isHexColor() throws Exception {
        DataService service = getDataService();

        assertTrue(service.isHexColor("#1f1f1F"));
        assertTrue(service.isHexColor("#AFAFAF"));
        assertTrue(service.isHexColor("#1AFFa1"));
        assertTrue(service.isHexColor("#222fff"));
        assertTrue(service.isHexColor("#F00"));
        assertTrue(service.isHexColor("#bbffffff"));
        assertFalse(service.isHexColor("123456"));
        assertFalse(service.isHexColor("#afafah"));
        assertFalse(service.isHexColor("#123abce"));
        assertFalse(service.isHexColor("aFaE3f"));
        assertFalse(service.isHexColor("F00"));
        assertFalse(service.isHexColor("#afaf"));
        assertFalse(service.isHexColor("#F0h"));
    }

    @Test
    public void isCreditCard() throws Exception {
        DataService service = getDataService();

        assertTrue(service.isCreditCard(CreditCardNumber));
        assertTrue(service.isCreditCard("  4974- 0411-   3456- 7895 "));
        assertTrue(service.isCreditCard("hello  4974- 0411-   3456- 7895 world"));
        assertTrue(service.isCreditCard("hello  4974- 0411- hey  3456- 7895 world"));
        assertFalse(service.isCreditCard("hello  1234- 0411- hey  3456- 7895 world"));
    }

    @Test
    public void isPassword() throws Exception {
        DataService service = getDataService();

        assertFalse(service.isPassword("Hello"));
        assertTrue(service.isPassword(MicrosoftPassword));
    }

    @Test
    public void keepOrIgnoreCreditCard() throws Exception {
        DataService service = getDataService();

        assertTrue(service.keepOrIgnoreCreditCard(CreditCardNumber));
        assertFalse(service.keepOrIgnoreCreditCard(CreditCardNumber));

        TestUtilities.getSettingProvider().AvoidCreditCard = "false";

        assertFalse(service.keepOrIgnoreCreditCard(CreditCardNumber));
        assertFalse(service.keepOrIgnoreCreditCard(CreditCardNumber));
        assertFalse(service.keepOrIgnoreCreditCard(CreditCardNumber));

        TestUtilities.getSettingProvider().AvoidCreditCard = "true";

        assertTrue(service.keepOrIgnoreCreditCard("  4974- 0412-3456- 7895 "));
        assertTrue(service.keepOrIgnoreCreditCard(CreditCardNumber));
        assertTrue(service.keepOrIgnoreCreditCard("  4974- 0412-3456- 7895 "));
    }

    @Test
    public void keepOrIgnorePassword() throws Exception {
        DataService service = getDataService();

        assertTrue(service.keepOrIgnorePassword(MicrosoftPassword));
        assertFalse(service.keepOrIgnorePassword(MicrosoftPassword));

        TestUtilities.getSettingProvider().AvoidPasswords = "false";

        assertFalse(service.keepOrIgnorePassword(MicrosoftPassword));
        assertFalse(service.keepOrIgnorePassword(MicrosoftPassword));
        assertFalse(service.keepOrIgnorePassword(MicrosoftPassword));

        TestUtilities.getSettingProvider().AvoidPasswords = "true";

        assertTrue(service.keepOrIgnorePassword("M||cr0sof t"));
        assertTrue(service.keepOrIgnorePassword("M|||cr0sof t"));
        assertTrue(service.keepOrIgnorePassword("M||cr0sof t"));
    }

    @Test
    public void getDataIdentifiers() throws Exception {
        DataService service = getDataService();

        List<DataIdentifier> identifiers = service.getDataIdentifiers();

        assertEquals(identifiers.size(), 1);
        assertEquals(identifiers.get(0).getFormatName(), "Text");
    }

    @Test
    public void addRemoveData() throws Exception {
        DataService service = getDataService();

        try {
            service.addDataEntry(null, new QueryableArrayList<>(), false, false);
            fail();
        } catch (Exception ex) {
        }

        ClipboardData entry = new ClipboardData("Hello World", new Date(System.currentTimeMillis()));
        service.addDataEntry(entry, new QueryableArrayList<>(), false, false);
        UUID guid1 = service.getDataEntries().last().getIdentifier();
        UUID guid11 = service.getCache().last().getIdentifier();

        entry = new ClipboardData("Hello World 2", new Date(System.currentTimeMillis()));
        service.addDataEntry(entry, new QueryableArrayList<>(), false, false);
        UUID guid2 = service.getDataEntries().first().getIdentifier();
        UUID guid22 = service.getCache().first().getIdentifier();

        assertNotEquals(guid1, guid2);
        assertNotEquals(guid1, guid22);
        assertNotEquals(guid11, guid2);

        service.removeDataAsync(service.getDataEntries().first().getIdentifier(), service.getDataEntries().first().getDataIdentifiers()).await();

        assertEquals(1, service.getDataEntries().size());
        assertEquals(2, service.getCache().size());
        assertEquals(DataEntryStatus.DELETED, service.getCache().first().getStatus());
        assertEquals(DataEntryStatus.ADDED, service.getCache().last().getStatus());
    }

    @Test
    public void dataEntry() throws Exception {
        DataService service = getDataService();

        ClipboardData entry = new ClipboardData("Hello World", new Date(System.currentTimeMillis()));
        service.addDataEntry(entry, new QueryableArrayList<>(), false, false);

        DataEntry dataEntry = service.getDataEntries().get(service.getDataEntries().size() - 1);
        DataEntryCache dataEntryCache = service.getCache().get(service.getCache().size() - 1);

        assertTrue(dataEntry.canSynchronize());
        assertFalse(dataEntry.isFavorite());
        assertTrue(dataEntry.getIdentifier() != null);
        assertTrue(dataEntry.getDate() != null);

        assertEquals(dataEntry.getIdentifier(), dataEntryCache.getIdentifier());
        assertEquals(DataEntryStatus.ADDED, dataEntryCache.getStatus());
    }

    @Test
    public void dataEntryThumbnailText() throws Exception {
        DataService service = getDataService();

        String value = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
        ClipboardData entry = new ClipboardData(value, new Date(System.currentTimeMillis()));
        service.addDataEntry(entry, new QueryableArrayList<>(), false, false);

        DataEntry dataEntry = service.getDataEntries().get(0);

        assertEquals(dataEntry.getThumbnail().getType(), ThumbnailDataType.STRING);
        assertEquals(DataHelper.fromBase64(dataEntry.getThumbnail().getValue(), String.class), "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It h...");
    }

    @Test
    public void dataEntryThumbnailLink() throws Exception {
        DataService service = getDataService();

        String value = "http://www.google.com";
        ClipboardData entry = new ClipboardData(value, new Date(System.currentTimeMillis()));
        service.addDataEntry(entry, new QueryableArrayList<>(), false, false);
        DataEntry dataEntry = service.getDataEntries().get(0);

        assertEquals(dataEntry.getThumbnail().getType(), ThumbnailDataType.LINK);
        assertEquals(DataHelper.fromBase64(dataEntry.getThumbnail().getValue(), Link.class).getUri(), "http://www.google.com");
        assertEquals(DataHelper.fromBase64(dataEntry.getThumbnail().getValue(), Link.class).getTitle(), "Google");
    }

    @Test
    public void expireLimit() throws Exception {
        DataService service = getDataService();

        TestUtilities.getSettingProvider().MaxDataToKeep = Integer.toString(Integer.parseInt(TestUtilities.getSettingProvider().MaxDataToKeep) + 5);

        for (int i = 0; i < Integer.parseInt(TestUtilities.getSettingProvider().DateExpireLimit) + 5; i++) {
            String value = Integer.toString(Integer.parseInt(TestUtilities.getSettingProvider().DateExpireLimit) + 5 - i);
            Date date = new Date(System.currentTimeMillis() - TimeUnit.DAYS.toMillis((Integer.parseInt(TestUtilities.getSettingProvider().DateExpireLimit) + 5 - i)));
            service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), false, false);
        }

        assertEquals(service.getDataEntries().size(), Integer.parseInt(TestUtilities.getSettingProvider().DateExpireLimit) - 1);
        assertEquals(DataHelper.fromBase64(service.getDataEntries().first().getThumbnail().getValue(), String.class), "1");
        assertEquals(DataHelper.fromBase64(service.getDataEntries().last().getThumbnail().getValue(), String.class), Integer.toString(Integer.parseInt(TestUtilities.getSettingProvider().DateExpireLimit) - 1));
    }

    @Test
    public void maxDataToKeep() throws Exception {
        DataService service = getDataService();

        for (int i = 0; i < Integer.parseInt(TestUtilities.getSettingProvider().MaxDataToKeep) + 5; i++) {
            String value = Integer.toString(Integer.parseInt(TestUtilities.getSettingProvider().MaxDataToKeep) + 5 - i);
            Date date = new Date(System.currentTimeMillis());
            service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), false, false);
        }

        assertEquals(service.getDataEntries().size(), Integer.parseInt(TestUtilities.getSettingProvider().MaxDataToKeep));
        assertEquals(DataHelper.fromBase64(service.getDataEntries().first().getThumbnail().getValue(), String.class), "1");
        assertEquals(DataHelper.fromBase64(service.getDataEntries().last().getThumbnail().getValue(), String.class), TestUtilities.getSettingProvider().MaxDataToKeep);
    }

    @Test
    public void favorite() throws Exception {
        DataService service = getDataService();

        for (int i = 0; i < 10; i++) {
            String value = Integer.toString(i);
            Date date = new Date(System.currentTimeMillis());
            service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), false, false);
        }

        service.addDataEntry(new ClipboardData("-1", new Date(System.currentTimeMillis())), new QueryableArrayList<>(), false, false);

        assertEquals(service.getDataEntries().size(), 11);
        assertEquals(DataHelper.fromBase64(service.getDataEntries().first().getThumbnail().getValue(), String.class), "-1");
        assertEquals(DataHelper.fromBase64(service.getDataEntries().last().getThumbnail().getValue(), String.class), "0");

        service.getDataEntries().last().setIsFavorite(true);
        service.reorganizeAsync(true).await();

        assertEquals(service.getDataEntries().size(), 11);
        assertEquals(DataHelper.fromBase64(service.getDataEntries().first().getThumbnail().getValue(), String.class), "0");
        assertEquals(DataHelper.fromBase64(service.getDataEntries().get(1).getThumbnail().getValue(), String.class), "-1");
    }

    @Test
    public void removeAll() throws Exception {
        DataService service = getDataService();

        for (int i = 0; i < 10; i++) {
            String value = Integer.toString(i);
            Date date = new Date(System.currentTimeMillis());
            service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), false, false);
        }

        assertEquals(service.getDataEntries().size(), 10);
        assertEquals(service.getCache().size(), 10);

        assertTrue(service.getCache().all(dataEntryCache -> dataEntryCache.getStatus() == DataEntryStatus.ADDED));
        service.removeAllDataAsync().await();

        assertEquals(service.getDataEntries().size(), 0);

        assertEquals(service.getCache().size(), 10);

        assertTrue(service.getCache().all(dataEntryCache -> dataEntryCache.getStatus() == DataEntryStatus.DELETED));
    }

    @Test
    public void disablePasswordAndCreditCardSync() throws Exception {
        DataService service = getDataService();
        Date date = new Date(System.currentTimeMillis());
        String value = "Hello";

        service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), false, false);
        assertTrue(service.getDataEntries().first().canSynchronize());

        service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), true, false);
        assertFalse(service.getDataEntries().first().canSynchronize());

        service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), false, true);
        assertFalse(service.getDataEntries().first().canSynchronize());

        TestUtilities.getSettingProvider().DisablePasswordAndCreditCardSync = "false";

        service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), false, false);
        assertTrue(service.getDataEntries().first().canSynchronize());

        service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), true, false);
        assertTrue(service.getDataEntries().first().canSynchronize());

        service.addDataEntry(new ClipboardData(value, date), new QueryableArrayList<>(), false, true);
        assertTrue(service.getDataEntries().first().canSynchronize());
    }

    private DataService getDataService() {
        return ServiceLocator.getService(DataService.class);
    }
}