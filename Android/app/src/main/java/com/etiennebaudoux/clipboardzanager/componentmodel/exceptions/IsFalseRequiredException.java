package com.etiennebaudoux.clipboardzanager.componentmodel.exceptions;

public class IsFalseRequiredException extends RuntimeException {
    public IsFalseRequiredException() {
        super("The value must be false");
    }
}
