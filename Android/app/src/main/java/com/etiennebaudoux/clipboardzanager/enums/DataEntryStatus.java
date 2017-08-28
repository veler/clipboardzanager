package com.etiennebaudoux.clipboardzanager.enums;

public enum DataEntryStatus
{
    /**
     * The data has been added locally and has not been synchronized yet.
     */
    Added,

    /**
     * The data has been synchronized with the cloud and did not changed since the last synchronization.
     */
    DidNotChanged,

    /**
     * The data has been removed locally and has not been synchronized yet.
     */
    Deleted
}
