using MailerSend.AspNetCore;

namespace VetFlow.Server.Services
{
    public class MailerSendServiceWrapper : IMailerSendService      // To allow mocking in unit tests
    {
        private readonly MailerSendService _mailerSendService;

        public MailerSendServiceWrapper(MailerSendService mailerSendService)
        {
            _mailerSendService = mailerSendService;
        }

        public Task SendMailAsync(
            IEnumerable<Recipient> to,
            IEnumerable<Recipient>? cc = null,
            IEnumerable<Recipient>? bcc = null,
            string? subject = null,
            string? text = null,
            string? html = null,
            string? templateId = null,
            IEnumerable<Attachment>? attachments = null,
            DateTime? sendAt = null,
            CancellationToken cancellationToken = default)
        {
            return _mailerSendService.SendMailAsync(
                to, cc, bcc, subject, text, html,
                templateId, attachments, sendAt, cancellationToken);
        }
    }
}
