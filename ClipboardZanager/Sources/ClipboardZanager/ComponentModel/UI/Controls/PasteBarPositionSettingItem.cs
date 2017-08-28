using System.Windows;
using System.Windows.Controls;
using ClipboardZanager.ComponentModel.Enums;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Represents a paste bar position in the setting window.
    /// </summary>
    internal class PasteBarPositionSettingItem : ComboBoxItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets the paste bar position.
        /// </summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(PasteBarPosition), typeof(PasteBarPositionSettingItem));

        /// <summary>
        /// Gets or sets the paste bar position.
        /// </summary>
        public PasteBarPosition Position
        {
            get { return (PasteBarPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        #endregion
    }
}
