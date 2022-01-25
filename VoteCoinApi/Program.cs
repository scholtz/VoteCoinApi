var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<VoteCoinApi.Repository.SpaceRepository>();
builder.Services.Configure<VoteCoinApi.Model.ApiConfig>(
    builder.Configuration.GetSection("api"));
builder.Services.AddResponseCaching();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://localhost:8080", "https://localhost:44363/", "https://app.vote-coin.com");
        });
});
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();
app.UseCors();

if (Directory.Exists("/app"))
{
    File.WriteAllText("/app/ready.txt", DateTimeOffset.Now.ToString("R"));
}
app.Run();
