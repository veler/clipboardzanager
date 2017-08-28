package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import com.android.internal.util.Predicate;
import com.etiennebaudoux.clipboardzanager.componentmodel.exceptions.QueryableArrayListException;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.Set;

/**
 * Provides functionality to evaluate queries against a specific data source wherein the type of the data is known.
 *
 * @param <T> The type of the data in the data source.
 */
public class QueryableArrayList<T> extends ArrayList<T> {
    //region Fields

    private final String EmptyCollection = "Collection is empty.";

    //endregion

    //region Methods

    //region First

    /**
     * Returns the first element of a sequence.
     *
     * @return The first element in the specified sequence.
     * @throws QueryableArrayListException
     */
    public T first() throws QueryableArrayListException {
        if (size() == 0) {
            throw new QueryableArrayListException(EmptyCollection);
        }

        return get(0);
    }

    /**
     * Returns the first element in a sequence that satisfies a specified condition.
     *
     * @param predicate A function to test each element for a condition.
     * @return The first element in the sequence that passes the test in the specified predicate function.
     * @throws QueryableArrayListException
     */
    public T first(Predicate<T> predicate) throws QueryableArrayListException {
        if (size() == 0) {
            throw new QueryableArrayListException(EmptyCollection);
        }

        for (T item : this) {
            if (predicate.apply(item)) {
                return item;
            }
        }

        throw new QueryableArrayListException("No match.");
    }

    //endregion

    //region FirstOrDefault

    /**
     * Returns the first element of a sequence, or a default value if the sequence contains no elements.
     *
     * @return Null if source is empty; otherwise, the first element in source.
     */
    public T firstOrDefault() {
        if (any()) {
            return get(0);
        }

        return null;
    }

    /**
     * Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
     *
     * @param predicate A function to test each element for a condition.
     * @return Null if source is empty or if no element passes the test specified by predicate; otherwise, the first element in source that passes the test specified by predicate.
     */
    public T firstOrDefault(Predicate<T> predicate) {
        for (T item : this) {
            if (predicate.apply(item)) {
                return item;
            }
        }

        return null;
    }

    //endregion

    //region Last

    /**
     * Returns the last element of a sequence..
     *
     * @return The last element in the specified sequence.
     * @throws QueryableArrayListException
     */
    public T last() throws QueryableArrayListException {
        if (size() == 0) {
            throw new QueryableArrayListException(EmptyCollection);
        }

        return get(size() - 1);
    }

    //endregion

    //region Single

    /**
     * Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
     *
     * @return The single element of the input sequence.
     * @throws QueryableArrayListException
     */
    public T single() throws QueryableArrayListException {
        if (size() == 1) {
            return get(0);
        }

        throw new QueryableArrayListException("The collection is empty or there are more than one item.");
    }

    /**
     * Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
     *
     * @param predicate A function to test an element for a condition.
     * @return The single element of the input sequence that satisfies a condition.
     * @throws QueryableArrayListException
     */
    public T single(Predicate<T> predicate) throws QueryableArrayListException {
        if (size() == 0) {
            throw new QueryableArrayListException(EmptyCollection);
        }

        T result = null;
        for (T item : this) {
            if (predicate.apply(item)) {
                if (result != null) {
                    throw new QueryableArrayListException("More than one item match this predicate.");
                }
                result = item;
            }
        }

        if (result == null) {
            throw new QueryableArrayListException("No match.");
        }

        return result;
    }

    //endregion

    //region SingleOrDefault

    /**
     * Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
     *
     * @return The single element of the input sequence, or null if the sequence contains no elements.
     * @throws QueryableArrayListException
     */
    public T singleOrDefault() throws QueryableArrayListException {
        if (size() == 0) {
            return null;
        } else if (size() == 1) {
            return get(0);
        }

        throw new QueryableArrayListException("The collection have more than one item.");
    }

    /**
     * Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
     *
     * @param predicate A function to test an element for a condition.
     * @return The single element of the input sequence, or null if the sequence contains no elements.
     * @throws QueryableArrayListException
     */
    public T singleOrDefault(Predicate<T> predicate) throws QueryableArrayListException {
        if (size() == 0) {
            return null;
        }

        T result = null;
        for (T item : this) {
            if (predicate.apply(item)) {
                if (result != null) {
                    throw new QueryableArrayListException("More than one item match this predicate.");
                }
                result = item;
            }
        }

        if (result == null) {
            throw new QueryableArrayListException("No match.");
        }

        return result;
    }

    //endregion

    //region Any

    /**
     * Determines whether a sequence contains any elements.
     *
     * @return true if the source sequence contains any elements; otherwise, false.
     */
    public boolean any() {
        return size() > 0;
    }

    /**
     * Determines whether any element of a sequence satisfies a condition.
     *
     * @param predicate A function to test an element for a condition.
     * @return true if any elements in the source sequence pass the test in the specified predicate; otherwise, false.
     */
    public boolean any(Predicate<T> predicate) {
        for (T item : this) {
            if (predicate.apply(item)) {
                return true;
            }
        }

        return false;
    }

    //endregion

    //region All

    /**
     * Determines whether all elements of a sequence satisfy a condition.
     *
     * @param predicate A function to test an element for a condition.
     * @return true if every element of the source sequence passes the test in the specified predicate, or if the sequence is empty; otherwise, false.
     */
    public boolean all(Predicate<T> predicate) {
        for (T item : this) {
            if (!predicate.apply(item)) {
                return false;
            }
        }

        return true;
    }

    //endregion

    //region Where

    /**
     * Filters a sequence of values based on a predicate.
     *
     * @param predicate A function to test an element for a condition.
     * @return A {@link QueryableArrayList<T>} that contains elements from the input sequence that satisfy the condition.
     */
    public QueryableArrayList<T> where(Predicate<T> predicate) {
        QueryableArrayList<T> result = new QueryableArrayList<T>();

        for (T item : this) {
            if (predicate.apply(item)) {
                result.add(item);
            }
        }

        return result;
    }

    //endregion

    //region OfType

    /**
     * Filters the elements of an {@link QueryableArrayList} based on a specified type.
     *
     * @param type The type to filter the elements of the sequence on.
     * @param <U>  The type to filter the elements of the sequence on.
     * @return A {@link QueryableArrayList<U>} that contains elements from the input sequence of type {@link U}.
     */
    public <U> QueryableArrayList<U> ofType(Class<U> type) {
        QueryableArrayList<U> result = new QueryableArrayList<U>();

        for (T item : this) {
            if (type.isAssignableFrom(item.getClass())) {
                result.add(type.cast(item));
            }
        }

        return result;
    }

    //endregion

    //region Skip

    /**
     * Bypasses a specified number of elements in a sequence and then returns the remaining elements.
     *
     * @param count The number of elements to skip before returning the remaining elements.
     * @return A {@link QueryableArrayList<T>} that contains the elements that occur after the specified index in the input sequence.
     */
    public QueryableArrayList<T> skip(int count) {
        QueryableArrayList<T> result = new QueryableArrayList<T>();

        if (size() < count) {
            return result;
        }

        int i = count;
        while (i < size()) {
            result.add(get(i));
            i++;
        }

        return result;
    }

    //endregion

    //region Union

    /**
     * Produces the set union of two sequences by using the default equality comparer.
     *
     * @param source A {@link QueryableArrayList<T>} whose distinct elements form the current set for the union.
     * @return A {@link QueryableArrayList<T>} that contains the elements from both input sequences, excluding duplicates.
     */
    public QueryableArrayList<T> union(QueryableArrayList<T> source) {
        Requires.notNull(source, "source");
        Set<T> set = new HashSet<>();

        set.addAll(source);
        set.addAll(this);

        QueryableArrayList<T> result = new QueryableArrayList<>();
        result.addAll(set);
        return result;
    }

    //endregion

    //endregion
}
