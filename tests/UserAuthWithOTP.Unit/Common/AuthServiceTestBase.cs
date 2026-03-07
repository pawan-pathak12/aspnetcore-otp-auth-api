using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UserAuth.Api.Data;
using UserAuth.Api.Interfaces.Service;
using UserAuth.Api.Repository.InMemory;
using UserAuth.Api.Services;

public abstract class AuthServiceTestBase
{
    protected ITokenService _tokenService = null!;
    protected IRefreshTokenService _refreshTokenService = null!;
    protected IOtpService _otpService = null!;
    protected IEmailService _emailService = null!;

    protected InMemoryUserRepo _userRepo = null!;
    protected InMemoryRefreshTokenRepo _inMemoryRefreshToken = null!;
    protected InMemoryOtpVerificationRepository _otpVerificationRepository = null!;

    protected AuthService _authService = null!;
    protected Mock<IConfiguration> ConfigMock { get; private set; } = null!;

    [TestInitialize]
    public void TestInit()
    {
        var dbContext = new InMemoryDbContext();

        _userRepo = new InMemoryUserRepo(dbContext);
        _inMemoryRefreshToken = new InMemoryRefreshTokenRepo(dbContext);
        _otpVerificationRepository = new InMemoryOtpVerificationRepository(dbContext);

        // Setup IConfiguration with actual values
        ConfigMock = new Mock<IConfiguration>();

        var jwtSectionMock = new Mock<IConfigurationSection>();

        jwtSectionMock.Setup(s => s["Key"]).Returns("this-is-your-very-secure-jwt-secret-key-1234567890");
        jwtSectionMock.Setup(s => s["Issuer"]).Returns("JwtAuthLearning.Api");
        jwtSectionMock.Setup(s => s["Audience"]).Returns("JwtAuthLearning.Api");
        jwtSectionMock.Setup(s => s["ExpiresInMinutes"]).Returns("15");

        ConfigMock.Setup(c => c.GetSection("Jwt")).Returns(jwtSectionMock.Object);
        // Mock EmailService instead of using real one
        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Create loggers
        var loggerMock = new Mock<ILogger<OtpService>>();

        //  Create services in correct order
        _tokenService = new TokenService(ConfigMock.Object);
        _emailService = emailServiceMock.Object;
        _refreshTokenService = new RefreshTokenService(_inMemoryRefreshToken);
        _otpService = new OtpService(
            _otpVerificationRepository,
            _emailService,
            loggerMock.Object,
            _userRepo
        );

        //  Create AuthService
        _authService = new AuthService(
            _tokenService,
            _refreshTokenService,
            _otpService,
            _userRepo
        );
    }
}