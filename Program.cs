using Microsoft.EntityFrameworkCore;
using Repaso3.Components;
using Repaso3.DAL;
using Repaso3.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var ConnectionString = builder.Configuration.GetConnectionString("ConStr");

builder.Services.AddDbContextFactory<Contexto>(o => o.UseSqlite(ConnectionString));

builder.Services.AddScoped<PedidoServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
