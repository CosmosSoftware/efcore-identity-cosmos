﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Containers;
using PieroDeTomi.EntityFrameworkCore.Identity.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Containers.Tests
{
    [TestClass()]
    public class ContainerUtilitiesTests
    {

        private static TestUtilities utils;
        private static ContainerUtilities containerUtilities;

        /// <summary>
        /// Class initialize
        /// </summary>
        /// <param name="context"></param>
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            //
            // Setup context.
            //
            utils = new TestUtilities();
            containerUtilities = utils.GetContainerUtilities();
        }

        /// <summary>
        /// Class cleanup
        /// </summary>
        /// <param name="context"></param>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            containerUtilities.Dispose();
        }

        /// <summary>
        /// Removes all containers prior to running tests.
        /// </summary>
        /// <returns></returns>
        //[TestMethod()]
        //public async Task A1_RemoveAllContainersPriorToTest()
        //{
        //    Assert.IsNotNull(containerUtilities);

        //    // Get rid of all the containers if they exist.
        //    await containerUtilities.DeleteRequiredContainers();
        //}

        [TestMethod()]
        public async Task A2_DeleteDatabaseIfExistsTest()
        {
            var result = await containerUtilities.DeleteDatabaseIfExists(TestUtilities.DATABASENAME);

            Assert.IsTrue(result == null || result.StatusCode == System.Net.HttpStatusCode.OK || result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        [TestMethod()]
        public async Task A3_CreateDatabaseIfExistsTest()
        {
            var result = await containerUtilities.CreateDatabaseAsync(TestUtilities.DATABASENAME);

            Assert.IsTrue(result.StatusCode == System.Net.HttpStatusCode.OK || result.StatusCode == System.Net.HttpStatusCode.NoContent || result.StatusCode == System.Net.HttpStatusCode.Created);
        }

        /// <summary>
        /// Removes all containers after tests have run.
        /// </summary>
        /// <returns></returns>
        [TestMethod()]
        public async Task X_RemoveAllContainersPriorToTest()
        {
            Assert.IsNotNull(containerUtilities);

            // Get rid of all the containers if they exist.
            await containerUtilities.DeleteRequiredContainers();
        }

        /// <summary>
        /// Establishes the utilities class can be created.
        /// </summary>
        [TestMethod()]
        public void ContainerUtilitiesTest()
        {
            Assert.IsNotNull(containerUtilities);
        }

        [TestMethod()]
        public async Task A4_CreateRequiredContainersTest()
        {
            var containers = await containerUtilities.CreateRequiredContainers();

            var requiredContainerDefinitions = containerUtilities.GetRequiredContainerDefinitions();

            Assert.AreEqual(requiredContainerDefinitions.Count, containers.Count);

            foreach(var con in requiredContainerDefinitions)
            {
                Assert.IsTrue(containers.Any(a => a.Id == con.ContainerName));
            }
        }

        [TestMethod()]
        public void Z_DeleteDatabaseIfExistsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteContainerIfExistsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetRequiredContainerDefinitionsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DisposeTest()
        {
            Assert.Fail();
        }
    }
}