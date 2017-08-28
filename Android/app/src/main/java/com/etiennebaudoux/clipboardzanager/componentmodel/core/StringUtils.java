package com.etiennebaudoux.clipboardzanager.componentmodel.core;

/**
 * Provides a set of methods designed to extend the Strings.
 */

public final class StringUtils {
    /**
     * Indicates whether the specified string is null or an empty string.
     *
     * @param input The {@link String} to test.
     * @return true if the value parameter is null or an empty string (""); otherwise, false.
     */
    public static boolean isNullOrEmpty(String input) {
        return input == null || input.isEmpty();
    }

    /**
     * Indicates whether a specified string is null, empty, or consists only of white-space characters.
     *
     * @param input The {@link String} to test.
     * @return true if the value parameter is null or empty, or if value consists exclusively of white-space characters.
     */
    public static boolean isEmptyOrWhiteSpace(String input) {
        return input == null || input.trim().isEmpty();
    }
}
