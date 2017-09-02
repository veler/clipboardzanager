package com.etiennebaudoux.clipboardzanager.enums;

import android.support.annotation.IntDef;

import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;

@IntDef({DataEntryStatus.ADDED, DataEntryStatus.DID_NOT_CHANGED, DataEntryStatus.DELETED})
@Retention(RetentionPolicy.SOURCE)
public @interface DataEntryStatus
{
    /**
     * The data has been added locally and has not been synchronized yet.
     */
    int ADDED = 0;

    /**
     * The data has been synchronized with the cloud and did not changed since the last synchronization.
     */
    int DID_NOT_CHANGED = 1;

    /**
     * The data has been removed locally and has not been synchronized yet.
     */
    int DELETED = 2;
}
