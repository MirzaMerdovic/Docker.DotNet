using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Otter.Models;
using Xunit;

namespace Otter.Tests
{
    [CollectionDefinition("Otter")]
    public class OtterTestsCollection : ICollectionFixture<OtterFixture>
    {
    }

    public class OtterFixture : IDisposable
    {
        // Tests require an image whose containers continue running when created new, and works on both Windows an Linux containers. 
        private const string IMAGE_NAME = "nats";

        private readonly bool _wasSwarmInitialized = false;

        public OtterFixture()
        {
            // Do not wait forever in case it gets stuck
            CancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            CancellationSource.Token.Register(() => throw new TimeoutException("Otter test timeout exception"));

            DockerClientConfiguration = new DockerClientConfiguration();
            DockerClient = DockerClientConfiguration.CreateClient();

            // Create image
            DockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = IMAGE_NAME,
                    Tag = "latest"
                },
                null,
                async m =>
                {
                    Console.WriteLine(JsonConvert.SerializeObject(m));
                    Debug.WriteLine(JsonConvert.SerializeObject(m));

                    await Task.CompletedTask;
                },
                CancellationSource.Token).GetAwaiter().GetResult();

            // Create local image tag to reuse
            var existingImagesResponse = DockerClient.Images.ListImagesAsync(
               new ImagesListParameters
               {
                   Filters = new Dictionary<string, IDictionary<string, bool>>
                   {
                       ["reference"] = new Dictionary<string, bool>
                       {
                           [IMAGE_NAME] = true
                       }
                   }
               },
               CancellationSource.Token
           ).GetAwaiter().GetResult();

            ImageId = existingImagesResponse[0].ID;

            DockerClient.Images.TagImageAsync(
                ImageId,
                new ImageTagParameters
                {
                    RepositoryName = RepositoryName,
                    Tag = Tag
                },
                CancellationSource.Token
            ).GetAwaiter().GetResult();

            // Init swarm if not part of one
            try
            {
                DockerClient.Swarm.InitSwarmAsync(new SwarmInitParameters { AdvertiseAddr = "10.10.10.10", ListenAddr = "127.0.0.1" }, default).GetAwaiter().GetResult();
            }
            catch
            {
                Console.WriteLine("Couldn't init a new swarm, node should take part of a existing one");
                _wasSwarmInitialized = true;
            }
        }

        public CancellationTokenSource CancellationSource { get; }
        public DockerClient DockerClient { get; }
        public DockerClientConfiguration DockerClientConfiguration { get; }
        public string RepositoryName { get; } = Guid.NewGuid().ToString();
        public string Tag { get; } = Guid.NewGuid().ToString();
        public string ImageId { get; }

        public void Dispose()
        {
            if (_wasSwarmInitialized)
                DockerClient.Swarm.LeaveSwarmAsync(new SwarmLeaveParameters { Force = true }, CancellationSource.Token).GetAwaiter().GetResult();

            var containerList = DockerClient.Containers.ListContainersAsync(
                new ContainersListParameters
                {
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                    {
                        ["ancestor"] = new Dictionary<string, bool>
                        {
                            [$"{RepositoryName}:{Tag}"] = true
                        }
                    },
                    All = true,
                },
                CancellationSource.Token
                ).GetAwaiter().GetResult();

            foreach (ContainerListResponse container in containerList)
            {
                DockerClient.Containers.RemoveContainerAsync(
                    container.ID,
                    new ContainerRemoveParameters
                    {
                        Force = true
                    },
                    CancellationSource.Token
                ).GetAwaiter().GetResult();
            }

            var imageList = DockerClient.Images.ListImagesAsync(
                new ImagesListParameters
                {
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                    {
                        ["reference"] = new Dictionary<string, bool>
                        {
                            [ImageId] = true
                        },
                        ["since"] = new Dictionary<string, bool>
                        {
                            [ImageId] = true
                        }
                    },
                    All = true
                },
                CancellationSource.Token
            ).GetAwaiter().GetResult();

            foreach (ImagesListResponse image in imageList)
            {
                DockerClient.Images.DeleteImageAsync(
                    image.ID,
                    new ImageDeleteParameters { Force = true },
                    CancellationSource.Token
                ).GetAwaiter().GetResult();
            }

            DockerClient.Dispose();
            DockerClientConfiguration.Dispose();
            CancellationSource.Dispose();
        }
    }
}
