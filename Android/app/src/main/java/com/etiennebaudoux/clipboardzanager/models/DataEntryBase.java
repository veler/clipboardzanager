package com.etiennebaudoux.clipboardzanager.models;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;
import com.google.gson.annotations.SerializedName;

import java.io.Serializable;
import java.util.Date;
import java.util.UUID;

/**
 * Represents the basic information of a data entry locally or on a cloud server.
 */
public class DataEntryBase implements Serializable {
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

    //region DataIdentifiers

    @SerializedName("DataIdentifiers")
    private QueryableArrayList<DataIdentifier> _dataIdentifiers;

    /**
     * Gets the list of identifiers for a clipboard data
     *
     * @return
     */
    public QueryableArrayList<DataIdentifier> getDataIdentifiers() {
        return _dataIdentifiers;
    }

    /**
     * Sets the list of identifiers for a clipboard data
     *
     * @param value
     */
    public void setDataIdentifiers(QueryableArrayList<DataIdentifier> value) {
        _dataIdentifiers = value;
    }

    //endregion

    //region Date

    @SerializedName("Date")
    private Date _date;

    /**
     * Gets the {@link Date} that defines when the data has been copied.
     *
     * @return
     */
    public Date getDate() {
        return _date;
    }

    /**
     * Sets the {@link Date} that defines when the data has been copied.
     *
     * @param value
     */
    public void setDate(Date value) {
        _date = value;
    }

    //endregion

    //region IsFavorite

    @SerializedName("IsFavorite")
    private boolean _isFavorite;

    /**
     * Gets a value that defines whether this data is a favorite or not.
     *
     * @return
     */
    public boolean isFavorite() {
        return _isFavorite;
    }

    /**
     * Sets a value that defines whether this data is a favorite or not.
     *
     * @param value
     */
    public void setIsFavorite(boolean value) {
        _isFavorite = value;
    }

    //endregion

    //endregion
}
