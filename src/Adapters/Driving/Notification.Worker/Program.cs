using System.Collections.Generic;
using final_challenge_grupo_118_notification;
using final_challenge_grupo_118_notification.Models;
using final_challenge_grupo_118_notification.Services;
using final_challenge_grupo_118_notification.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StandardDependencies.Injection;
using StandardDependencies.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<SmsNotificationService>();
builder.Services.AddSingleton<TelegramNotificationService>();
builder.Services.AddSingleton<NotificationServiceFactory>();
// Bind das configurações
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Registro do serviço
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
builder.Services.AddTransient<ISmsService, SmsNotificationService>();

var openTelemetryOptions = new OpenTelemetryOptions
{
    ServiceName = "NotificationWorker",
    ServiceVersion = "1.0.0",
    Url = "https://localhost:4318",
    Exporters = new List<ExporterTypes> { ExporterTypes.OTLP }
};

builder.ConfigureCommonElements(openTelemetryOptions);

var host = builder.Build();
host.Run();
