﻿using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Otter.Converters.QueryString
{
    internal class JsonQueryStringConverter : IQueryStringConverter
    {
        public bool CanConvert(Type t)
        {
            return true;
        }

        public string[] Convert(object o)
        {
            Debug.Assert(o != null);

            return new[] { JsonConvert.SerializeObject(o, Formatting.None) };
        }
    }
}