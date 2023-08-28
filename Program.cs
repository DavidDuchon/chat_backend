using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

using chat_backend.Services;
using chat_backend.Options;
using chat_backend.Data;
using chat_backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(
    options => 
    {
        options.AddDefaultPolicy(policy => {
            policy.WithOrigins("http://localhost:4200").SetIsOriginAllowedToAllowWildcardSubdomains().AllowCredentials().AllowAnyHeader();
        });
    }
);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));

builder.Services.AddScoped<IJWTService,JWTService>();

var tokenValidationParameters = new TokenValidationParameters {
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JWT:JWTKey"))),
        RequireAudience = false,
        ValidateAudience=false,
        ValidateIssuer=false,
        
    };

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("refresh",options => {
        options.TokenValidationParameters = tokenValidationParameters;
        options.Events = new JwtBearerEvents{
            OnMessageReceived = (MessageReceivedContext context) =>
            {
                if (context.HttpContext.Request.Cookies["refresh"] is not null)
                    context.Token = context.HttpContext.Request.Cookies["refresh"];
                else
                    context.Token = "failed";
                return Task.CompletedTask;
            }
        };
        }
    ).AddJwtBearer("default",options => {
        options.TokenValidationParameters = tokenValidationParameters;
    }).AddJwtBearer("signalr",options => {
        options.TokenValidationParameters = tokenValidationParameters;
        options.Events = new JwtBearerEvents{
            OnMessageReceived = (MessageReceivedContext context) => {
                context.Token = context.HttpContext.Request.Query["access_token"];
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessToken",policy => policy.RequireClaim("User"));
    options.AddPolicy("RefreshToken",policy => policy.RequireClaim("For"));
});

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chat");

app.Run();
