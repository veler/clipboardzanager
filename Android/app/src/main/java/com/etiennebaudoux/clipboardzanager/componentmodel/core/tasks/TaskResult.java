package com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks;

/**
 * Represents the result of an asynchronous task.
 *
 * @param <T> The expected type result.
 */
public class TaskResult<T> {
    //region Fields

    private T result;
    private Exception error;

    //endregion

    //region Constructors

    /**
     * Initialize a new instance of the {@link TaskResult} class.
     *
     * @param result The result of the task.
     */
    public TaskResult(T result) {
        super();
        this.result = result;
    }

    /**
     * Initialize a new instance of the {@link TaskResult} class.
     *
     * @param error An exception that thrown during the task.
     */
    public TaskResult(Exception error) {
        super();
        this.error = error;
    }

    //endregion

    //region Methods

    /**
     * Gets the result of the task.
     *
     * @return The result of the task. If an error has been thrown, a {@link RuntimeException} will be thrown.
     */
    public T getResult() {
        if (error != null) {
            throw new RuntimeException(error);
        }

        return result;
    }

    //endregion
}
