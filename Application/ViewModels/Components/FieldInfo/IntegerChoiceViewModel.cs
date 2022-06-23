using ReactiveUI;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.Application.ViewModels.Components.FieldInfo
{
    internal class IntegerChoiceViewModel : FieldInfoViewModel
    {
        public IntegerChoiceFieldInfo FieldInfo { get; }
        public override string FieldName => FieldInfo.Name;
        public override string DisplayName => FieldInfo.ReadableName;
        public override string? Tooltip => FieldInfo.Tooltip;
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
            FieldInfo = new IntegerChoiceFieldInfo("TestField", "Test Field", new int[] { 1, 3, 5, 7, 10 }, "Test Field");
            this.value = 5;
        }

        public IntegerChoiceViewModel(IntegerChoiceFieldInfo fieldInfo, int value)
        {
            FieldInfo = fieldInfo;
            this.value = value;
        }

        public override object GetValue() => value;
    }
}
