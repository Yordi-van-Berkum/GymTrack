using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI.Models;
using WebAPI.Services;


var builder = WebApplication.CreateBuilder(args);


// Registreert de database context.
// Deze wordt gebruikt door Entity Framework Core om met de database te communiceren.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer("name=DefaultConnection");
});


// Registreert ASP.NET Identity.
// Identity beheert gebruikers, wachtwoorden, rollen en accountgegevens.
// De JWT authenticatie wordt apart toegevoegd omdat wij zelf tokens maken.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Zorgt ervoor dat ieder e-mailadres maar één keer gebruikt kan worden.
        options.User.RequireUniqueEmail = true;


        // Instellingen voor wachtwoordbeveiliging.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;


        // Geen e-mailbevestiging nodig om in te loggen.
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();



// Configureert JWT authenticatie.
// Hiermee kan ASP.NET Core controleren of de access token geldig is.
builder.Services
    .AddAuthentication(options =>
    {
        // Geeft aan dat JWT Bearer gebruikt wordt voor authenticatie.
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

        // Geeft aan dat JWT gebruikt wordt wanneer toegang geweigerd wordt.
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Regels waarmee de JWT token gecontroleerd wordt.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Controleert of de token ondertekend is met onze geheime sleutel.
            ValidateIssuerSigningKey = true,

            // Gebruikt dezelfde sleutel waarmee de token in AuthService gemaakt wordt.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),


            // Wij gebruiken geen aparte issuer controle.
            ValidateIssuer = false,


            // Wij gebruiken geen aparte audience controle.
            ValidateAudience = false,


            // Controleert of de token nog geldig is.
            ValidateLifetime = true,


            // Geen extra tijd toestaan nadat de token verlopen is.
            ClockSkew = TimeSpan.Zero
        };
    });



// Activeert authorization.
// Hiermee kunnen controllers gebruikmaken van [Authorize].
builder.Services.AddAuthorization();



// Registreert eigen applicatieservices.
builder.Services.AddScoped<IAuthService, AuthService>();


// Registreert controllers.
builder.Services.AddControllers();


// OpenAPI / Swagger configuratie.
builder.Services.AddOpenApi();



var app = builder.Build();


// Staat requests vanuit Blazor toe.
app.UseCors(policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
});



// Swagger alleen in development omgeving.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "api");
    });
}


// Zorgt ervoor dat HTTPS gebruikt wordt.
app.UseHttpsRedirection();


// Controleert eerst of een gebruiker een geldige JWT token heeft.
app.UseAuthentication();


// Controleert daarna of de gebruiker toegang heeft tot endpoints.
app.UseAuthorization();


// Activeert alle controllers zoals AuthController.
app.MapControllers();


app.Run();