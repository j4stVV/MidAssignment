using Lib.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Lib.Domain.Entities;
using Lib.Application.Abstractions.Services;
using Lib.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LibraryManament.Extensions;
using Lib.Domain.Repositories;
using Lib.Persistence.Repositories;
using LibraryManament.Middleware;
using Lib.Infrastructure.Mapping;
using FluentValidation;
using Lib.Application.Validators.BookValidators;

namespace LibraryManament;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Logging
        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
        });

        // Add DbContext
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add Identity with User and Role
        builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 6;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Add Authentication with JWT
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateActor = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Value,
                    ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes
                    (builder.Configuration.GetSection("Jwt:Key").Value!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Add Authorization
        builder.Services.AddAuthorization();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
        });

        // Register Repositories
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IBookRepository, BookRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IBookBorrowingRequestRepository, BookBorrowingRequestRepository>();
        builder.Services.AddScoped<IBookBorrowingRequestDetailsRepository, BookBorrowingRequestDetailsRepository>();

        //Register Services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        builder.Services.AddScoped<IBookService, BookService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IBookBorrowingRequestService, BookBorrowingRequestService>();

        // Add AutoMapper
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        // Add FluentValidation
        //builder.Services.AddScoped<IValidator<Book>, BookValidator>();
        //builder.Services.AddScoped<IValidator<CreateBookDto>, CreateBookDtoValidator>();
        //builder.Services.AddScoped<IValidator<UpdateBookDto>, UpdateBookDtoValidator>();
        //builder.Services.AddScoped<IValidator<Category>, CategoryValidator>();
        //builder.Services.AddScoped<IValidator<BookBorrowingRequest>, BookBorrowingRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateBookDtoValidator>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Add Exception Handling Middleware
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.ApplyMigrations();
        }

        app.UseCors("AllowAll");

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DataSeeder.SeedDataAsync(context);

            await RoleInitializer.InitializeAsync(scope.ServiceProvider);
        }

        app.Run();
    }
}
