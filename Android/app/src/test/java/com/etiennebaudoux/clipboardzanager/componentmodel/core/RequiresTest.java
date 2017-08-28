package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.IsFalseRequiredException;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.IsTrueRequiredException;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotNullOrEmptyRequiredException;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotNullOrWhiteSpaceRequiredException;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotNullRequiredException;

import org.junit.Test;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;

public class RequiresTest {
    @Test
    public void requiresNotNull() {
        try {
            Requires.notNull(null, "param1");
            fail();
        } catch (NotNullRequiredException e) {
            assertEquals(e.getMessage(), "The value must not be null : param1");
        }
    }

    @Test
    public void requiresNotNullOrEmpty() {
        try {
            Requires.notNullOrEmpty("", "param1");
            fail();
        } catch (NotNullOrEmptyRequiredException e) {
            assertEquals(e.getMessage(), "The value must not be null or empty : param1");
        }
    }

    @Test
    public void requiresNotNullOrWhiteSpace() {
        try {
            Requires.notNullOrWhiteSpace(" ", "param1");
            fail();
        } catch (NotNullOrWhiteSpaceRequiredException e) {
            assertEquals(e.getMessage(), "The value must not be null or white space : param1");
        }
    }

    @Test
    public void requiresIsFalse() {
        try {
            Requires.isFalse(true);
            fail();
        } catch (IsFalseRequiredException e) {
            assertEquals(e.getMessage(), "The value must be false");
        }
    }

    @Test
    public void requiresIsTrue() {
        try {
            Requires.isTrue(false);
            fail();
        } catch (IsTrueRequiredException e) {
            assertEquals(e.getMessage(), "The value must be true");
        }
    }
}