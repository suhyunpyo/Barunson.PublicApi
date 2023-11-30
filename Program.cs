using Azure.Identity;
using Barunson.DbContext;
using Barunson.PublicApi.Config;
using Barunson.PublicApi.Filter;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text;


namespace Barunson.PublicApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Application gateway x-forword-for ����
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.ForwardLimit = 5;
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.100.0"), 24));
            });
            #endregion

            if (builder.Environment.IsProduction())
            {
                builder.Configuration.AddAzureKeyVault(
                    new Uri($"https://barunsecret.vault.azure.net/"),
                    new DefaultAzureCredential());
            }
            else
            {
                //���� ȯ�濡���� ���
                builder.Configuration.AddAzureKeyVault(
                     new Uri($"https://dev-barunsecret.vault.azure.net/"),
                     new DefaultAzureCredential());
                IdentityModelEventSource.ShowPII = true;
            }
            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            var securitySchema = new OpenApiSecurityScheme
            {
                Description = "������ ���� �������� JWT Authorization header�� ��ū�� �������� �մϴ�.<br /> \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };
           
           
            builder.Services.AddSwaggerGen(c =>
            {
                c.OperationFilter<OperationAuthorizationFilter>();
                c.AddSecurityDefinition("Bearer", securitySchema);
  
            });

            var barunJwt = builder.Configuration.GetSection("BarunApiJwt").Get<BarunApiJwt>();
            builder.Services.AddSingleton(barunJwt);

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = barunJwt.Authority;
                    options.Configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = barunJwt.Issuer,
                        ValidAudience = barunJwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(barunJwt.SecretKey)),
                    };
                });

            #region DB Context 

            builder.Services.AddDbContext<BarunsonContext>(options => 
                options.UseSqlServer(builder.Configuration.GetConnectionString("BarunsonDBConn")));
            builder.Services.AddDbContext<BarShopContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("BarShopDBConn")));
            builder.Services.AddDbContext<DearDeerContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DearDeerDBConn"),
                    new MariaDbServerVersion(new Version(10, 1))
                    )
                );           

            #endregion

            builder.Services.AddHealthChecks();

            var app = builder.Build();
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //X-Forward heder ���
            app.UseForwardedHeaders();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            //���� �˻�
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}