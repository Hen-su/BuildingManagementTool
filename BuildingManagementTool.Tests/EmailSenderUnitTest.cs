using BuildingManagementTool.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManagementTool.Tests
{
    internal class EmailSenderUnitTest
    {
        private EmailSender _emailSender;
        private Mock<ILogger<EmailSender>> _mockLogger;
        private Mock<IOptions<AuthMessageSenderOptions>> _mockOptions;
        private AuthMessageSenderOptions _options;
        [SetUp] public void SetUp() 
        {
            _options = new AuthMessageSenderOptions
            {
                SendGridKey = "test_key",
                FromEmail = "test@example.com"
            };

            _mockLogger = new Mock<ILogger<EmailSender>>();
            _mockOptions = new Mock<IOptions<AuthMessageSenderOptions>>();
            _mockOptions.Setup(o => o.Value).Returns(_options);

            _emailSender = new EmailSender(_mockOptions.Object, _mockLogger.Object);
        }

        [Test]
        public async Task SendEmailAsync_ValidSendGridKey_CallsExecute()
        {
            var emailSenderMock = new Mock<EmailSender>(_mockOptions.Object, _mockLogger.Object);
            emailSenderMock.Setup(es => es.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                           .Returns(Task.CompletedTask)
                           .Verifiable();

            await emailSenderMock.Object.SendEmailAsync(_options.FromEmail, "subject", "message");

            emailSenderMock.Verify(es => es.Execute(_options.SendGridKey, "subject", "message", _options.FromEmail), Times.Once);
        }

        [Test]
        public void SendEmailAsync_NullSendGridKey_ThrowsException()
        {
            _options.SendGridKey = null;

            Assert.ThrowsAsync<Exception>(async () =>
                await _emailSender.SendEmailAsync(_options.FromEmail, "subject", "message"));
        }

    }
}
