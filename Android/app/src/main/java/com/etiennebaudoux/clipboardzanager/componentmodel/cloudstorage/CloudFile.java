package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage;

import java.util.Date;

/**
 * Represents a file in the cloud provided by a CloudStorageProvider.
 */
public class CloudFile {
    //region Properties

    //region Name

    private String _name;

    /**
     * Gets the name of the file.
     *
     * @return The name of the file.
     */
    public String getName() {
        return _name;
    }

    /**
     * Sets the name of the file.
     *
     * @param name The name of the file.
     */
    public void setName(String name) {
        _name = name;
    }

    //endregion

    //region FullPath

    private String _fullPath;

    /**
     * Gets the full path to the file.
     *
     * @return The full path to the file.
     */
    public String getFullPath() {
        return _fullPath;
    }

    /**
     * Sets the full path to the file.
     *
     * @param fullPath the full path to the file.
     */
    public void setFullPath(String fullPath) {
        _fullPath = fullPath;
    }

    //endregion

    //region LastModificationUtcDate

    private Date _lastModificationUtcDate;

    /**
     * Gets the date and time of last modification in Utc format on the server.
     *
     * @return The date and time of last modification in Utc format on the server.
     */
    public Date getLastModificationUtcDate() {
        return _lastModificationUtcDate;
    }

    /**
     * Sets the date and time of last modification in Utc format on the server.
     *
     * @param lastModificationUtcDate The date and time of last modification in Utc format on the server.
     */
    public void setLastModificationUtcDate(Date lastModificationUtcDate) {
        _lastModificationUtcDate = lastModificationUtcDate;
    }

    //endregion

    //endregion
}
