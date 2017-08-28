package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import org.junit.Test;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

public class QueryableArrayListTest {
    private static final String HelloString = "Hello";
    private static final String WorldString = "World";

    @Test
    public void first() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertEquals(list.first(), HelloString);

        list = new QueryableArrayList<>();
        try {
            list.first();
            fail();
        } catch (Exception ex) {
        }
    }

    @Test
    public void firstPredicate() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertEquals(list.first(item -> item.equals(WorldString)), WorldString);

        try {
            list.first(item -> item.equals("World2"));
            fail();
        } catch (Exception ex) {
        }
    }

    @Test
    public void firstOrDefault() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertEquals(list.firstOrDefault(), HelloString);

        list = new QueryableArrayList<>();
        assertEquals(list.firstOrDefault(), null);
    }

    @Test
    public void firstOrDefaultPredicate() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertEquals(list.firstOrDefault(item -> item.equals(WorldString)), WorldString);

        list = new QueryableArrayList<>();
        assertEquals(list.firstOrDefault(item -> item.equals("World2")), null);
    }

    @Test
    public void last() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertEquals(list.last(), "2");

        list = new QueryableArrayList<>();
        try {
            list.last();
            fail();
        } catch (Exception ex) {
        }
    }

    @Test
    public void single() throws Exception {
        QueryableArrayList<String> list = new QueryableArrayList<>();

        try {
            list.single();
            fail();
        } catch (Exception ex) {
        }

        list.add(HelloString);
        assertEquals(list.single(), HelloString);

        list.add(WorldString);
        try {
            list.single();
            fail();
        } catch (Exception ex) {
        }
    }

    @Test
    public void singlePredicate() throws Exception {
        QueryableArrayList<String> list = new QueryableArrayList<>();
        list.add(HelloString);

        try {
            list.single(item -> item.equals(WorldString));
            fail();
        } catch (Exception ex) {
        }

        list.add(WorldString);
        assertEquals(list.single(item -> item.equals(WorldString)), WorldString);

        list.add(WorldString);
        try {
            list.single(item -> item.equals(WorldString));
            fail();
        } catch (Exception ex) {
        }
    }

    @Test
    public void singleOrDefault() throws Exception {
        QueryableArrayList<String> list = new QueryableArrayList<>();

        assertEquals(list.singleOrDefault(), null);

        list.add(HelloString);
        assertEquals(list.singleOrDefault(), HelloString);

        list.add(WorldString);
        try {
            list.singleOrDefault();
            fail();
        } catch (Exception ex) {
        }
    }

    @Test
    public void singleOrDefaultPredicate() throws Exception {
        QueryableArrayList<String> list = new QueryableArrayList<>();
        assertEquals(list.singleOrDefault(), null);
        
        list.add(HelloString);

        try {
            list.singleOrDefault(item -> item.equals(WorldString));
            fail();
        } catch (Exception ex) {
        }

        list.add(WorldString);
        assertEquals(list.singleOrDefault(item -> item.equals(WorldString)), WorldString);

        list.add(WorldString);
        try {
            list.singleOrDefault(item -> item.equals(WorldString));
            fail();
        } catch (Exception ex) {
        }
    }

    @Test
    public void any() throws Exception {
        QueryableArrayList<String> list = new QueryableArrayList<>();
        assertFalse(list.any());

        list.add(HelloString);
        assertTrue(list.any());
    }

    @Test
    public void anyPredicate() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertTrue(list.any(item -> item.equals(WorldString)));
        assertFalse(list.any(item -> item.equals("World2")));
    }

    @Test
    public void allPredicate() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertTrue(list.all(item -> item.length() > 0));
        assertFalse(list.all(item -> item.equals(WorldString)));
    }

    @Test
    public void wherePrecidate() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertEquals(list.where(item -> item.length() > 1).size(), 2);
    }

    @Test
    public void ofType() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertEquals(list.ofType(String.class).size(), 4);
        assertEquals(list.ofType(Boolean.class).size(), 0);
    }

    @Test
    public void skip() throws Exception {
        QueryableArrayList<String> list = getMockList();
        assertEquals(list.skip(3).single(), "2");
    }

    @Test
    public void union() throws Exception {
        String A = "A";
        String B = "B";
        String C = "C";
        String D = "D";
        String E = "E";
        String F = "F";

        QueryableArrayList<String> list1 = new QueryableArrayList<>();
        list1.add(A);
        list1.add(B);
        list1.add(C);

        QueryableArrayList<String> list2 = new QueryableArrayList<>();
        list1.add(B);
        list1.add(C);
        list1.add(D);
        list1.add(E);
        list1.add(F);

        QueryableArrayList<String> list3 = list1.union(list2);
        assertEquals(list3.size(), 6);
        assertEquals(list3.get(0), A);
        assertEquals(list3.get(1), B);
        assertEquals(list3.get(2), C);
        assertEquals(list3.get(3), D);
        assertEquals(list3.get(4), E);
        assertEquals(list3.get(5), F);
    }

    private QueryableArrayList<String> getMockList() {
        QueryableArrayList<String> list = new QueryableArrayList<>();
        list.add(HelloString);
        list.add(WorldString);
        list.add("1");
        list.add("2");
        return list;
    }
}