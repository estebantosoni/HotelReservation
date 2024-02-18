
using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>();

builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//enable caching to avoid going to database when the request are allways to the same endpoint
builder.Services.AddResponseCaching();

//global support for versioning
//note: the packages has been moved to another, so these packages are now deprecated

//specify version
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    //show in swagger the api versions available for endpoints when i make a request
    options.ReportApiVersions = true;
});

//suppot for many versioning endpoints in a controller
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    //replace v{version} in path on swagger for the current version (by default)
    options.SubstituteApiVersionInUrl = true;
});


var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

//default config for Authentication
// a package needs to be installed
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers(options =>
{
    //instead of set each cache option on each controller, you can create a caching profile

    options.CacheProfiles.Add("Default30",
        new CacheProfile()
        {
            Duration = 30
        });

    //options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//config for swagger
builder.Services.AddSwaggerGen(options =>
{
    //how your API is protected
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Auth using Bearer. \r\n\r\n" +
            "Enter 'Bearer' [space] 'YourToken' in the input. \r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        //key (like Postman)
        Name = "Authorization",
        //the token must be added in the header
        In = ParameterLocation.Header,
        //value
        Scheme = "Bearer"
    });

    //global security config
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        //objects list
        { 
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            //for key-value pair
            new List<string>()
        }
    });
    
    //modify swagger doc to change UI
    options.SwaggerDoc("v1",new OpenApiInfo
    {
        Version = "v1",
        Title = "Magic Villa V1",
        Description = "API to manage Villa",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Styvien",
            Url = new Uri("https://estebantosoni.vercel.app")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "Magic Villa V2",
        Description = "API to manage Villa",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Styvien",
            Url = new Uri("https://estebantosoni.vercel.app")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});

builder.Services.AddAutoMapper(typeof(MappingConfig));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    //set config to UI
    app.UseSwaggerUI(options =>
    {
        //set support for many versioning: this will allows to choose the version that we want
        options.SwaggerEndpoint("/swagger/v1/swagger.json","Magic_VillaV1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Magic_VillaV2");

    });
}

app.UseHttpsRedirection();

//add auth injection to use jwt tokens and more
//UseAuthentication() must be used before UseAuthorization(), because the user must be authenticated first
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
