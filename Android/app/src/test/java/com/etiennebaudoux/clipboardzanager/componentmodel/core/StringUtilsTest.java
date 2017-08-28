package com.etiennebaudoux.clipboardzanager.componentmodel.core;


import org.junit.Test;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

public class StringUtilsTest {
    @Test
    public void isNullOrEmpty() {
        assertTrue(StringUtils.isNullOrEmpty(""));
        assertTrue(StringUtils.isNullOrEmpty(null));
        assertFalse(StringUtils.isNullOrEmpty(" "));
        assertFalse(StringUtils.isNullOrEmpty("yo"));
    }

    @Test
    public void isEmptyOrWhiteSpace() {
        assertTrue(StringUtils.isEmptyOrWhiteSpace(""));
        assertTrue(StringUtils.isEmptyOrWhiteSpace(null));
        assertTrue(StringUtils.isEmptyOrWhiteSpace(" "));
        assertFalse(StringUtils.isEmptyOrWhiteSpace("yo"));
    }
}