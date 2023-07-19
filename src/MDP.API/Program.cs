using System.Text.Json.Serialization;
using MDP.API.Middleware;
using MDP.Caching;
using MDP.Caching.Contract;
using MDP.Manager;
using MDP.Manager.Contract;
using MDP.OMDb;
using MDP.OMDb.Contract;
using MDP.Videos;
using MDP.Videos.Contract;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<OMDbSettings>()
    .Bind(builder.Configuration.GetSection(OMDbSettings.SectionName));

builder.Services.AddOptions<YoutubeSettings>()
    .Bind(builder.Configuration.GetSection(YoutubeSettings.SectionName));

builder.Services.AddScoped<IYoutubeService, YoutubeService>();
builder.Services.AddScoped<IOMDbService, OMDbService>();
builder.Services.AddScoped<IMovieManager, MovieManager>();
builder.Services.AddSingleton<ICacheClient, CacheClient>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddAutoMapper(new[] { typeof(OMDbProfile).Assembly, typeof(YoutubeProfile).Assembly });

var app = builder.Build();

app.UseAppExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();