package com.etiennebaudoux.clipboardzanager.models;

import com.etiennebaudoux.clipboardzanager.enums.ThumbnailDataType;
import com.google.gson.annotations.SerializedName;

import java.io.Serializable;

/**
 * Represents a thumbnail of a data of the clipboard.
 */
public class Thumbnail implements Serializable {
    //region Properties

    //region Value

    @SerializedName("Value")
    private String _value;

    /**
     * Gets the value of the thumbnail.
     *
     * @return
     */
    public String getValue() {
        return _value;
    }

    /**
     * Sets the value of the thumbnail.
     *
     * @param value
     */
    public void setValue(String value) {
        _value = value;
    }

    //endregion

    //region Type

    @SerializedName("Type")
    private ThumbnailDataType _type;

    /**
     * Gets the type of the value.
     *
     * @return
     */
    public ThumbnailDataType getType() {
        return _type;
    }

    /**
     * Sets the type of the value.
     *
     * @param value
     */
    public void setType(ThumbnailDataType value) {
        _type = value;
    }

    //endregion

    //endregion
}
