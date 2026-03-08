using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UserAuth.Api.Data;
using UserAuth.Api.Interfaces.Service;
using UserAuth.Api.Repository.InMemory;
using UserAuth.Api.Services;

namespace UserAuthWithOTP.Unit.Common
{
    public abstract class OtpServiceTestBase
    {
        protected InMemoryOtpVerificationRepository _otpVerificationRepository = null!;
        protected InMemoryUserRepo _userRepo = null!;

        protected IEmailService _emailService = null!;


        protected IOtpService _otpService = null!;
        protected Mock<IConfiguration> ConfigMock { get; private set; } = null!;

        [TestInitialize]
        public void TestInit()
        {
            var dbContext = new InMemoryDbContext();

            _userRepo = new InMemoryUserRepo(dbContext);
            _otpVerificationRepository = new InMemoryOtpVerificationRepository(dbContext);

            var emailLogger = new Mock<ILogger<EmailService>>();
            var otpLogger = new Mock<ILogger<OtpService>>();

            ConfigMock = new Mock<IConfiguration>();

            var emailSection = new Mock<IConfigurationSection>();

            emailSection.Setup(s => s["SmtpServer"]).Returns("smtp.gmail.com");
            emailSection.Setup(s => s["Port"]).Returns("587");
            emailSection.Setup(s => s["SenderEmail"]).Returns("");
            emailSection.Setup(s => s["SenderPassword"]).Returns("");
            emailSection.Setup(s => s["SenderName"]).Returns("");

            ConfigMock.Setup(c => c.GetSection("EmailSettings")).Returns(emailSection.Object);

            _emailService = new EmailService(ConfigMock.Object, emailLogger.Object);
            _otpService = new OtpService(_otpVerificationRepository, _emailService, otpLogger.Object, _userRepo);

        }
    }
}
