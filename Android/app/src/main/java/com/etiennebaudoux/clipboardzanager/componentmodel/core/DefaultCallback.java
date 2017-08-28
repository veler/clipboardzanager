package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import android.util.Log;

import com.onedrive.sdk.concurrency.ICallback;
import com.onedrive.sdk.core.ClientException;

/**
 * A default callback that logs errors
 *
 * @param <T> The type returned by this callback
 */
public class DefaultCallback<T> implements ICallback<T> {
    /**
     * The exception text for not implemented runtime exceptions
     */
    private static final String SUCCESS_MUST_BE_IMPLEMENTED = "Success must be implemented";

    @Override
    public void success(final T t) {
        throw new RuntimeException(SUCCESS_MUST_BE_IMPLEMENTED);
    }

    @Override
    public void failure(final ClientException error) {
        if (error != null) {
            Log.e(getClass().getSimpleName(), error.getMessage());
            throw new RuntimeException(error);
        }
    }
}
