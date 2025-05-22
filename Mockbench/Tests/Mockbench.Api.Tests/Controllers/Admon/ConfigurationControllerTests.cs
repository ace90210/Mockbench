using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Shared.Models.Configuration;
using Mockbench.Shared.Models.Utility;
using Moq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Mockbench.Api.Tests.Controllers.Admon
{
    public class ConfigurationControllerTests
    {
        private readonly Mock<ILogger<ConfigurationController>> _mockLogger;
        private readonly Mock<IOptions<DeploymentConfiguration>> _mockDeploymentConfigurationOptions;
        private readonly Mock<IDatabaseConfigurationService> _mockDatabaseConfigurationService;
        private readonly ConfigurationController _controller;
        private readonly DeploymentConfiguration _deploymentConfiguration;

        public ConfigurationControllerTests()
        {
            _mockLogger = new Mock<ILogger<ConfigurationController>>();
            _mockDeploymentConfigurationOptions = new Mock<IOptions<DeploymentConfiguration>>();
            _mockDatabaseConfigurationService = new Mock<IDatabaseConfigurationService>();

            _deploymentConfiguration = new DeploymentConfiguration
            {
                DatabaseConfig = new DatabaseConfig { MainConnectionString = "test_connection_string" },
                Debug = true
            };
            _mockDeploymentConfigurationOptions.Setup(o => o.Value).Returns(_deploymentConfiguration);

            _controller = new ConfigurationController(
                _mockLogger.Object,
                _mockDeploymentConfigurationOptions.Object,
                _mockDatabaseConfigurationService.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        private void SetRequestBody(string content)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            _controller.HttpContext.Request.Body = stream;
            _controller.HttpContext.Request.ContentLength = stream.Length;
        }

         [Fact]
        public async Task GetAsync_WhenSqlConnectionSucceeds_ReturnsOkWithConfiguration()
        {
            // Arrange
            _mockDatabaseConfigurationService
                .Setup(s => s.DoesConnectionStringWorkAsync(It.IsAny<string>()))
                .ReturnsAsync(ConnectionStringStatus.Success);
            _mockDatabaseConfigurationService
                .Setup(s => s.GetPendingMigrationsAsync())
                .ReturnsAsync(new List<string> { "migration1" });

            // Act
            var result = await _controller.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<DeploymentConfiguration>(okResult.Value);
            Assert.Equal(ConnectionStringStatus.Success, returnValue.SqlConnectionStatus);
            Assert.Contains("migration1", returnValue.PendingMigrations);
            // Check if connection string is masked (assuming Debug = false by default in this specific copied config)
            // If Debug can be true and unmasked, a separate test or logic adjustment here is needed.
            // For this test, assuming the default path leads to masking if not overridden:
            var tempConfigForMaskingCheck = _deploymentConfiguration.CopyTo(new DeploymentConfiguration());
            if (!string.IsNullOrWhiteSpace(tempConfigForMaskingCheck.DatabaseConfig?.MainConnectionString) && !(_deploymentConfiguration.Debug ?? true))
            {
                 Assert.Equal("*****", returnValue.DatabaseConfig.MainConnectionString);
            }
            else
            {
                 Assert.Equal(_deploymentConfiguration.DatabaseConfig.MainConnectionString, returnValue.DatabaseConfig.MainConnectionString);
            }
        }

        [Fact]
        public async Task GetAsync_WhenSqlConnectionSucceedsAndDebugIsTrue_ReturnsOkWithUnmaskedConnectionString()
        {
            // Arrange
            _deploymentConfiguration.Debug = true; // Ensure debug is true
            // We need to re-initialize the controller if the IOptions.Value changes its Debug state
            // or ensure the existing _controller uses this updated config.
            // For simplicity in this test, let's assume IOptions<T>.Value is correctly set at construction
            // or we re-initialize. Given the current setup, re-initializing for clarity:
             var tempDeploymentConfig = new DeploymentConfiguration
            {
                DatabaseConfig = new DatabaseConfig { MainConnectionString = "test_connection_string_debug" },
                Debug = true
            };
            var mockOptions = new Mock<IOptions<DeploymentConfiguration>>();
            mockOptions.Setup(o => o.Value).Returns(tempDeploymentConfig);

            var controllerWithDebug = new ConfigurationController(
                _mockLogger.Object,
                mockOptions.Object, // Use the new options
                _mockDatabaseConfigurationService.Object);


            _mockDatabaseConfigurationService
                .Setup(s => s.DoesConnectionStringWorkAsync(tempDeploymentConfig.DatabaseConfig.MainConnectionString))
                .ReturnsAsync(ConnectionStringStatus.Success);
            _mockDatabaseConfigurationService
                .Setup(s => s.GetPendingMigrationsAsync())
                .ReturnsAsync(new List<string> { "migration1_debug" });

            // Act
            var result = await controllerWithDebug.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<DeploymentConfiguration>(okResult.Value);
            Assert.Equal(ConnectionStringStatus.Success, returnValue.SqlConnectionStatus);
            Assert.Contains("migration1_debug", returnValue.PendingMigrations);
            Assert.Equal("test_connection_string_debug", returnValue.DatabaseConfig.MainConnectionString);
        }


        [Fact]
        public async Task GetAsync_WhenSqlConnectionFails_ReturnsOkWithConfigurationAndAllMigrations()
        {
            // Arrange
            _mockDatabaseConfigurationService
                .Setup(s => s.DoesConnectionStringWorkAsync(It.IsAny<string>()))
                .ReturnsAsync(ConnectionStringStatus.Failed);
            _mockDatabaseConfigurationService
                .Setup(s => s.GetAllMigrations())
                .Returns(new List<string> { "migration1", "migration2" });

            // Act
            var result = await _controller.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<DeploymentConfiguration>(okResult.Value);
            Assert.Equal(ConnectionStringStatus.Failed, returnValue.SqlConnectionStatus);
            Assert.Contains("migration1", returnValue.PendingMigrations);
            Assert.Contains("migration2", returnValue.PendingMigrations);
        }

        [Fact]
        public async Task GetAsync_WhenDebugIsFalse_MasksConnectionString()
        {
            // Arrange
            var tempDeploymentConfig =  new DeploymentConfiguration
            {
                DatabaseConfig = new DatabaseConfig { MainConnectionString = "sensitive_connection_string" },
                Debug = false // Explicitly false
            };
            var mockOptions = new Mock<IOptions<DeploymentConfiguration>>();
            mockOptions.Setup(o => o.Value).Returns(tempDeploymentConfig);

            var controllerNoDebug = new ConfigurationController(
                 _mockLogger.Object,
                mockOptions.Object,
                _mockDatabaseConfigurationService.Object);


            _mockDatabaseConfigurationService
                .Setup(s => s.DoesConnectionStringWorkAsync(It.IsAny<string>()))
                .ReturnsAsync(ConnectionStringStatus.Success); // Connection status doesn't affect masking logic here
            _mockDatabaseConfigurationService
                .Setup(s => s.GetPendingMigrationsAsync())
                .ReturnsAsync(new List<string>());

            // Act
            var result = await controllerNoDebug.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<DeploymentConfiguration>(okResult.Value);
            Assert.Equal("*****", returnValue.DatabaseConfig.MainConnectionString);
        }


        [Fact]
        public async Task TestConnection_WhenDebugIsTrue_ReturnsOkWithTestResult()
        {
            // Arrange
            // _deploymentConfiguration.Debug = true; // Already set in constructor for _controller
            var testConnectionString = "new_test_connection";
            SetRequestBody(testConnectionString);

            var expectedTestResult = new ConnectionStringTestResult { ConnectionStringStatus = ConnectionStringStatus.Success, Message = string.Empty };
            _mockDatabaseConfigurationService
                .Setup(s => s.TestConnectionStringWorkAsync(testConnectionString))
                .ReturnsAsync(expectedTestResult);

            // Act
            var result = await _controller.TestConnection();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ConnectionStringTestResult>(okResult.Value);
            Assert.Equal(expectedTestResult.ConnectionStringStatus, returnValue.ConnectionStringStatus);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"testing connection string {testConnectionString}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TestConnection_WhenDebugIsFalse_ReturnsNotFound()
        {
            // Arrange
             var tempDeploymentConfig =  new DeploymentConfiguration { Debug = false };
             var mockOptions = new Mock<IOptions<DeploymentConfiguration>>();
             mockOptions.Setup(o => o.Value).Returns(tempDeploymentConfig);

             var controllerNoDebug = new ConfigurationController(
                _mockLogger.Object,
                mockOptions.Object,
                _mockDatabaseConfigurationService.Object);
            // Need to set HttpContext for the new controller instance if Request.Body is accessed
            var httpContext = new DefaultHttpContext();
            controllerNoDebug.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("any_connection_string"));
            controllerNoDebug.HttpContext.Request.Body = stream;


            // Act
            var result = await controllerNoDebug.TestConnection();

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task TestConnection_WhenExceptionOccursAndDebugIsTrue_ReturnsInternalServerError()
        {
            // Arrange
            // _deploymentConfiguration.Debug = true; // Already set
            var testConnectionString = "bad_connection_string";
            SetRequestBody(testConnectionString);
            var exception = new Exception("Test connection error");
            _mockDatabaseConfigurationService
                .Setup(s => s.TestConnectionStringWorkAsync(testConnectionString))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.TestConnection();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal(exception.ToString(), statusCodeResult.Value);
            _mockLogger.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error testing connection string")),
                   exception,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
        }

        [Fact]
        public async Task ApplyMigrations_WhenSuccessful_ReturnsOk()
        {
            // Arrange
            _mockDatabaseConfigurationService
                .Setup(s => s.ApplyMigrationsAsync(_deploymentConfiguration.DatabaseConfig.MainConnectionString))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ApplyMigrations();

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ApplyMigrations_WhenSqlExceptionOccurs_ReturnsBadRequest()
        {
            // Arrange
            // Create a more robust SqlException
            var sqlException = SqlExceptionCreator.CreatePopulatedSqlException("Test SQL error", 123, "Test SQL error");
    
            _mockDatabaseConfigurationService
                .Setup(s => s.ApplyMigrationsAsync(_deploymentConfiguration.DatabaseConfig.MainConnectionString))
                .ThrowsAsync(sqlException);

            // Act
            var result = await _controller.ApplyMigrations();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var error = Assert.IsType<BadRequestResultDto>(badRequestResult.Value);
            Assert.Equal("ApplyMigration", error.Title);

            var errorMessages = error.Errors.FirstOrDefault();
            Assert.Contains("Test SQL error", errorMessages.Value);
        }

        [Fact]
        public async Task ApplyMigrations_WhenGenericExceptionOccurs_ReturnsBadRequest()
        {
            // Arrange
            var exception = new Exception("Unexpected error");
            _mockDatabaseConfigurationService
                .Setup(s => s.ApplyMigrationsAsync(_deploymentConfiguration.DatabaseConfig.MainConnectionString))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.ApplyMigrations();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var error = Assert.IsType<BadRequestResultDto>(badRequestResult.Value);
            var errorMessages = error.Errors.FirstOrDefault();
            Assert.Contains("Failed for unexpected reason", errorMessages.Value);
            // If your ToBadRequestResult() for generic exceptions sets a Source, assert it here.
            // Assert.Equal("SomeSource", error.Source);
        }

    }

    public static class SqlExceptionCreator
    {  // --- Main method to create and populate SqlException ---
        public static SqlException CreatePopulatedSqlException(
            string overallFallbackMessage,
            int errorNumber,
            string specificSqlErrorMessage)
        {
            // 1. Create an uninitialized SqlException instance
            var exception = FormatterServices.GetUninitializedObject(typeof(SqlException)) as SqlException;

            if (exception == null)
            {
                throw new InvalidOperationException("Failed to create uninitialized SqlException instance.");
            }

            // 2. Create SqlError and SqlErrorCollection
            // These methods use reflection as discussed in previous answers
            SqlError sqlError = ConstructSqlError(errorNumber, specificSqlErrorMessage);
            SqlErrorCollection errorCollection = ConstructSqlErrorCollection();

            if (sqlError != null && errorCollection != null)
            {
                // Add SqlError to SqlErrorCollection (using reflection for the Add method)
                AddErrorToCollection(errorCollection, sqlError);

                // 3. Set the private '_errors' field of the SqlException
                FieldInfo errorsField = typeof(SqlException).GetField("_errors", BindingFlags.NonPublic | BindingFlags.Instance);
                if (errorsField != null)
                {
                    errorsField.SetValue(exception, errorCollection);
                }
                else
                {
                    Console.WriteLine("WARNING: _errors field not found on SqlException. Message and Number may not function correctly.");
                }
            }
            else
            {
                Console.WriteLine("WARNING: SqlError or SqlErrorCollection could not be created. SqlException will be minimally populated.");
            }

            // 4. Set the private '_message' field of the base Exception class
            // This is a fallback if _errors is not set or if some logic uses base.Message
            FieldInfo messageField = typeof(Exception).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);
            if (messageField != null)
            {
                messageField.SetValue(exception, overallFallbackMessage);
            }
            else
            {
                Console.WriteLine("WARNING: _message field not found on Exception. Base message cannot be set.");
            }

            return exception;
        }

        // --- Helper Methods (using reflection, simplified here for brevity) ---
        // You need to ensure these are correctly implemented as per previous discussions

        private static SqlError ConstructSqlError(int errorNumber, string message)
        {
            // This method needs to find an internal constructor of SqlError,
            // prepare all its arguments (number, state, class, server, message, procedure, lineNo),
            // and invoke it.
            // Example (highly simplified, real one is more complex):
            Type sqlErrorType = typeof(SqlError);
            // Find appropriate SqlError constructor (e.g., one taking 7+ parameters)
            // This is the part that had TargetParameterCountException earlier
            ConstructorInfo ctor = sqlErrorType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int)  },
                null);

            if (ctor == null)
            {
                Console.WriteLine("ERROR: SqlError constructor not found.");
                return null;
            }
            try
            {
                // Provide all necessary arguments for the SqlError constructor
                return (SqlError)ctor.Invoke(new object[] {
                errorNumber, // number
                (byte)0,     // state
                (byte)14,    // class
                "MockServer",// server
                message,     // message
                "MockProc",  // procedure
                1            // lineNumber
            });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR constructing SqlError: {ex.Message} (Inner: {ex.InnerException?.Message})");
                return null;
            }
        }

        private static SqlErrorCollection ConstructSqlErrorCollection()
        {
            // SqlErrorCollection also has an internal constructor
            Type collectionType = typeof(SqlErrorCollection);
            ConstructorInfo ctor = collectionType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (ctor == null)
            {
                Console.WriteLine("ERROR: SqlErrorCollection constructor not found.");
                return null;
            }
            return (SqlErrorCollection)ctor.Invoke(null);
        }

        private static void AddErrorToCollection(SqlErrorCollection collection, SqlError error)
        {
            if (collection == null || error == null) return;
            // SqlErrorCollection.Add is protected internal
            MethodInfo addMethod = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
            if (addMethod == null)
            {
                Console.WriteLine("ERROR: SqlErrorCollection.Add method not found.");
                return;
            }
            addMethod.Invoke(collection, new object[] { error });
        }
    }
}
