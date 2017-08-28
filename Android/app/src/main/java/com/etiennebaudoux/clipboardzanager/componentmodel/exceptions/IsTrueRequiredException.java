package com.etiennebaudoux.clipboardzanager.componentmodel.exceptions;

public class IsTrueRequiredException extends RuntimeException {
    public IsTrueRequiredException() {
        super("The value must be true");
    }
}
