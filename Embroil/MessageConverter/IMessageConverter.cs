using System.Collections.Generic;

namespace Vrc.Embroil.MessageConverter
{
    public interface IMessageConverter<T1,T2>
    {
        T2 CovertTo(params T1[] input);
        IEnumerable<T1> ConvertFrom(T2 input);
    }
}