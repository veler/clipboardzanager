package com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks;

import android.os.AsyncTask;

import java.util.concurrent.Callable;

/**
 * Provides an asynchronous task that can return a result if we wait for its end.
 */
class TaskBase<T> extends AsyncTask<Void, Void, TaskResult<T>> {
    //region Fields

    private Callable<T> _func;

    //endregion

    //region Constructors

    /**
     * Initialize a new instance of the {@link TaskBase} class.
     *
     * @param func the function to run asynchronously.
     */
    public TaskBase(Callable<T> func) {
        _func = func;
    }

    //endregion

    //region Methods

    @Override
    protected TaskResult<T> doInBackground(Void... params) {
        try {
            return new TaskResult<T>(_func.call());
        } catch (Exception exception) {
            return new TaskResult<T>(exception);
        }
    }

    //endregion
}
