using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Docker.DotNet
{
    internal class JsonRequestContent<T> : IRequestContent
    {
        private const string JsonMimeType = "application/json";

        private readonly T _value;

        public JsonRequestContent(T val)
        {
            if (EqualityComparer<T>.Default.Equals(val))
            {
                throw new ArgumentNullException(nameof(val));
            }

            _value = val;
        }

        public HttpContent GetContent()
        {
            var serializedObject = JsonSerializer.SerializeObject(_value);

            return new StringContent(serializedObject, Encoding.UTF8, JsonMimeType);
        }
    }
}