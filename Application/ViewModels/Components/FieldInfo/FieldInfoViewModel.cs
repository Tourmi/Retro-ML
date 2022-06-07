using Retro_ML.Configuration.FieldInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.Application.ViewModels.Components.FieldInfo
{
    internal abstract class FieldInfoViewModel : ViewModelBase
    {
        public static FieldInfoViewModel GetFieldInfoViewModel(Configuration.FieldInformation.FieldInfo fieldInfo, object value)
        {
            return fieldInfo switch
            {
                BoolFieldInfo fi => new BoolViewModel(fi, (bool)value),
                IntegerChoiceFieldInfo fi => new IntegerChoiceViewModel(fi, (int)value),
                IntegerFieldInfo fi => new IntegerViewModel(fi, (int)value),
                DoubleFieldInfo fi => new DoubleViewModel(fi, (double)value),
                _ => throw new NotImplementedException("FieldInfo type not supported."),
            };
        }
    }
}
