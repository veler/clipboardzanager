package com.etiennebaudoux.clipboardzanager.componentmodel.services;

import android.content.ClipData;
import android.support.test.runner.AndroidJUnit4;

import com.etiennebaudoux.clipboardzanager.TestUtilities;
import com.etiennebaudoux.clipboardzanager.componentmodel.core.DataHelper;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;

import static org.junit.Assert.assertEquals;

@RunWith(AndroidJUnit4.class)
public class ClipboardServiceTest {
    @Before
    public void testInitialize() throws Exception {
        TestUtilities.initialize();
    }

    @After
    public void testCleanUp() throws Exception {
        // TODO :
        // GetDataService().RemoveAllDataAsync().Wait();
        // GetDataService().Cache.Clear();

        getDataService().getCache().clear();
        getDataService().getDataEntries().clear();
    }

    @Test
    public void clipboardServiceAddData() throws Exception {
        DataService dataService = getDataService();
        ClipboardService service = getClipboardService();

        service.onClipboardChanged(new ClipData.Item("Hello World"));

        assertEquals(dataService.getDataEntries().size(), 1);
    }

    @Test
    public void clipboardServiceCreditCard() throws Exception {
        DataService dataService = getDataService();
        ClipboardService service = getClipboardService();

        service.onClipboardChanged(new ClipData.Item("  4974- 0411-3456- 7895 "));

        assertEquals(dataService.getDataEntries().size(), 0);

        service.onClipboardChanged(new ClipData.Item("  4974- 0411-3456- 7895 "));

        assertEquals(dataService.getDataEntries().size(), 1);
        assertEquals(DataHelper.fromBase64(dataService.getDataEntries().get(0).getThumbnail().getValue(), String.class), "4974-••••-••••-7895");

        service.onClipboardChanged(new ClipData.Item("  4974- 0411-3451- 7895 "));

        assertEquals(dataService.getDataEntries().size(), 1);

        service.onClipboardChanged(new ClipData.Item("  4974- 0411-3451- 7895 "));

        assertEquals(dataService.getDataEntries().size(), 2);

        TestUtilities.getSettingProvider().AvoidCreditCard = "false";

        service.onClipboardChanged(new ClipData.Item("  4974- 0411-3451- 7895 "));

        assertEquals(dataService.getDataEntries().size(), 3);
    }

    private ClipboardService getClipboardService() {
        return ServiceLocator.getService(ClipboardService.class);
    }

    private DataService getDataService() {
        return ServiceLocator.getService(DataService.class);
    }
}