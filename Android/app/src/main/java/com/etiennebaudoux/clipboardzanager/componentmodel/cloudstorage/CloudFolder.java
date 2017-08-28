package com.etiennebaudoux.clipboardzanager.componentmodel.cloudstorage;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.QueryableArrayList;

/**
 * Represents a file in the cloud provided by a CloudStorageProvider.
 */
public class CloudFolder {
    //region Properties

    //region Name

    private String _name;

    /**
     * Gets the name of the folder.
     *
     * @return The name of the folder.
     */
    public String getName() {
        return _name;
    }

    /**
     * Sets the name of the folder.
     *
     * @param name The name of the folder.
     */
    public void setName(String name) {
        _name = name;
    }

    //endregion

    //region FullPath

    private String _fullPath;

    /**
     * Gets the full path to the folder.
     *
     * @return The full path to the folder.
     */
    public String getFullPath() {
        return _fullPath;
    }

    /**
     * Sets the full path to the folder.
     *
     * @param fullPath the full path to the folder.
     */
    public void setFullPath(String fullPath) {
        _fullPath = fullPath;
    }

    //endregion

    //region Size

    private long _size;

    /**
     * Gets the size of the folder's content.
     *
     * @return The size of the folder's content.
     */
    public long getSize() {
        return _size;
    }

    /**
     * Sets the size of the folder's content.
     *
     * @param size The size of the folder's content.
     */
    public void setSize(long size) {
        _size = size;
    }

    //endregion

    //region Files

    private QueryableArrayList<CloudFile> _files;

    /**
     * Gets the list of files inside the folder.
     *
     * @return The list of files inside the folder.
     */
    public QueryableArrayList<CloudFile> getFiles() {
        return _files;
    }

    /**
     * Sets the list of files inside the folder.
     *
     * @param files The list of files inside the folder.
     */
    public void setFiles(QueryableArrayList<CloudFile> files) {
        _files = files;
    }

    //endregion

    //endregion
}
