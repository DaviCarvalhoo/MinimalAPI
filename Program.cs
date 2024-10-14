using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entitys;
using MinimalAPI.Domain.Interface;
using MinimalAPI.Domain.ModelViews;
using MinimalAPI.Domain.Services;
using MinimalAPI.Infra.Db;

#region Builder
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
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administrator

app.MapPost("/login", ([FromBody]LoginDTO loginDTO, IAdministratorService administratorService) =>{
  if(administratorService.Login(loginDTO) != null)
    return Results.Ok("Login com sucesso");
  else
    return Results.Unauthorized();
}).WithTags("Administrators");
#endregion

#region  Veiculos
app.MapPost("/veiculos", ([FromBody]VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>{

  var veiculo = new Veiculo{
    Nome = veiculoDTO.Nome,
    Marca = veiculoDTO.Marca,
    Ano = veiculoDTO.Ano,
  };

  veiculoService.Incluir(veiculo);

  return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculoService veiculoService) =>{

  var veiculos = veiculoService.Todos(pagina);

  return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos{id}", ([FromRoute]int id, IVeiculoService veiculoService) =>{

  var veiculo = veiculoService.BuscaPorId(id);
  if(veiculo == null) return Results.NotFound();

  return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos{id}", ([FromRoute]int id,VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>{

  var veiculo = veiculoService.BuscaPorId(id);
  if(veiculo == null) return Results.NotFound();

  veiculo.Nome = veiculoDTO.Nome;
  veiculo.Marca = veiculoDTO.Marca;
  veiculo.Ano = veiculoDTO.Ano;

  veiculoService.Atualizar(veiculo);

  return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos{id}", ([FromRoute]int id, IVeiculoService veiculoService) =>{

  var veiculo = veiculoService.BuscaPorId(id);
  if(veiculo == null) return Results.NotFound();

  veiculoService.Apagar(veiculo);

  return Results.NoContent();
}).WithTags("Veiculos");

#endregion

app.UseSwagger();
app.UseSwaggerUI();
app.Run();


