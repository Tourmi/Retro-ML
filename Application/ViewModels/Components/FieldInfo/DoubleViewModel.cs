using ReactiveUI;
using Retro_ML.Configuration.FieldInformation;

namespace Retro_ML.Application.ViewModels.Components.FieldInfo
{
    internal class DoubleViewModel : FieldInfoViewModel
    {
        public DoubleFieldInfo FieldInfo { get; }
        public override string FieldName => FieldInfo.Name;
        public override string DisplayName => FieldInfo.ReadableName;
        public override string? Tooltip => FieldInfo.Tooltip;

        private double value;
        public double Value
        {
            get => value;
            set
            {
                this.RaiseAndSetIfChanged(ref this.value, value);
            }
        }

        public double MinimumValue => FieldInfo.MinimumValue;
        public double MaximumValue => FieldInfo.MaximumValue;
        public double Increment => FieldInfo.Increment;
        public bool HasIncrement => FieldInfo.Increment > 0;

        public DoubleViewModel()
        {
            FieldInfo = new DoubleFieldInfo("TestField", "Test Field", 5.05, 25.05, 0.05, "Test Field");
            Value = 15;
        }

        public DoubleViewModel(DoubleFieldInfo fieldInfo, double value)
        {
            FieldInfo = fieldInfo;
            Value = value;
        }

        public override object GetValue() => value;
    }
}
