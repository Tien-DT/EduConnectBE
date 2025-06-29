using EduConnect.Application.Services.Class;
using EduConnect.Application.Services.Interface;
using EduConnect.Application.Services.Background;
using EduConnect.Application.Services.Cache;
using EduConnect.Application.Services.Queue;
using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Options;
using EduConnect.Infrastruture.Repositories.Class;
using EduConnect.Infrastruture.Repositories.Interface;
using EduConnect.Infrastruture.UnitOfWork.Class;
using EduConnect.Infrastruture.UnitOfWork.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using EduConnect.Application.Mapping;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;

namespace EduConnect.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddCors();

            builder.Services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "EduConnect", Version = "v1" });
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
            builder.Services.Configure<GeminiOption>(builder.Configuration.GetSection("GeminiApi"));
            builder.Services.Configure<RedisOption>(builder.Configuration.GetSection("Redis"));
            builder.Services.AddMemoryCache();

            // Redis Configuration
            var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });
            }
            else
            {
                // Fallback to in-memory cache if Redis is not configured
                builder.Services.AddMemoryCache();
            }

            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IChatBotService, ChatBotService>();
            builder.Services.AddScoped<IGeminiFunctionService, GeminiFunctionService>();
            builder.Services.AddScoped<IChatCacheService, ChatCacheService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<IExamService, ExamService>();
            builder.Services.AddScoped<IExamRepository, ExamRepository>();
            builder.Services.AddScoped<IClassTeacherRepository, ClassTeacherRepository>();
            builder.Services.AddScoped<IPrincipalRepository, PrincipalRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IBaseRepository<Timetable, int>, BaseRepository<Timetable, int>>();
            builder.Services.AddScoped<IBaseRepository<Class, int>, BaseRepository<Class, int>>();
            builder.Services.AddScoped<ITeacherService, TeacherService>();
            builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
            builder.Services.AddScoped<IParentService, ParentService>();
            builder.Services.AddScoped<IParentRepository, ParentRepository>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<ITimetableSlotRepository, TimetableSlotRepository>();
            builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();
            builder.Services.AddScoped<ITimetableService, TimetableService>();
            builder.Services.AddScoped<IClassService, ClassService>();

            // Email services
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IEmailQueueService, EmailQueueService>();
            builder.Services.AddScoped<IEmailVerificationTokenService, EmailVerificationTokenService>();
            builder.Services.AddHostedService<EmailQueueBackgroundService>();

            builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<ApplicationProfile>(); }, typeof(ApplicationProfile).Assembly);

            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
            builder.Services.AddSingleton(x => {
                var config = x.GetRequiredService<IOptions<CloudinarySettings>>().Value;
                var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
                return new Cloudinary(account);
            });

            builder.Services.AddTransient<EduConnectApplication.Services.Class.FileUploadService>();

            var connectionString = builder.Configuration.GetConnectionString("EduConnectDb");
            builder.Services.AddDbContext<EduConnectDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
                options.TokenLifespan = TimeSpan.FromMinutes(5));
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;
            })
                            .AddDefaultTokenProviders()
                            .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("EduConnect")
                            .AddEntityFrameworkStores<EduConnectDbContext>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var config = builder.Configuration;
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.UseSecurityTokenValidators = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ClockSkew = TimeSpan.FromSeconds(5), 
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true, 
                    ValidateLifetime = true,
                    ValidAudience = config["JwtSettings:Audience"],
                    ValidIssuer = config["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!)),
                    RoleClaimType = ClaimTypes.Role 
                };
            });

            var app = builder.Build();

            app.UseCors(option => option
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Content-Disposition"));

            app.UseSwagger();
            app.UseSwaggerUI();
           
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
