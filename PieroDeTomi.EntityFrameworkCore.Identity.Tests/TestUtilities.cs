﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PieroDeTomi.EntityFrameworkCore.Identity.Cosmos;
using PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Repositories;
using PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Stores;
using System.Reflection;

namespace PieroDeTomi.EntityFrameworkCore.Identity.Tests
{
    public class TestUtilities
    {
        private IConfigurationRoot? _configuration;

        /// <summary>
        /// Non-mormalized email address for user 1
        /// </summary>
        public const string IDENUSER1EMAIL = "Foo1@acme.com";
        /// <summary>
        /// Non-normalized email address for user 2
        /// </summary>
        public const string IDENUSER2EMAIL = "Foo2@acme.com";


        public const string IDENUSER1ID = "507b7565-493e-49d7-94c7-d60e21036b4a";
        public const string IDENUSER2ID = "55250c6f-7c91-465a-a9ce-ea9bbe6caf81";

        /// <summary>
        /// Gets the configuration
        /// </summary>
        /// <returns></returns>
        public IConfigurationRoot GetConfig()
        {
            if (_configuration != null) return _configuration;

            // the type specified here is just so the secrets library can 
            // find the UserSecretId we added in the csproj file
            var jsonConfig = Path.Combine(Environment.CurrentDirectory, "appsettings.json");

            var builder = new ConfigurationBuilder()
                .AddJsonFile(jsonConfig, true)
                .AddEnvironmentVariables() // Added to read environment variables from GitHub Actions
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true); // User secrets override all - put here

            _configuration = Retry.Do(() => builder.Build(), TimeSpan.FromSeconds(1));

            return _configuration;
        }

        /// <summary>
        /// Gets the value of a configuration key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKeyValue(string key)
        {
            return GetKeyValue(GetConfig(), key);
        }

        private string GetKeyValue(IConfigurationRoot config, string key)
        {
            var data = config[key];

            if (string.IsNullOrEmpty(data))
            {
                // First attempt to get the value of the key as named.
                data = Environment.GetEnvironmentVariable(key);

                if (string.IsNullOrEmpty(data))
                {
                    // For Github Actions, secrets are forced upper case
                    data = Environment.GetEnvironmentVariable(key.ToUpper());
                }
            }
            return string.IsNullOrEmpty(data) ? string.Empty : data;
        }

        /// <summary>
        /// Get Cosmos DB Options
        /// </summary>
        /// <returns></returns>
        public DbContextOptions GetDbOptions()
        {
            var config = GetConfig();
            var connectionString = config.GetConnectionString("ApplicationDbContextConnection");
            var builder = new DbContextOptionsBuilder();
            builder.UseCosmos(connectionString, "cosmosdb");

            return builder.Options;
        }

        /// <summary>
        /// Get an instance of the Cosmos DB context.
        /// </summary>
        /// <returns></returns>
        public CosmosIdentityDbContext<IdentityUser> GetDbContext()
        {
            var dbContext = new CosmosIdentityDbContext<IdentityUser>(GetDbOptions());
            return dbContext;
        }

        /// <summary>
        /// Get an instance of the Cosmos DB user store.
        /// </summary>
        /// <returns></returns>
        public CosmosUserStore<IdentityUser> GetUserStore()
        {

            var repository = new CosmosIdentityRepository<CosmosIdentityDbContext<IdentityUser>, IdentityUser>(GetDbContext());

            var userStore = new CosmosUserStore<IdentityUser>(repository);

            return userStore;
        }

        /// <summary>
        /// Get an instance of the Cosmos DB role store
        /// </summary>
        /// <returns></returns>
        public CosmosRoleStore<IdentityRole> GetRoleStore()
        {
            var repository = new CosmosIdentityRepository<CosmosIdentityDbContext<IdentityUser>, IdentityUser>(GetDbContext());

            var rolestore = new CosmosRoleStore<IdentityRole>(repository);

            return rolestore;
        }

        /// <summary>
        /// Get an instance of the role manager
        /// </summary>
        /// <returns></returns>
        public RoleManager<IdentityRole> GetRoleManager()
        {
            var userStore = GetRoleStore();
            var userManager = new RoleManager<IdentityRole>(userStore, null, null, null, GetLogger<RoleManager<IdentityRole>>());
            return userManager;
        }

        /// <summary>
        /// Get and instance of the user manager
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public UserManager<IdentityUser> GetUserManager()
        {
            var userStore = GetUserStore();
            var userManager = new UserManager<IdentityUser>(userStore, null, new PasswordHasher<IdentityUser>(), null,
                null, null, null, null, GetLogger<UserManager<IdentityUser>>());
            return userManager;
        }

        /// <summary>
        /// Get a mock logger
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ILogger<T> GetLogger<T>()
        {
            return new Logger<T>(new NullLoggerFactory());
        }
    }
}
