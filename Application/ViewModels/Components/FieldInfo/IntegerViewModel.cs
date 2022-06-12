using ReactiveUI;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.Application.ViewModels.Components.FieldInfo
{
    internal class IntegerViewModel : FieldInfoViewModel
    {
        public IntegerFieldInfo FieldInfo { get; }
        public override string FieldName => FieldInfo.Name;
        public override string DisplayName => FieldInfo.ReadableName;

        private int value;
        public int Value
        {
            get => value;
            set
            {
                this.RaiseAndSetIfChanged(ref this.value, value);
            }
        }
        public int MinimumValue => FieldInfo.MinimumValue;
        public int MaximumValue => FieldInfo.MaximumValue;
        public int Increment => FieldInfo.Increment;
        public bool HasIncrement => FieldInfo.Increment > 0;

        public IntegerViewModel()
        {
            FieldInfo = new IntegerFieldInfo("TestField", "Test Field", 5, 25, 5);
            Value = 15;
        }

        public IntegerViewModel(IntegerFieldInfo fieldInfo, int value)
        {
            FieldInfo = fieldInfo;
            Value = value;
        }

        public override object GetValue() => value;
    }
}
