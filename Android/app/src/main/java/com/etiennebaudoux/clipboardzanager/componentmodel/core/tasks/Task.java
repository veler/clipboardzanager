package com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks;

import android.os.AsyncTask;
import android.os.Build;

import java.util.concurrent.Callable;

/**
 * Provides an asynchronous task that can return a result if we wait for its end.
 */
public class Task<T> {
    //region Fields

    private TaskBase<T> _task;

    //endregion

    //region Constructors

    /**
     * Initialize a new instance of the {@link Task} class.
     *
     * @param func the function to run asynchronously.
     */
    public Task(Callable<T> func) {
        _task = new TaskBase<T>(func);
    }

    //endregion

    //region Methods

    /**
     * Executes the task.
     */
    public void start() {
        if (_task.getStatus() == AsyncTask.Status.PENDING) {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB) {
                _task.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
            } else {
                _task.execute();
            }
        } else {
            throw new RuntimeException("The task has already been started.");
        }
    }

    /**
     * Waits if necessary for the computation to complete, and then retrieves its result.
     *
     * @return Returns the result of the task.
     */
    public T await() {
        if (_task.getStatus() == AsyncTask.Status.PENDING) {
            start();
        }

        try {
            return _task.get().getResult();
        } catch (RuntimeException exception) {
            throw exception;
        } catch (Exception exception) {
            throw new RuntimeException(exception);
        }
    }

    /**
     * Returns the current status of this task.
     *
     * @return The current status of this task.
     */
    public AsyncTask.Status getStatus() {
        return _task.getStatus();
    }

    //endregion
}
