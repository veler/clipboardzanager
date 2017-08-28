package com.etiennebaudoux.clipboardzanager.componentmodel.exceptions;

public class NotAuthenticatedException extends RuntimeException {
    public NotAuthenticatedException() {
        super("User not authenticated.");
    }
}
