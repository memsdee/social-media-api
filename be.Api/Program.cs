using be.Api.Extensions;
using be.Application;
using be.Infrastructure;
using be.Infrastructure.Hubs;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("_myAllowSpecificOrigins");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard();
app.MapControllers();
app.MapHub<MainHub>("hubs");
app.RegisterRecurringJobs();

app.Run();

namespace be.Api
{
    public class Program
    {
    }
}