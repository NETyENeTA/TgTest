using Telegram.Bot;
using Telegram.Bot.Examples.Minimal.API;
using Telegram.Bot.Examples.Minimal.API.Services;

var builder = WebApplication.CreateBuilder(args);

var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
var botToken = botConfig.BotToken ?? string.Empty;

builder.Services.AddHostedService<ConfigureWebhook>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("TelegramWebhook")
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botToken, httpClient));

// Dummy business-logic service
builder.Services.AddScoped<HandleUpdateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHttpLogging();

// Configure custom endpoint per Telegram API recommendations:
app.MapPost($"/bot/{botConfig.EscapedBotToken}", async (
    ITelegramBotClient botClient,
    HttpRequest request,
    HandleUpdateService handleUpdateService,
    NewtonsoftJsonUpdate update) => {
    if (update.Message == null) throw new ArgumentException(nameof(update.Message));

    await handleUpdateService.EchoAsync(update);
    return Results.Ok();
}).WithName("TelegramWebhook");

app.Run();