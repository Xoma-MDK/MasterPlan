using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseSwaggerForOcelotUI();
app.UseOcelot().Wait();
app.Run();
