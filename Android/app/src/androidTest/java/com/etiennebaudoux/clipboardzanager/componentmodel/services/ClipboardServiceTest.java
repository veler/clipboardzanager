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

    /**
     * Related to Issue #5 (The app crash after copying a large text)
     */
    @Test
    public void clipboardServiceLargeText() throws Exception {
        DataService dataService = getDataService();
        ClipboardService service = getClipboardService();

        String largeText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis eget" +
                " velit est. Proin pretium sem sed felis mollis feugiat. Nullam accumsan, erat" +
                " nec fringilla pharetra, lectus diam aliquam enim, id hendrerit neque justo at" +
                " neque. Pellentesque feugiat tortor sapien, sed ullamcorper velit luctus a." +
                " Vestibulum venenatis finibus consequat. Sed porta diam id quam faucibus," +
                " placerat pharetra urna tristique. Phasellus sed sollicitudin augue. Etiam" +
                " vel dui accumsan, eleifend enim sed, suscipit elit. Mauris sed consequat" +
                " tortor. Nunc vitae purus quis metus venenatis porta. Pellentesque nec magna" +
                " hendrerit, scelerisque erat sed, aliquam massa. Vivamus ex dui, euismod et diam" +
                " vel, facilisis bibendum erat. Aenean ac ante augue. Ut sed metus id nibh mollis" +
                " blandit vitae quis ex. Donec non iaculis orci, mattis pretium eros.Quisque" +
                " hendrerit a lacus a congue. Nunc egestas in nisi in blandit. Vestibulum" +
                " mattis arcu eu tempor ultricies. Cras dui mi, vulputate non eros eu," +
                " ullamcorper consectetur arcu. Ut mollis, est vitae tempus pharetra, elit ligula" +
                " laoreet leo, non tempor orci dui quis dolor. In elementum, tortor sit amet" +
                " pretium tristique, velit felis malesuada lorem, a tempor sem metus mollis lacus."+
                " Sed commodo justo ac justo pulvinar, eget finibus eros tempus. Fusce id turpis" +
                " finibus, varius massa ac, sagittis ligula. Curabitur id lorem ut ex mattis" +
                " rhoncus in non odio. Aenean ut risus eu velit ornare vestibulum. Duis faucibus" +
                " diam at elit molestie posuere. Phasellus gravida lacinia elit, non fringilla" +
                " leo rhoncus cursus. Ut viverra dui vitae nisi venenatis tempus. In consequat" +
                " orci id risus aliquet, condimentum bibendum ante placerat. Pellentesque id" +
                " mauris sed dui fermentum interdum.Ut facilisis nisl id aliquam rhoncus. Cras" +
                " pulvinar eros blandit neque suscipit elementum. Morbi quis ultricies urna." +
                " Suspendisse ut fermentum ipsum. Nullam a turpis tempus, tincidunt dolor vel," +
                " consequat elit. Etiam tristique accumsan ante, in rutrum eros imperdiet id." +
                " Fusce neque nisl, vehicula consectetur placerat at, vulputate maximus libero." +
                " Maecenas sed erat ac leo congue euismod quis ac nulla. Pellentesque eu aliquam" +
                " nunc. Orci varius natoque penatibus et magnis dis parturient montes, nascetur" +
                " ridiculus mus.Nam rhoncus ante a elit iaculis mollis. Nullam lacinia, sem sed" +
                " sodales blandit, felis arcu pharetra leo, vitae placerat leo tortor sit amet" +
                " mauris. Aliquam interdum dolor non eleifend malesuada. Aliquam commodo erat ac" +
                " orci venenatis, a pretium eros posuere. Sed tempus ipsum risus, et interdum" +
                " erat aliquet eget. Praesent tincidunt consectetur urna, at euismod ligula" +
                " vulputate vel. Sed posuere mi orci, sit amet aliquet erat ultrices et. Fusce" +
                " eu ligula in lorem ultricies rutrum vitae non quam. Donec auctor, magna a" +
                " ultrices tincidunt, ante lectus interdum felis, vel volutpat elit sapien sed" +
                " orci. Nullam placerat accumsan ornare. Nam dapibus posuere ligula, eu tempor" +
                " lacus vestibulum at. Sed dignissim, tellus in sollicitudin hendrerit, ex nisl" +
                " laoreet lorem, et egestas enim nunc vel ex. Praesent sit amet diam tempus," +
                " auctor massa quis, tincidunt mauris. Duis eu libero vel purus condimentum cursus"+
                " vitae nec tortor.Curabitur nec tincidunt tellus. Vestibulum ante ipsum primis" +
                " in faucibus orci luctus et ultrices posuere cubilia Curae; Curabitur ac nunc" +
                " ut mauris faucibus finibus quis eget dui. Aliquam vel nunc eget eros ullamcorper"+
                " porta. Vestibulum at rutrum diam. Pellentesque mollis quam nisi, non aliquet" +
                " nibh elementum ac. Sed a pellentesque nisi, a fringilla erat. Donec eget" +
                " interdum eros, id pellentesque lorem. Praesent eros massa, venenatis nec mi in," +
                " rhoncus luctus lacus. Etiam sem massa, vestibulum cursus nibh sed, consectetur" +
                " aliquam odio. Donec rutrum ipsum ac blandit aliquam. Aenean a arcu dolor.Nam" +
                " elementum quam eu pretium fermentum. In rutrum nec quam quis lacinia. Quisque" +
                " tempor ligula vitae eros bibendum, ut vestibulum est dapibus. Maecenas pulvinar" +
                " felis ac dui pretium, ut gravida dolor gravida. Ut tempor condimentum efficitur."+
                " Sed semper ex et ipsum pharetra, quis sagittis nibh rutrum. Suspendisse pulvinar"+
                " est est, id viverra nulla varius a.Pellentesque non ipsum arcu. Maecenas magna" +
                " eros, faucibus sed odio nec, hendrerit pretium elit. Quisque tempor, eros sit" +
                " amet tristique imperdiet, sapien orci volutpat ligula, sit amet auctor dui" +
                " metus id ante. Fusce tincidunt varius nisl, sit amet ornare augue placerat at." +
                " Etiam vulputate augue ut gravida accumsan. Class aptent taciti sociosqu ad" +
                " litora torquent per conubia nostra, per inceptos himenaeos. Phasellus finibus" +
                " venenatis lectus, eget dapibus turpis feugiat non. Curabitur viverra lorem" +
                " risus, eget gravida sapien fringilla vestibulum. Vivamus ex odio, consectetur" +
                " vel ipsum in, dapibus ultricies dolor. Ut nec diam fringilla, sodales turpis" +
                " sed, scelerisque magna. Mauris varius, augue quis dapibus venenatis, massa" +
                " neque euismod velit, ornare convallis velit elit sed arcu. Maecenas in" +
                " porttitor dui, id elementum risus. Aliquam venenatis sollicitudin justo" +
                " ut orci aliquam.";

        service.onClipboardChanged(new ClipData.Item(largeText));
        assertEquals(dataService.getDataEntries().size(), 1);
    }

    private ClipboardService getClipboardService() {
        return ServiceLocator.getService(ClipboardService.class);
    }

    private DataService getDataService() {
        return ServiceLocator.getService(DataService.class);
    }
}