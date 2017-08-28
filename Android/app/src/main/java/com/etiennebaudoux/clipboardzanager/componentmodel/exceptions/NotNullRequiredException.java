package com.etiennebaudoux.clipboardzanager.componentmodel.exceptions;

public class NotNullRequiredException extends RuntimeException {
    public NotNullRequiredException(String parameterName) {
        super("The value must not be null : " + parameterName);
    }
}
