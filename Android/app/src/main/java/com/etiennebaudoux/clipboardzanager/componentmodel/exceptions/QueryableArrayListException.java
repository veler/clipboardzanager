package com.etiennebaudoux.clipboardzanager.componentmodel.exceptions;

public class QueryableArrayListException extends RuntimeException {
    public QueryableArrayListException(String message) {
        super("Unable to complete the query : " + message);
    }
}
