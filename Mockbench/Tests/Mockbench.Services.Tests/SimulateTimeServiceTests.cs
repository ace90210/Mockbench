// Mockbench.Services.Tests/SimulateTimeServiceTests.cs
using Mockbench.Abstractions.Repositories;
using Mockbench.Abstractions.Services;
using Mockbench.Services.MockServices;
using Mockbench.Shared;
using Mockbench.Shared.Models.Timetravel;
using Moq;


namespace Mockbench.Services.Tests
{
    public class SimulateTimeServiceTests
    {
        private readonly Mock<ICommonRepository> _mockCommonRepository;
        private readonly ISimulateTimeService _simulateTimeService;

        public SimulateTimeServiceTests()
        {
            _mockCommonRepository = new Mock<ICommonRepository>();
            _simulateTimeService = new SimulateTimeService(_mockCommonRepository.Object);
        }

        [Fact]
        public void Constructor_NullCommonRepository_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>("baseRepository", () => new SimulateTimeService(null!));
        }

        // --- GetTimes Tests ---
        [Theory]
        [InlineData(TimeTravelScope.Endpoint)]
        [InlineData(TimeTravelScope.Microservice)]
        [InlineData(TimeTravelScope.Environment)]
        [InlineData(TimeTravelScope.Tenant)]
        public async Task GetTimes_ValidScope_CallsCorrectRepositoryMethod(TimeTravelScope scope)
        {
            // Arrange
            var id = 123;
            var expectedTimeTravelDto = new TimeTravelDto { CurrentTime = DateTime.UtcNow };

            _mockCommonRepository.Setup(r => r.GetRequestTimes(id)).ReturnsAsync(expectedTimeTravelDto);
            _mockCommonRepository.Setup(r => r.GetMicroserviceTimes(id)).ReturnsAsync(expectedTimeTravelDto);
            _mockCommonRepository.Setup(r => r.GetEnvironmentTimes(id)).ReturnsAsync(expectedTimeTravelDto);
            _mockCommonRepository.Setup(r => r.GetTenanEnvironmentTimes(id)).ReturnsAsync(expectedTimeTravelDto); // Corrected method name based on repository interface.

            // Act
            var result = await _simulateTimeService.GetTimes(scope, id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTimeTravelDto.CurrentTime, result.CurrentTime);

            switch (scope)
            {
                case TimeTravelScope.Endpoint:
                    _mockCommonRepository.Verify(r => r.GetRequestTimes(id), Times.Once);
                    break;
                case TimeTravelScope.Microservice:
                    _mockCommonRepository.Verify(r => r.GetMicroserviceTimes(id), Times.Once);
                    break;
                case TimeTravelScope.Environment:
                    _mockCommonRepository.Verify(r => r.GetEnvironmentTimes(id), Times.Once);
                    break;
                case TimeTravelScope.Tenant:
                    _mockCommonRepository.Verify(r => r.GetTenanEnvironmentTimes(id), Times.Once);
                    break;
            }
        }

        [Fact]
        public async Task GetTimes_InvalidScope_ReturnsNull()
        {
            // Arrange
            var id = 1;
            var invalidScope = (TimeTravelScope)99; // An undefined enum value

            // Act
            var result = await _simulateTimeService.GetTimes(invalidScope, id);

            // Assert
            Assert.Null(result);
            _mockCommonRepository.Verify(r => r.GetRequestTimes(It.IsAny<int>()), Times.Never);
            _mockCommonRepository.Verify(r => r.GetMicroserviceTimes(It.IsAny<int>()), Times.Never);
            _mockCommonRepository.Verify(r => r.GetEnvironmentTimes(It.IsAny<int>()), Times.Never);
            _mockCommonRepository.Verify(r => r.GetTenanEnvironmentTimes(It.IsAny<int>()), Times.Never);
        }

        // --- SetSimulateTime Tests ---
        [Theory]
        [InlineData(TimeTravelScope.Endpoint)]
        [InlineData(TimeTravelScope.Microservice)]
        [InlineData(TimeTravelScope.Environment)]
        [InlineData(TimeTravelScope.Tenant)]
        public async Task SetSimulateTime_ValidScope_CallsCorrectRepositoryMethodAndReturnsResult(TimeTravelScope scope)
        {
            // Arrange
            var id = 456;
            var timeToSet = DateTime.UtcNow.AddDays(1);
            var updateDto = new UpdateTimeTravelDto { Scope = scope, Time = timeToSet };
            var expectedResult = true;

            _mockCommonRepository.Setup(r => r.SetSimulateTimeOnRequest(timeToSet, id)).ReturnsAsync(expectedResult);
            _mockCommonRepository.Setup(r => r.SetSimulateTimeOnMicroservice(timeToSet, id)).ReturnsAsync(expectedResult);
            _mockCommonRepository.Setup(r => r.SetSimulateTimeOnEnvironment(timeToSet, id)).ReturnsAsync(expectedResult);
            _mockCommonRepository.Setup(r => r.SetSimulateTimeOnTenant(timeToSet, id)).ReturnsAsync(expectedResult);

            // Act
            var result = await _simulateTimeService.SetSimulateTime(updateDto, id);

            // Assert
            Assert.Equal(expectedResult, result);

            switch (scope)
            {
                case TimeTravelScope.Endpoint:
                    _mockCommonRepository.Verify(r => r.SetSimulateTimeOnRequest(timeToSet, id), Times.Once);
                    break;
                case TimeTravelScope.Microservice:
                    _mockCommonRepository.Verify(r => r.SetSimulateTimeOnMicroservice(timeToSet, id), Times.Once);
                    break;
                case TimeTravelScope.Environment:
                    _mockCommonRepository.Verify(r => r.SetSimulateTimeOnEnvironment(timeToSet, id), Times.Once);
                    break;
                case TimeTravelScope.Tenant:
                    _mockCommonRepository.Verify(r => r.SetSimulateTimeOnTenant(timeToSet, id), Times.Once);
                    break;
            }
        }

        [Fact]
        public async Task SetSimulateTime_NullTime_CallsRepositoryMethodWithNullTime()
        {
            // Arrange
            var id = 789;
            DateTime? timeToSet = null;
            var scope = TimeTravelScope.Microservice;
            var updateDto = new UpdateTimeTravelDto { Scope = scope, Time = timeToSet };
            var expectedResult = true;

            _mockCommonRepository.Setup(r => r.SetSimulateTimeOnMicroservice(timeToSet, id)).ReturnsAsync(expectedResult);

            // Act
            var result = await _simulateTimeService.SetSimulateTime(updateDto, id);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockCommonRepository.Verify(r => r.SetSimulateTimeOnMicroservice(timeToSet, id), Times.Once);
        }

        [Fact]
        public async Task SetSimulateTime_InvalidScope_ReturnsFalse()
        {
            // Arrange
            var id = 1;
            var timeToSet = DateTime.UtcNow;
            var invalidScope = (TimeTravelScope)99; // An undefined enum value
            var updateDto = new UpdateTimeTravelDto { Scope = invalidScope, Time = timeToSet };


            // Act
            var result = await _simulateTimeService.SetSimulateTime(updateDto, id);

            // Assert
            Assert.False(result);
            _mockCommonRepository.Verify(r => r.SetSimulateTimeOnRequest(It.IsAny<DateTime?>(), It.IsAny<int>()), Times.Never);
            _mockCommonRepository.Verify(r => r.SetSimulateTimeOnMicroservice(It.IsAny<DateTime?>(), It.IsAny<int>()), Times.Never);
            _mockCommonRepository.Verify(r => r.SetSimulateTimeOnEnvironment(It.IsAny<DateTime?>(), It.IsAny<int>()), Times.Never);
            _mockCommonRepository.Verify(r => r.SetSimulateTimeOnTenant(It.IsAny<DateTime?>(), It.IsAny<int>()), Times.Never);
        }
    }

}