﻿using System;
using System.Diagnostics;
using System.Globalization;

namespace Otter.Converters.QueryString
{
    internal class TimeSpanSecondsQueryStringConverter : IQueryStringConverter
    {
        public bool CanConvert(Type t)
        {
            return t == typeof(TimeSpan);
        }

        public string[] Convert(object o)
        {
            Debug.Assert(o != null);

            return new[] { ((TimeSpan)o).TotalSeconds.ToString(CultureInfo.InvariantCulture) };
        }
    }
}