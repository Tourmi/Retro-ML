using System;

namespace Retro_ML.ViewModels
{
    internal class MainPageViewModel : ViewModelBase
    {
        public event Action? OnStartTrainingCalled;
        public event Action? OnOpenPlayMode;

        #region Strings
        public static string GreetingString => "Super Mario World - Machine Learning";
        public static string TrainingString => "Training";
        public static string PlayString => "Play mode";
        public static string OpenSettingsString => "Configuration";
        #endregion

        public void OpenTraining()
        {
            OnStartTrainingCalled?.Invoke();
        }

        public void OpenPlay()
        {
            OnOpenPlayMode?.Invoke();
        }

        public void OpenConfigSettings()
        {
            var configViewModel = new ConfigurationViewModel();
            configViewModel.ShowWindow(ViewLocator.GetMainWindow());
        }
    }
}
