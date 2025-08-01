using Odin.Core;
using Odin.Services.Drives;
using Odin.Services.Drives.FileSystem.Base.Upload;
using Odin.Services.Peer.Outgoing.Drive;
using Odin.ClientApi.App;
using Odin.Core.Identity;
using Odin.Core.Time;
using Odin.Services.Authorization.Acl;
using Odin.Services.Authorization.ExchangeGrants;

using static InvoiceScheduler.Utils.Utils;
using Odin.Core.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Odin.ClientApi.Universal.Drive;
using Odin.Services.Peer.Encryption;


namespace InvoiceScheduler.Services;

public enum MailDeliveryStatus
{
    NotSent = 10,    // Draft state
    Sending = 15,    // When it's sending; Used for optimistic updates
    Sent = 20,       // when delivered to your identity
    Delivered = 30,  // when delivered to the recipient inbox
    // Read = 40,    // Not supported, we don't want read receipts on mail
    Failed = 50      // when the message failed to send to the recipient
}


public class RichTextNode
{
    public string? Type { get; set; }
    public string? Id { get; set; }
    public string? Value { get; set; }
    public string? Text { get; set; }
    public List<RichTextNode>? Children { get; set; }
}

// RichText is a list of RichTextNode
public class RichText : List<RichTextNode> { }


public class MailConversation
{
    /// <summary>
    /// Stored in content => The origin of the conversation; Created uniquely for each new conversation; And kept the same for each reply/forward
    /// </summary>
    public string OriginId { get; set; } = string.Empty;

    /// <summary>
    /// Stored in groupId => The thread of the conversation; Created uniquely for new conversations and forwards
    /// </summary>
    public string ThreadId { get; set; } = string.Empty;

    // /// <summary>
    // /// Stored in meta => The unique id of the conversation; Created uniquely for each message
    // /// </summary>
    // public string UniqueId { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
    public RichText Message { get; set; } = new RichText();

    /// <summary>
    /// Used purely for search purposes
    /// </summary>
    public string? PlainMessage { get; set; }

    /// <summary>
    /// Used purely for search purposes
    /// </summary>
    public string? PlainAttachment { get; set; }

    /// <summary>
    /// Copy of the senderOdinId which is reset when the recipient marks the message as read => TODO: Remove this in favor of the fileMetadata.originalAuthor
    /// </summary>
    public string Sender { get; set; } = string.Empty;

    public List<string> Recipients { get; set; } = new List<string>();
    public bool? IsRead { get; set; }


    /// <summary>
    /// DeliveryStatus of the message. Indicates if the message is sent, delivered or read
    /// </summary>
    public MailDeliveryStatus DeliveryStatus { get; set; }
}

public class OdinEnvironmentConfiguration
{
    public string ClientAuthToken { get; set; } = string.Empty;
    public string SharedSecret { get; set; } = string.Empty;
    public string Identity { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
}

public class EmailProvider
{
    private const int MailConversationFileType = 9000;
    private const string MailMessagePayloadKey = "mail_invc";
    private readonly Guid _mailAppId = Guid.Parse("6e8ecfff-7c15-40e4-94f4-d6e83bfb5857");

    private static TargetDrive MailDrive => new()
    {
        Alias = Guid.Parse("e69b5a48a663482fbfd846f3b0b143b0"),
        Type = Guid.Parse("2dfecc40311e41e5a12455e925144202")
    };

    /// <summary>
    /// Reads environment configuration from environment variables.
    /// </summary>
    /// <returns>Configuration object with environment values.</returns>
    private static OdinEnvironmentConfiguration GetEnvironmentConfiguration()
    {
        return new OdinEnvironmentConfiguration
        {
            ClientAuthToken = Environment.GetEnvironmentVariable("CLIENTAUTHTOKEN") ??
                throw new InvalidOperationException("CLIENTAUTHTOKEN environment variable is required"),
            SharedSecret = Environment.GetEnvironmentVariable("SHAREDSECRET") ??
                throw new InvalidOperationException("SHAREDSECRET environment variable is required"),
            Identity = Environment.GetEnvironmentVariable("IDENTITY") ??
                throw new InvalidOperationException("IDENTITY environment variable is required"),
            Recipient = Environment.GetEnvironmentVariable("RECIPIENT") ??
                throw new InvalidOperationException("RECIPIENT environment variable is required")
        };
    }

    /// <summary>
    /// Sends an email with the generated invoice PDF as an attachment.
    /// </summary>
    /// <param name="pdfPath">The file path of the PDF to attach.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendInvoiceEmailAsync(string pdfPath)
    {

        // Get environment configuration
        var envConfig = GetEnvironmentConfiguration();
        // Implementation for sending email with the PDF attachment
        Console.WriteLine($"Sending invoice to {envConfig.Recipient} with attachment {pdfPath}");


        var recipientId = new OdinId(envConfig.Recipient);
        var identityId = new OdinId(envConfig.Identity);
        var authToken = ClientAuthenticationToken.FromPortableBytes64(envConfig.ClientAuthToken);
        var sharedSecretBytes = Convert.FromBase64String(envConfig.SharedSecret);
        var cat = new ClientAccessToken()
        {
            Id = authToken.Id,
            AccessTokenHalfKey = authToken.AccessTokenHalfKey,
            ClientTokenType = authToken.ClientTokenType,
            SharedSecret = sharedSecretBytes.ToSensitiveByteArray()
        };
        var client = new OdinApiAppClient(identityId, cat);

        // let's verify the token first
        if (!await client.VerifyToken())
        {
            throw new Exception("Invalid client authentication token");
        }

        var uniqueId = Guid.NewGuid();
        var threadId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var displayName = await GetDisplayNameAsync(recipientId);
        var myName = await GetDisplayNameAsync(identityId);

        var transitOptions = new TransitOptions()
        {
            SendContents = SendContents.All,
            UseAppNotification = true,
            Recipients = [recipientId.ToString()],
            Priority = OutboxPriority.Medium,
            AppNotificationOptions = new AppNotificationOptions()
            {
                AppId = _mailAppId,
                Silent = false,
                TagId = uniqueId,
                TypeId = threadId,
                UnEncryptedMessage = $"{myName} sent you an invoice",
            },
        };


        var message = new RichText {
            new RichTextNode {
                Type = "p",
                Id = "-q_Tm0331n",
                Children = new List<RichTextNode> {
                    new RichTextNode {
                        Text = $"Hi {displayName},\n\nPlease find attached the invoice for this month.\n\nBest regards,\n{myName}"
                    }
                }
            }
        };

        var mailConversation = new MailConversation()
        {
            OriginId = originId.ToString(),
            ThreadId = threadId.ToString(),
            Subject = $"Invoice {GetPreviousMonthDate(DateTime.Now)}",
            Message = message,
            Recipients = [recipientId.DomainName],
            Sender = identityId.DomainName,
            DeliveryStatus = MailDeliveryStatus.Sent,
        };

        // Serialize the mail conversation to JSON
        var mailConversationJson = OdinSystemSerializer.Serialize(mailConversation, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        var shouldEmbedContent = mailConversationJson.ToUtf8ByteArray().Length < MaxHeaderContentBytes;
        var keyHeader = KeyHeader.NewRandom16();

        var payloads = new List<UploadablePayloadDefinition>();
        // Read the PDF file as a byte array
        if (!shouldEmbedContent)
        {
            var uploadablePayload = new UploadablePayloadDefinition
            {
                Key = Defaultpayloadkey,
                ContentType = "application/json",
                Content = mailConversationJson.ToUtf8ByteArray(),
                Thumbnails = [],
                Iv = keyHeader.Iv,
            };
            payloads.Add(uploadablePayload);

        }

        var pdfBytes = await File.ReadAllBytesAsync(pdfPath);

        // Add the PDF as a payload
        var pdfPayload = new UploadablePayloadDefinition
        {
            Key = MailMessagePayloadKey,
            ContentType = "application/pdf",
            Content = pdfBytes,
            Thumbnails = [],
            Iv = keyHeader.Iv,
        };
        payloads.Add(pdfPayload);


        UploadFileMetadata uploadFileMetadata = new()
        {
            IsEncrypted = true,
            AppData = new UploadAppFileMetaData()
            {
                Tags = [originId],
                GroupId = threadId,
                UniqueId = uniqueId,
                FileType = MailConversationFileType,
                UserDate = UnixTimeUtc.Now(),
                Content = shouldEmbedContent ? mailConversationJson : null
            },
            AllowDistribution = true,
            AccessControlList =
            {
                RequiredSecurityGroup = SecurityGroupType.Owner
            },
        };

        UploadManifest uploadManifest = new()
        {
            PayloadDescriptors = payloads.Select(tpd => tpd.ToPayloadDescriptor()).ToList()
        };


        var response = await client.Drive.UploadNewEncryptedFile(MailDrive, keyHeader, uploadFileMetadata, uploadManifest, payloads, transitOptions);
        if (!response.response.IsSuccessStatusCode)
        {

            Console.WriteLine($"Error sending email: {response.response.Error.Content}");

            throw new InvalidOperationException($"Failed to send email: {response.response.ReasonPhrase}");
        }

        // Update the delivery status of the mail conversation
        mailConversation.DeliveryStatus = MailDeliveryStatus.Delivered;

        // Optionally, you can log or return the response
        Console.WriteLine($"Email sent successfully to {envConfig.Recipient} with unique ID: {uniqueId}");

        // Optionally, you can return the mail conversation for further processing
        return;

    }

    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<string> GetDisplayNameAsync(string identity)
    {
        try
        {
            var url = $"https://{identity}/pub/profile";
            var response = await HttpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var name = System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("name").GetString();
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }
        }
        catch
        {
            // Ignore errors, fallback below
        }
        return identity;
    }
}