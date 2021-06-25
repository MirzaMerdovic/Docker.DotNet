using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Docker.DotNet
{
    /// <summary>
    /// Facade for <see cref="JsonConvert"/>.
    /// </summary>
    internal static class JsonSerializer
    {
        private static readonly Func<JsonSerializerSettings> Settings = delegate ()
        {
            return
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new JsonConverter[]
                        {
                            new JsonIso8601AndUnixEpochDateConverter(),
                            new JsonVersionConverter(),
                            new StringEnumConverter(),
                            new TimeSpanSecondsConverter(),
                            new TimeSpanNanosecondsConverter(),
                            new JsonBase64Converter()
                        }
                };
        };

        private static readonly Func<Newtonsoft.Json.JsonSerializer> Serializer = delegate ()
        {
            return Newtonsoft.Json.JsonSerializer.CreateDefault(Settings());
        };

        public static Task<T> Deserialize<T>(JsonReader jsonReader, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();
            using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
            {
                Task.Factory.StartNew(
                    () => tcs.TrySetResult(Serializer().Deserialize<T>(jsonReader)),
                    cancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                );

                return tcs.Task;
            }
        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings());
        }

        public static string SerializeObject<T>(T value)
        {
            return JsonConvert.SerializeObject(value, Settings());
        }
    }
}