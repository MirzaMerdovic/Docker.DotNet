using System;

namespace Otter.Converters.QueryString
{
    internal interface IQueryStringConverter
    {
        bool CanConvert(Type t);

        string[] Convert(object o);
    }
}