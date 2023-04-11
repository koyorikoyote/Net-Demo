using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.FileProviders;
using SeatPlan;
using System.Security.Principal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSession();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

DBConfig.ConnectionString = builder.Configuration["ConnectionStrings:Dev"];
if (builder.Environment.IsStaging())
{
    DBConfig.ConnectionString = builder.Configuration["ConnectionStrings:PreProd"];
}
else if (builder.Environment.IsProduction())
{
    DBConfig.ConnectionString = builder.Configuration["ConnectionStrings:Prod"];
}

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//}

HttpHelper.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());
app.UseSession();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();