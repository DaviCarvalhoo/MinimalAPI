using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entitys;
using MinimalAPI.Domain.Interface;
using MinimalAPI.Domain.ModelViews;
using MinimalAPI.Domain.Services;
using MinimalAPI.Infra.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options => {
  options.UseMySql(
    builder.Configuration.GetConnectionString("mysql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
  );
});
var app = builder.Build();


app.MapGet("/", () => Results.Json(new Home()));

app.MapPost("/login", ([FromBody]LoginDTO loginDTO, IAdministratorService administratorService) =>{
  if(administratorService.Login(loginDTO) != null)
    return Results.Ok("Login com sucesso");
  else
    return Results.Unauthorized();
});


app.MapPost("/veiculos", ([FromBody]VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>{

  var veiculo = new Veiculo{
    Nome = veiculoDTO.Nome,
    Marca = veiculoDTO.Marca,
    Ano = veiculoDTO.Ano,
  };

  veiculoService.Incluir(veiculo);

  return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
});
app.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculoService veiculoService) =>{

  var veiculos = veiculoService.Todos(pagina);

  return Results.Ok(veiculos);
});

app.UseSwagger();
app.UseSwaggerUI();
app.Run();


