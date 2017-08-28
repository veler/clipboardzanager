package com.etiennebaudoux.clipboardzanager.models;

import com.google.gson.annotations.SerializedName;

import java.io.Serializable;

/**
 * Represents a link in a {@link Thumbnail}
 */
public class Link implements Serializable {
    //region Properties

    //region Uri

    @SerializedName("Uri")
    private String _uri;

    /**
     * Gets the uri of the link.
     *
     * @return
     */
    public String getUri() {
        return _uri;
    }

    /**
     * Sets the uri of the link.
     *
     * @param value
     */
    public void setUri(String value) {
        _uri = value;
    }

    //endregion

    //region Title

    @SerializedName("Title")
    private String _title;

    /**
     * Gets the title of the page.
     *
     * @return
     */
    public String getTitle() {
        return _title;
    }

    /**
     * Sets the title of the page.
     *
     * @param value
     */
    public void setTitle(String value) {
        _title = value;
    }

    //endregion

    //endregion
}
