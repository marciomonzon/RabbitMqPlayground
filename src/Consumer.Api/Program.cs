using Consumer.Api.Messaging.Interfaces;
using Consumer.Api.Messaging.Services;
using Consumer.Api.Messaging.Options;
using Consumer.Api.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .Configure<RabbitMqOptions>(
        builder.Configuration
        .GetSection(RabbitMqOptions.SectionName));


builder.Services
    .AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

builder.Services.AddHostedService<OrderConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();