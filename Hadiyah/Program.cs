using HadiyahMigrations;
using HadiyahRepositories.Implementation;
using HadiyahRepositories.Interfaces;
using HadiyahServices.Implementation;
using HadiyahServices.Interfaces;
using HadiyahServices.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
// DI
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();


builder.Services.AddHttpContextAccessor();

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                var returnUrl = context.Request.Path + context.Request.QueryString;
                var encoded = Uri.EscapeDataString(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
                context.Response.Redirect($"/User/Login?returnUrl={encoded}&reason=unauthorized");
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                var returnUrl = context.Request.Path + context.Request.QueryString;
                var encoded = Uri.EscapeDataString(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
                context.Response.Redirect($"/User/Login?returnUrl={encoded}&reason=forbidden");
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddControllersWithViews();
builder.Services.AddSession();
var app = builder.Build();

app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
// Automatically attach JWT token from cookie
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["jwt"];
    if (!string.IsNullOrEmpty(token))
        context.Request.Headers.Add("Authorization", $"Bearer {token}");

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Default route: User/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Shop}/{action=Index}/{id?}"
);

app.Run();
