package com.etiennebaudoux.clipboardzanager.models;

import com.google.gson.annotations.SerializedName;

import java.io.Serializable;
import java.util.UUID;

/**
 * Represents the information used to identify a part of a clipboard data.
 */
public class DataIdentifier implements Serializable {
    //region Properties

    //region Identifier

    @SerializedName("Identifier")
    private UUID _identifier;

    /**
     * Gets the data file identifier
     *
     * @return
     */
    public UUID getIdentifier() {
        return _identifier;
    }

    /**
     * Sets the data file identifier
     *
     * @param value
     */
    public void setIdentifier(UUID value) {
        _identifier = value;
    }

    //endregion

    //region FormatName

    @SerializedName("FormatName")
    private String _formatName;

    /**
     * Gets the name of the clipboard data format
     *
     * @return
     */
    public String getFormatName() {
        return _formatName;
    }

    /**
     * Sets the name of the clipboard data format
     *
     * @param value
     */
    public void setFormatName(String value) {
        _formatName = value;
    }

    //endregion

    //endregion
}
