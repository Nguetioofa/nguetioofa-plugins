using Projet.API.Extensions;
using Projet.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseAutofacContainer();

builder.Services
    .AddApiRequestLimits()
    .AddApiRequestContext()
    .AddApiLocalization()
    .AddApiControllers()
    .AddApiValidationErrors()
    .AddApiExceptionHandling()
    .AddApiStatusCodePages()
    .AddApiSwagger();

var app = builder.Build();

// ORDRE VOULU : la culture posée par UseRequestLocalization ne remonte pas à ses appelants (méthode async) ;
// placés avant elle, les deux filets d'erreur écriraient dans la langue de la machine.
app.UseRequestLocalization();
app.UseApiExceptionHandling();
app.UseApiStatusCodePages();

// En production, la documentation décrirait chaque point d'entrée à qui la demande.
if (app.Environment.IsDevelopment())
    app.UseApiSwagger();

app.UseHttpsRedirection();

app.MapApiControllers();

app.Run();
