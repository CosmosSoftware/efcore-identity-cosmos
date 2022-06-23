using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Containers
{
    /// <summary>
    /// Utilities for creating Cosmos DB Containers
    /// </summary>
    /// <remarks>
    /// This class is only ment to run when the database needs to be created, deleted or containers removed.
    /// </remarks>
    public class ContainerUtilities : IDisposable
    {
        private readonly CosmosClient _client;
        private readonly string _databaseName;

        /// <summary>
        /// Constructor that creates the database if it does not already exist.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="databaseName"
        /// <param name="clientOptions"></param>
        public ContainerUtilities(string connectionString, string databaseName, CosmosClientOptions clientOptions = null)
        {

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            _client = new CosmosClient(connectionString, clientOptions);
            _databaseName = databaseName;
            _client.CreateDatabaseIfNotExistsAsync(_databaseName).Wait();
        }

        /// <summary>
        /// Deletes the database.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>WARNING! ALL DATA WILL BE LOST AND THIS CANNOT BE UNDONE!</remarks>
        public async Task<DatabaseResponse> DeleteDatabaseIfExists(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            var database = _client.GetDatabase(databaseName);

            var response = await database.DeleteAsync();

            return response;
        }

        /// <summary>
        /// Creates the specified container if it does not already exist.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="partitionKeyPath"></param>
        /// <param name="throughput"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Container> CreateContainerIfNotExistsAsync(string containerName, string partitionKeyPath, int? throughput)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException(nameof(containerName));

            if (string.IsNullOrEmpty(partitionKeyPath))
                throw new ArgumentNullException(nameof(partitionKeyPath));

            if (!partitionKeyPath.StartsWith("/"))
                throw new ArgumentException(nameof(partitionKeyPath), "Path must begin with /");

            var database = _client.GetDatabase(_databaseName);

            Container container = await database.CreateContainerIfNotExistsAsync(
                    id: containerName,
                    partitionKeyPath: partitionKeyPath,
                    throughput: throughput);

            return container;
        }

        /// <summary>
        /// Deletes a container
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<ContainerResponse> DeleteContainerIfExists(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException(nameof(containerName));

            var database = _client.GetDatabase(_databaseName);
            var container = database.GetContainer(containerName);
            var response = await container.DeleteContainerAsync();
            return response;
        }

        /// <summary>
        /// Get a list of all the required containers.
        /// </summary>
        /// <returns></returns>
        public List<ContainerDefinition> GetRequiredContainerDefinitions()
        {
            var list = new List<ContainerDefinition>();

            list.Add(new ContainerDefinition() { ContainerName = "Identity", PartitionKey = "/Id" });
            list.Add(new ContainerDefinition() { ContainerName = "Identity_DeviceFlowCodes", PartitionKey = "/SessionId" });
            list.Add(new ContainerDefinition() { ContainerName = "Identity_Logins", PartitionKey = "/ProviderKey" });
            list.Add(new ContainerDefinition() { ContainerName = "Identity_PersistedGrant", PartitionKey = "/Key" });
            list.Add(new ContainerDefinition() { ContainerName = "Identity_Tokens", PartitionKey = "/UserId" });
            list.Add(new ContainerDefinition() { ContainerName = "Identity_UserRoles", PartitionKey = "/UserId" });
            list.Add(new ContainerDefinition() { ContainerName = "Identity_Roles", PartitionKey = "/Id" });

            return list;
        }

        /// <summary>
        /// Disposes of the class resources
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }
    }

    /// <summary>
    /// Container definition
    /// </summary>
    public class ContainerDefinition
    {
        public string ContainerName { get; set; }

        public string PartitionKey { get; set; }
    }
}
