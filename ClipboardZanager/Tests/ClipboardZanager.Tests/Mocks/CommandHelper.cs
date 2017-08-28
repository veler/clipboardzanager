using System.Windows.Input;

namespace ClipboardZanager.Tests.Mocks
{
    internal static class CommandHelper
    {
        internal static bool CheckBeginExecute(this ICommand command, object argument = null)
        {
            return CheckBeginExecuteCommand(command, argument);
        }

        internal static bool CheckBeginExecuteCommand(ICommand command, object argument)
        {
            var canExecute = false;
            lock (command)
            {
                canExecute = command.CanExecute(argument);
                if (canExecute)
                {
                    command.Execute(argument);
                }
            }

            return canExecute;
        }
    }
}
