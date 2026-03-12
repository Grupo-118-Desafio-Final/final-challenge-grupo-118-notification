using System.Collections.Generic;
using External.User.API.Models;
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

var openTelemetryOptions = builder
    .Configuration
    .GetSection(OpenTelemetryOptions.SectionName)
    .Get<OpenTelemetryOptions>();

builder.ConfigureCommonElements(openTelemetryOptions);

builder.Services.AddHostedService<Worker>();

// Bind das configurações
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
builder.Services.Configure<UserApiSettings>(builder.Configuration.GetSection("UserApi"));
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMqSettings"));

// Registro dos serviços
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ISmsService, SmsNotificationService>();
builder.Services.AddSingleton<TelegramNotificationService>();
builder.Services.AddSingleton<NotificationServiceFactory>();

var host = builder.Build();
host.Run();
