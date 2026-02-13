using MailerSend.AspNetCore;

namespace VetFlow.Server.Services
{
    public interface IMailerSendService     // To allow mocking in unit tests
    {
        Task SendMailAsync(
            IEnumerable<Recipient> to,
            IEnumerable<Recipient>? cc = null,
            IEnumerable<Recipient>? bcc = null,
            string? subject = null,
            string? text = null,
            string? html = null,
            string? templateId = null,
            IEnumerable<Attachment>? attachments = null,
            DateTime? sendAt = null,
            CancellationToken cancellationToken = default);
    }
}
