using ReactiveUI;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.Application.ViewModels.Components.FieldInfo
{
    internal class IntegerChoiceViewModel : ViewModelBase
    {
        public IntegerChoiceFieldInfo FieldInfo { get; }
        public string FieldName => FieldInfo.Name;
        public string DisplayName => FieldInfo.ReadableName;
        private int value;
        public int Value
        {
            get => value;
            set
            {
                this.RaiseAndSetIfChanged(ref this.value, value);
            }
        }
        public int[] PossibleValues => FieldInfo.PossibleValues;

        public IntegerChoiceViewModel()
        {
            FieldInfo = new IntegerChoiceFieldInfo("TestField", "Test Field", new int[] { 1, 3, 5, 7, 10 });
            this.value = 5;
        }

        public IntegerChoiceViewModel(IntegerChoiceFieldInfo fieldInfo, int value)
        {
            FieldInfo = fieldInfo;
            this.value = value;
        }
    }
}
