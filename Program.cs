using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Infra.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContexto>(options => {
  options.UseMySql(
    builder.Configuration.GetConnectionString("mysql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
  );
});
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapPost("/login", (LoginDTO loginDTO) =>{
  if(loginDTO.Email == "adm@test.com" && loginDTO.Senha == "123")
    return Results.Ok("Login com sucesso");
  else
    return Results.Unauthorized();
});



app.Run();


