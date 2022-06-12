using ReactiveUI;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.Application.ViewModels.Components.FieldInfo
{
    internal class BoolViewModel : FieldInfoViewModel
    {
        public BoolFieldInfo FieldInfo { get; }
        public override string FieldName => FieldInfo.Name;
        public override string DisplayName => FieldInfo.ReadableName;

        private bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set => this.RaiseAndSetIfChanged(ref isChecked, value);
        }

        public BoolViewModel()
        {
            this.IsChecked = true;
            FieldInfo = new BoolFieldInfo("TestField", "Test Field");
        }

        public BoolViewModel(BoolFieldInfo boolFieldInfo, bool value)
        {
            FieldInfo = boolFieldInfo;
            isChecked = value;
        }

        public override object GetValue() => isChecked;
    }
}
