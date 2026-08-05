using Gaia.Api.Errors;
using Gaia.Operations;
using Gaia.Operations.BuiltIn;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
const string DevelopmentClientCorsPolicy = "DevelopmentClient";
if (builder.Environment.IsDevelopment())
{
    var allowedOrigin =
        builder.Configuration["Cors:AllowedOrigin"]
        ?? throw new InvalidOperationException("Development CORS origin is not configured.");

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            DevelopmentClientCorsPolicy,
            policy =>
            {
                policy.WithOrigins(allowedOrigin).AllowAnyHeader().AllowAnyMethod();
            }
        );
    });
}

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UnexpectedExceptionHandler>();

builder.Services.AddSingleton<IBinaryOperation, AddOperation>();
builder.Services.AddSingleton<IBinaryOperation, SubtractOperation>();
builder.Services.AddSingleton<IBinaryOperation, ConcatenateOperation>();
builder.Services.AddSingleton<IBinaryOperation, ContainsOperation>();
builder.Services.AddSingleton<OperationExecutor>();

var app = builder.Build();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors(DevelopmentClientCorsPolicy);
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
