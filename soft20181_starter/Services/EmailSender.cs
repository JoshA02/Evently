using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
// using SendGrid;
// using SendGrid.Helpers.Mail;
using Postmark;
using PostmarkDotNet;

namespace soft20181_starter.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;

    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
        ILogger<EmailSender> logger)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
    }
    
    public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.PostmarkToken)) throw new Exception("Null PostmarkToken");
        if (string.IsNullOrEmpty(Options.PostmarkFromEmail)) throw new Exception("Null PostmarkFromEmail");
        
        await Execute(Options.PostmarkToken, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var msg = new PostmarkMessage()
        {
            From = Options.PostmarkFromEmail,
            To = toEmail,
            TrackOpens = false,
            Subject = subject,
            HtmlBody = message
        };

        var client = new PostmarkClient(apiKey);
        var sendResult = await client.SendMessageAsync(msg);
        
        _logger.LogInformation(sendResult.Status == PostmarkStatus.Success
            ? $"Email to {toEmail} queued successfully!"
            : $"Failure Email to {toEmail}");
    }
}