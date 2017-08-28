package com.etiennebaudoux.clipboardzanager.models;

import com.google.gson.annotations.SerializedName;

/**
 * Represents the information about a data from the clipboard.
 */
public class DataEntry extends DataEntryBase {
    //region Fields

    @SerializedName("Icon")
    private String _icon;

    //endregion

    //region Properties

    //region Thumbnail

    @SerializedName("Thumbnail")
    private Thumbnail _thumbnail;

    /**
     * Gets the linked {@link Thumbnail}
     *
     * @return
     */
    public Thumbnail getThumbnail() {
        return _thumbnail;
    }

    /**
     * Sets the linked {@link Thumbnail}
     *
     * @param value
     */
    public void setThumbnail(Thumbnail value) {
        _thumbnail = value;
    }

    //endregion

    //region IsCut

    @SerializedName("IsCut")
    private boolean _isCut;

    //endregion

    //region CanSynchronize

    @SerializedName("CanSynchronize")
    private boolean _canSynchronize;

    /**
     * Gets a value that defines whether this data can be synchronized in the cloud or not.
     *
     * @return
     */
    public boolean canSynchronize() {
        return _canSynchronize;
    }

    /**
     * Sets a value that defines whether this data can be synchronized in the cloud or not.
     *
     * @param value
     */
    public void setCanSynchronize(boolean value) {
        _canSynchronize = value;
    }

    //endregion

    //region IconIsFromWindowStore

    @SerializedName("IconIsFromWindowStore")
    private boolean _iconIsFromWindowStore;

    /**
     * Gets a value that defines whether the icon comes from a Windows Store app or not.
     *
     * @return
     */
    public boolean isIconIsFromWindowStore() {
        return _iconIsFromWindowStore;
    }

    /**
     * Sets a value that defines whether the icon comes from a Windows Store app or not.
     *
     * @param value
     */
    public void setIconIsFromWindowStore(boolean value) {
        _iconIsFromWindowStore = value;
    }

    //endregion

    //endregion
}