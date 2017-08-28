package com.etiennebaudoux.clipboardzanager.componentmodel.exceptions;

public class NotNullOrWhiteSpaceRequiredException extends RuntimeException {
    public NotNullOrWhiteSpaceRequiredException(String parameterName) {
        super("The value must not be null or white space : " + parameterName);
    }
}
