using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interactivity;

namespace ClipboardZanager.ComponentModel.UI
{
    /// <summary>
    /// Sets the designated property to the supplied value. TargetObject
    /// optionally designates the object on which to set the property. If
    /// TargetObject is not supplied then the property is set on the object
    /// to which the trigger is attached.
    /// </summary>
    public class SetPropertyAction : TriggerAction<FrameworkElement>
    {
        #region Properties

        /// <summary>
        /// The property to be executed in response to the trigger.
        /// </summary>
        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register("PropertyName", typeof(string), typeof(SetPropertyAction));

        /// <summary>
        /// The property to be executed in response to the trigger.
        /// </summary>
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        /// <summary>
        /// The value to set the property to.
        /// </summary>
        public static readonly DependencyProperty PropertyValueProperty = DependencyProperty.Register("PropertyValue", typeof(object), typeof(SetPropertyAction));

        /// <summary>
        /// The value to set the property to.
        /// </summary>
        public object PropertyValue
        {
            get { return GetValue(PropertyValueProperty); }
            set { SetValue(PropertyValueProperty, value); }
        }

        /// <summary>
        /// Specifies the object upon which to set the property.
        /// </summary>
        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register("TargetObject", typeof(object), typeof(SetPropertyAction));

        /// <summary>
        /// Specifies the object upon which to set the property.
        /// </summary>
        public object TargetObject
        {
            get { return GetValue(TargetObjectProperty); }
            set { SetValue(TargetObjectProperty, value); }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void Invoke(object parameter)
        {
            var target = TargetObject ?? AssociatedObject;
            var propertyInfo = target.GetType().GetProperty(PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

            propertyInfo.SetValue(target, Convert.ChangeType(PropertyValue, propertyInfo.PropertyType));
        }

        #endregion

    }

}
