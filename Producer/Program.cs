using Producer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<KafkaService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/test", () =>
{
    return Results.Ok($"Testing!");
})
.WithOpenApi();

app.MapPost("/message", (KafkaService kafka, string value) =>
{
    var correlationId = Guid.NewGuid().ToString();
    kafka.SendMessage(correlationId, value);
    var response = kafka.ConsumeMessage(correlationId);
    return Results.Ok(response);
})
.WithOpenApi();

app.Run();