package com.etiennebaudoux.clipboardzanager.componentmodel.core.events;

/**
 * The event handler interface
 *
 * @param <T> The type that is used to hold the event information.
 */
@FunctionalInterface
public interface EventHandler<T extends EventArgs> {
    void handle(Object sender, T args);
}