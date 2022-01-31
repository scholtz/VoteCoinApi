var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<VoteCoinApi.Repository.SpaceRepository>();
builder.Services.Configure<VoteCoinApi.Model.Config.ApiConfig>(
    builder.Configuration.GetSection("api"));
builder.Services.Configure<VoteCoinApi.Model.Config.AlgodConfig>(
    builder.Configuration.GetSection("algod"));
builder.Services.AddResponseCaching();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "VoteCoinPolicy",
        builder =>
        {
            builder.WithOrigins("http://localhost:8080", "https://localhost:44363", "https://app.vote-coin.com", "https://demo.vote-coin.com");
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
            builder.AllowCredentials();
        });
});
var app = builder.Build();

var repo = app.Services.GetService<VoteCoinApi.Repository.SpaceRepository>();
_ = repo.List("mainnet");// init repo

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();
app.UseCors("VoteCoinPolicy");

if (Directory.Exists("/app"))
{
    File.WriteAllText("/app/ready.txt", DateTimeOffset.Now.ToString("R"));
}
app.Run();
