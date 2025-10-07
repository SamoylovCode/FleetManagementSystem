using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Services;
using FleetManagementSystemApp.Infrastructure.Caching;
using FluentAssertions;
using Moq;

namespace FleetManagementSystemApp.Tests.Services;

// MethodName_StateUnderTest_ExpectedBehavior

public class UserServiceTests
{
    [Fact]
    public async Task GetUserById_WhenUserIdIsNullOrWhitespace_ReturnsFailure()
    {
        // Arrange
        var hybridCacheMock = new Mock<IHybridCache>();

        var sut = new UserService(
            dbContext: null,
            userDto: null,
            currentUserService: null,
            confirmationEmailService: null,
            hybridCache: hybridCacheMock.Object,
            serviceProvider: null,
            signInManager: null,
            userAddValidator: null,
            validator: null,
            loginValidator: null,
            userManager: null);

        // Act
        var result = await sut.GetUserByIdAsync(userId: " ");

        // Assert
        result.IsFailure.Should().BeTrue();
        hybridCacheMock.Verify(h => h.GetOrAddAsync(
            It.IsAny<Func<Task<ApplicationUserDto>>>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserById_WhenCacheHasValue_ReturnsDtoFromCache()
    {
        // Arrange
        var expectedDto = new ApplicationUserDto
        {
            UserId = "user-123",
            FirstName = "Иван",
            MiddleName = "Иванович",
            LastName = "Иванов",
            Email = "ivanov.test@example.com",
        };

        var hybridCacheMock = new Mock<IHybridCache>();

        hybridCacheMock
            .Setup(h => h.GetOrAddAsync(
                It.IsAny<Func<Task<ApplicationUserDto>>>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<string>()))
            .ReturnsAsync(expectedDto);

        var sut = new UserService(
            dbContext: null,
            userDto: null,
            currentUserService: null,
            confirmationEmailService: null,
            hybridCache: hybridCacheMock.Object,
            serviceProvider: null,
            signInManager: null,
            userAddValidator: null,
            validator: null,
            loginValidator: null,
            userManager: null);

        // Act
        var result = await sut.GetUserByIdAsync(userId: expectedDto.UserId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(expectedDto.UserId);

        hybridCacheMock
            .Verify(h => h.GetOrAddAsync(
                It.IsAny<Func<Task<ApplicationUserDto>>>(),
                expectedDto.UserId,
                It.IsAny<TimeSpan>(),
                It.IsAny<string>()),
            Times.Once);
    }
}