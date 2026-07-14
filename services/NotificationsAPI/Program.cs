using MassTransit;
using NotificationsAPI;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "NotificationsAPI")
    .WriteTo.Console(new RenderedCompactJsonFormatter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<NotificationLogService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<CorrelationContext>();

builder.Services.AddMassTransit(bus =>
{
    bus.AddConsumer<UserCreatedConsumer>();
    bus.AddConsumer<PaymentProcessedConsumer>();

    bus.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", host =>
        {
            host.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            host.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
        cfg.ReceiveEndpoint(builder.Configuration["RabbitMq:UserCreatedQueue"] ?? "notifications-user-created", endpoint =>
        {
            endpoint.ConfigureConsumer<UserCreatedConsumer>(context);
        });
        cfg.ReceiveEndpoint(builder.Configuration["RabbitMq:PaymentProcessedQueue"] ?? "notifications-payment-processed", endpoint =>
        {
            endpoint.ConfigureConsumer<PaymentProcessedConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "NotificationsAPI" }));

app.Run();

public partial class Program;
