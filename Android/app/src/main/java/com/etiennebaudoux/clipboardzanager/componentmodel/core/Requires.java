package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.IsFalseRequiredException;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.IsTrueRequiredException;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotNullOrEmptyRequiredException;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotNullOrWhiteSpaceRequiredException;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.NotNullRequiredException;

/**
 * Provides a set of methods that can be used to validate data.
 */

public final class Requires {
    /**
     * Throws an exception if the specified parameter's value is null.
     *
     * @param value         The value to test.
     * @param parameterName The name of the parameter to include in any thrown exception.
     */
    public static void notNull(Object value, String parameterName) {
        if (value == null) {
            throw new NotNullRequiredException(parameterName);
        }
    }

    /**
     * Throws an exception if the specified parameter's value is null or empty.
     *
     * @param value         The value to test.
     * @param parameterName The name of the parameter to include in any thrown exception.
     */
    public static void notNullOrEmpty(String value, String parameterName) {
        if (StringUtils.isNullOrEmpty(value)) {
            throw new NotNullOrEmptyRequiredException(parameterName);
        }
    }

    /**
     * Throws an exception if the specified parameter's value is null, empty, or whitespace.
     *
     * @param value         The value to test.
     * @param parameterName The name of the parameter to include in any thrown exception.
     */
    public static void notNullOrWhiteSpace(String value, String parameterName) {
        if (StringUtils.isEmptyOrWhiteSpace(value)) {
            throw new NotNullOrWhiteSpaceRequiredException(parameterName);
        }
    }

    /**
     * Throws an exception if the specified parameter's value is not false.
     *
     * @param value The value to test.
     */
    public static void isFalse(boolean value) {
        if (value) {
            throw new IsFalseRequiredException();
        }
    }

    /**
     * Throws an exception if the specified parameter's value is not true.
     *
     * @param value The value to test.
     */
    public static void isTrue(boolean value) {
        if (!value) {
            throw new IsTrueRequiredException();
        }
    }
}
