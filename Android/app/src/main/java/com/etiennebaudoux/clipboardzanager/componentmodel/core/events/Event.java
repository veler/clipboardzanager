package com.etiennebaudoux.clipboardzanager.componentmodel.core.events;

import java.util.ArrayList;

/**
 * Provides a basic implementation for the event interface.
 *
 * @param <T> The type that is used to hold the event information.
 */
public class Event<T extends EventArgs> {
    //region Fields

    private final ArrayList<EventHandler<T>> _eventHandlers = new ArrayList<>();

    //endregion

    //region Methods

    /**
     * Adds and event handler to this event that is called when the event is fired.
     *
     * @param handler The handler to be added.
     */
    public void addHandler(EventHandler<T> handler) {
        _eventHandlers.add(handler);
    }

    /**
     * Removes an event handler for that event.
     *
     * @param handler The handler to be removed.
     */
    public void removeHandler(EventHandler<T> handler) {
        _eventHandlers.remove(handler);
    }

    /**
     * Fires the event and causes all the registered event handlers to be called.
     *
     * @param sender The sender of the event.
     * @param args   The information about the event.
     */
    public void invoke(Object sender, T args) {
        for (EventHandler<T> handler : _eventHandlers) {
            handler.handle(sender, args);
        }
    }

    //endregion
}