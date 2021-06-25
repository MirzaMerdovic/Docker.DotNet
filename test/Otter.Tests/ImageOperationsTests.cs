using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Otter.Models;
using Xunit;
using Xunit.Abstractions;

namespace Otter.Tests
{
    [Collection("Otter")]
    public class ImageOperationsTests
    {

        private readonly CancellationTokenSource _cts;

        private readonly TestOutput _output;
        private readonly string _repositoryName;
        private readonly string _tag;
        private readonly DockerClientConfiguration _dockerConfiguration;
        private readonly DockerClient _dockerClient;

        public ImageOperationsTests(OtterFixture testFixture, ITestOutputHelper _outputHelper)
        {
            _output = new TestOutput(_outputHelper);

            _dockerConfiguration = new DockerClientConfiguration();
            _dockerClient = _dockerConfiguration.CreateClient();

            // Do not wait forever in case it gets stuck
            _cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            _cts.Token.Register(() => throw new TimeoutException("ImageOperationTests timeout"));

            _repositoryName = testFixture.RepositoryName;
            _tag = testFixture.Tag;
        }

        [Fact]
        public async Task CreateImageAsync_TaskCancelled_ThowsTaskCanceledException()
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);

            var newTag = Guid.NewGuid().ToString();
            var newRepositoryName = Guid.NewGuid().ToString();

            await _dockerClient.Images.TagImageAsync(
                $"{_repositoryName}:{_tag}",
                new ImageTagParameters
                {
                    RepositoryName = newRepositoryName,
                    Tag = newTag,
                    Force = true
                },
                cts.Token
            );

            var createImageTask = _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = $"{newRepositoryName}:{newTag}"
                },
                null,
                async message => 
                {
                    _output.WriteLine(JsonConvert.SerializeObject(message));
                    await Task.CompletedTask;
                },
                cts.Token);

            TimeSpan delay = TimeSpan.FromMilliseconds(5);
            cts.CancelAfter(delay);

            await Assert.ThrowsAsync<TaskCanceledException>(() => createImageTask);

            Assert.True(createImageTask.IsCanceled);
        }

        [Fact]
        public async Task DeleteImageAsync_RemovesImage()
        {
            var newImageTag = Guid.NewGuid().ToString();

            await _dockerClient.Images.TagImageAsync(
                $"{_repositoryName}:{_tag}",
                new ImageTagParameters
                {
                    RepositoryName = _repositoryName,
                    Tag = newImageTag
                },
                _cts.Token
            );

            var inspectExistingImageResponse = await _dockerClient.Images.InspectImageAsync(
                $"{_repositoryName}:{newImageTag}",
                _cts.Token
            );

            await _dockerClient.Images.DeleteImageAsync(
                $"{_repositoryName}:{newImageTag}",
                new ImageDeleteParameters(),
                _cts.Token
            );

            Task inspectDeletedImageTask = _dockerClient.Images.InspectImageAsync(
                $"{_repositoryName}:{newImageTag}",
                _cts.Token
            );

            Assert.NotNull(inspectExistingImageResponse);
            await Assert.ThrowsAsync<DockerImageNotFoundException>(() => inspectDeletedImageTask);
        }
    }
}
