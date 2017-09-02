package com.etiennebaudoux.clipboardzanager.models;

import com.etiennebaudoux.clipboardzanager.enums.DataEntryStatus;
import com.google.gson.annotations.SerializedName;

import java.io.Serializable;
import java.util.UUID;

/**
 * Represents the status of a data entry.
 */
public class DataEntryCache implements Serializable {
    //region Properties

    //region Identifier

    @SerializedName("Identifier")
    private UUID _identifier;

    /**
     * Gets the data entry identifier
     *
     * @return
     */
    public UUID getIdentifier() {
        return _identifier;
    }

    /**
     * Sets the data entry identifier
     *
     * @param value
     */
    public void setIdentifier(UUID value) {
        _identifier = value;
    }

    //endregion

    //region IsFavorite

    @SerializedName("Status")
    private @DataEntryStatus int _status;

    /**
     * Gets a value that defines whether this data is a favorite or not.
     *
     * @return @DataEntryStatus int
     */
    public @DataEntryStatus int getStatus() {
        return _status;
    }

    /**
     * Sets a value that defines whether this data is a favorite or not.
     *
     * @param @DataEntryStatus int
     */
    public void setStatus(@DataEntryStatus int value) {
        _status = value;
    }

    //endregion

    //endregion
}
