using System;

namespace Otter
{
    internal interface IQueryStringConverterInstanceFactory
    {
        IQueryStringConverter GetConverterInstance(Type t);
    }
}