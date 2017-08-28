using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ClipboardZanager.ComponentModel.UI
{
    public class ForceTouchCaptureBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.TouchDown += OnTouchDown;
        }

        void OnTouchDown(object sender, TouchEventArgs e)
        {
            e.TouchDevice.Capture(AssociatedObject);
            e.Handled = true;
        }

    }
}
