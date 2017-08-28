using ClipboardZanager.Shared.Core;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClipboardZanager.ComponentModel.UI
{
    /// <summary>
    /// Provides a set of methods used to dynamically get information on the visual tree.
    /// </summary>
    internal static class VisualHelper
    {
        /// <summary>
        /// Find all the children <see cref="DependencyObject"/> of a specific type.
        /// </summary>
        /// <typeparam name="T">The <see cref="DependencyObject"/> to search</typeparam>
        /// <param name="depObj">The root from where the search must start</param>
        /// <returns>A <see cref="IEnumerable{T}"/> that contains the list of <see cref="DependencyObject"/> that match the specific type.</returns>
        internal static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            Requires.NotNull(depObj, nameof(depObj));

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        /// <summary>
        /// Give the focus to the first focusable <see cref="UIElement"/>.
        /// </summary>
        /// <param name="control">The <see cref="UIElement"/> from where it should starts the search.</param>
        internal static void InitializeFocus(UIElement control)
        {
            if (control == null)
            {
                return;
            }

            var firstFocusable = FindVisualChildren<UIElement>(control).FirstOrDefault(c =>
            {
                if (c.Focusable)
                {
                    return c.Focusable;
                }

                var tabIndex = c.GetType().GetProperties().FirstOrDefault(prop => prop.Name == "TabIndex");
                if (tabIndex != null)
                {
                    return (int)tabIndex.GetValue(c) == 0;
                }

                return false;
            });

            if (firstFocusable != null)
            {
                firstFocusable.Focus();
                Keyboard.Focus(firstFocusable);
            }
        }
    }
}
