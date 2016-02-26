using System.Collections.Generic;

namespace Vrc.Embroil.Converter
{
    public interface IConverter<T1,T2>
    {
        T2 CovertTo(T1 input);
        IEnumerable<T1> ConvertFrom(T2 input);
    }
}