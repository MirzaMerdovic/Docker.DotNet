using System;

namespace Otter
{
    internal interface IQueryStringConverter
    {
        bool CanConvert(Type t);

        string[] Convert(object o);
    }
}