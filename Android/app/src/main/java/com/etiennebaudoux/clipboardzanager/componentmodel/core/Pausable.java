package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import java.io.Closeable;

/**
 * Provides a set of methods designed to pause something and resume it.
 */
public interface Pausable extends Closeable {
    /**
     * Put the process in pause
     */
    void pause();

    /**
     * Resume the process
     */
    void resume();
}
