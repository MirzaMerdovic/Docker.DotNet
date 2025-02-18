﻿using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Otter.Converters.Json
{
    using System.Reflection;

    internal class JsonIso8601AndUnixEpochDateConverter : JsonConverter
    {
        private static readonly DateTime UnixEpochBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var isNullableType =
                objectType.GetTypeInfo().IsGenericType &&
                objectType.GetGenericTypeDefinition() == typeof(Nullable<>);

            var value = reader.Value;

            DateTime result;

            if (value is DateTime time)
            {
                result = time;
            }
            else if (value is string @string)
            {
                // ISO 8601 String
                result = DateTime.Parse(@string, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
            else if (value is long @int)
            {
                // UNIX epoch timestamp (in seconds)
                result = UnixEpochBase.AddSeconds(@int);
            }
            else
            {
                throw new NotImplementedException($"Deserializing {value.GetType().FullName} back to {objectType.FullName} is not handled.");
            }

            if (isNullableType && result == default)
            {
                return null; // do not set result on DateTime? field
            }

            return result;
        }
    }
}