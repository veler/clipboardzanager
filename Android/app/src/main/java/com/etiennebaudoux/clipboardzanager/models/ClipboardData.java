package com.etiennebaudoux.clipboardzanager.models;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.Requires;

import java.util.Date;

/**
 * Represents the data that comes from the clipboard under Android.
 */
public class ClipboardData {
    //region Properties

    //region Data

    private String _data;

    /**
     * Gets the data from the clipboard.
     *
     * @return The data from the clipboard.
     */
    public String getData() {
        return _data;
    }

    /**
     * Sets the data from the clipboard.
     *
     * @param value The data from the clipboard.
     */
    private void setData(String value) {
        _data = value;
    }

    //endregion

    //region Date

    private Date _date;

    /**
     * Gets the date that corresponds to when the data have been intercepted.
     *
     * @return The date that corresponds to when the data have been intercepted.
     */
    public Date getDate() {
        return _date;
    }

    /**
     * Sets the date that corresponds to when the data have been intercepted.
     *
     * @param value The date that corresponds to when the data have been intercepted.
     */
    private void setDate(Date value) {
        _date = value;
    }

    //endregion

    //endregion

    //region Constructors

    /**
     * Initialize a new instance of the {@link ClipboardData} class.
     *
     * @param data The data from the clipboard.
     * @param date The date that corresponds to when the data have been intercepted.
     */
    public ClipboardData(String data, Date date) {
        Requires.notNull(data, "data");
        Requires.notNull(date, "date");

        setData(data);
        setDate(date);
    }

    //endregion
}
