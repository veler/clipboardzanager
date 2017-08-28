package com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks;

import android.os.AsyncTask;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.robolectric.RobolectricTestRunner;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

@RunWith(RobolectricTestRunner.class)
public class TaskTest {
    @Test
    public void taskRunner() {
        Task<Boolean> task = new Task<Boolean>(() -> {
            Thread.sleep(100);
            return true;
        });

        try {
            Boolean result = task.await();
            assertTrue(result);
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void taskRunnerAwait() {
        Task<Boolean> task = new Task<Boolean>(() -> {
            Thread.sleep(100);
            Task<Boolean> task2 = new Task<Boolean>(() -> {
                Thread.sleep(100);
                return true;
            });
            return task2.await();
        });

        try {
            assertEquals(task.await(), true);
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void taskRunnerAsync() {
        Task<Boolean> task = new Task<Boolean>(() -> {
            Thread.sleep(100);
            Task<Boolean> task2 = new Task<Boolean>(() -> {
                Thread.sleep(100);
                return true;
            });
            return task2.await();
        });

        try {
            task.start();
            Thread.sleep(100);
            assertEquals(task.getStatus(), AsyncTask.Status.RUNNING);
            Thread.sleep(200);
            assertEquals(task.getStatus(), AsyncTask.Status.FINISHED);
            assertEquals(task.await(), true);
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void taskRunnerException() {
        Task<Boolean> task = new Task<Boolean>(() -> {
            Thread.sleep(100);
            Task<Boolean> task2 = new Task<Boolean>(() -> {
                Thread.sleep(100);
                throw new RuntimeException("The task failed");
            });
            return task2.await();
        });

        try {
            Boolean result = task.await();
            fail();
        } catch (Exception e) {
            assertEquals(e.getMessage(), "java.lang.RuntimeException: java.lang.RuntimeException: The task failed");
        }
    }

    @Test
    public void taskRunnerExceptionAsync() {
        Task<Boolean> task = new Task<Boolean>(() -> {
            Thread.sleep(100);
            Task<Boolean> task2 = new Task<Boolean>(() -> {
                Thread.sleep(100);
                throw new RuntimeException("The task failed");
            });
            return task2.await();
        });

        try {
            task.start();
            Thread.sleep(100);
            assertEquals(task.getStatus(), AsyncTask.Status.RUNNING);
            Thread.sleep(200);
            assertEquals(task.getStatus(), AsyncTask.Status.FINISHED);
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }
}
