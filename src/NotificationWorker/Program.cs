using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<RegistrationNotificationConsumer>();
await builder.Build().RunAsync();

public sealed class RegistrationNotificationConsumer(ILogger<RegistrationNotificationConsumer> logger, IConfiguration configuration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "rabbitmq",
            UserName = configuration["RabbitMq:User"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };

        IConnection? connection = null;
        for (var attempt = 1; attempt <= 20 && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try { connection = factory.CreateConnection(); break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "RabbitMQ not ready (attempt {Attempt}/20); retrying.", attempt);
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
        if (connection is null) throw new InvalidOperationException("Could not connect to RabbitMQ.");

        using (connection)
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare("registration.created", durable: true, exclusive: false, autoDelete: false);
            channel.BasicQos(0, 1, false);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (_, args) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(args.Body.ToArray());
                    var message = JsonSerializer.Deserialize<RegistrationCreatedEvent>(json)
                        ?? throw new InvalidOperationException("Invalid registration event.");
                    // Demo delivery writes an audit-style log. Do not send real email/SMS from this sample.
                    logger.LogInformation("NOTIFICATION: registration {RegistrationId} for {CandidateName} ({CandidateNumber}), session {ExamSession}, subjects {Subjects}",
                        message.RegistrationId, message.CandidateName, message.CandidateNumber, message.ExamSession, message.Subjects);
                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Could not process registration event; requeueing.");
                    channel.BasicNack(args.DeliveryTag, false, true);
                }
                await Task.CompletedTask;
            };
            channel.BasicConsume("registration.created", false, consumer);
            while (!stoppingToken.IsCancellationRequested) await Task.Delay(1000, stoppingToken);
        }
    }
}

public sealed record RegistrationCreatedEvent(int RegistrationId, string CandidateNumber, string CandidateName,
    string ExamSession, string Subjects, DateTime CreatedAtUtc);