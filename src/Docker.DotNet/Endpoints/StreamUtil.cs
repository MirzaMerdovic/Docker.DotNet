using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Docker.DotNet.Models
{
    internal static class StreamUtil
    {
        internal static async Task MonitorStreamAsync(Task<Stream> streamTask, CancellationToken cancellationToken, Func<string, Task> progress)
        {
            var tcs = new TaskCompletionSource<string>();

            using (var stream = await streamTask)
            using (var reader = new StreamReader(stream, new UTF8Encoding(false)))
            using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
            {
                string line;
                while ((line = await await Task.WhenAny(reader.ReadLineAsync(), tcs.Task)) != null)
                {
                    await progress(line);
                }
            }
        }

        internal static async Task MonitorStreamForMessagesAsync<T>(
            Task<Stream> streamTask,
            CancellationToken cancellationToken, 
            Func<T, Task> progress)
        {
            var tcs = new TaskCompletionSource<bool>();

            using (var stream = await streamTask)
            using (var reader = new StreamReader(stream, new UTF8Encoding(false)))
            using (var jsonReader = new JsonTextReader(reader) { SupportMultipleContent = true })
            using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
            {
                while (await await Task.WhenAny(jsonReader.ReadAsync(cancellationToken), tcs.Task))
                {
                    var ev = await JsonSerializer.Deserialize<T>(jsonReader, cancellationToken);
                    await progress(ev);
                }
            }
        }

        internal static async Task MonitorResponseForMessagesAsync<T>(
            Task<HttpResponseMessage> responseTask,
            CancellationToken cancel, 
            Func<T, Task> progress)
        {
            using (var response = await responseTask)
            {
                await MonitorStreamForMessagesAsync(response.Content.ReadAsStreamAsync(), cancel, progress);
            }
        }
    }
}
