using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SMW_ML.ViewModels;
using SMW_ML.Views;
using System;

namespace SMW_ML
{
    internal class ViewLocator : IDataTemplate
    {

        private static MainWindow? window;

        public IControl Build(object data)
        {
            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }

        public static MainWindow GetMainWindow()
        {
            return window!;
        }

        public static void SetInstance(MainWindow window)
        {
            ViewLocator.window = window;
        }
    }
}
