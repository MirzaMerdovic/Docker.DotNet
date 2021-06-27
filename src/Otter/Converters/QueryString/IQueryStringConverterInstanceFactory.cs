using System;

namespace Otter.Converters.QueryString
{
    internal interface IQueryStringConverterInstanceFactory
    {
        IQueryStringConverter GetConverterInstance(Type t);
    }
}