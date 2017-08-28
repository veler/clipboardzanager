package com.etiennebaudoux.clipboardzanager.componentmodel.exceptions;

public class NotNullOrEmptyRequiredException extends RuntimeException {
    public NotNullOrEmptyRequiredException(String parameterName) {
        super("The value must not be null or empty : " + parameterName);
    }
}
