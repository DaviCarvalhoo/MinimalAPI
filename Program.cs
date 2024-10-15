using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entitys;
using MinimalAPI.Domain.Enums;
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

app.MapGet("/administradores", ([FromQuery]int? pagina, IAdministratorService administratorService) =>{
  var adms = new List<AdministratorModelView>();
  var administradores = administratorService.Todos(pagina);
  foreach(var adm in administradores)
  {
    adms.Add(new AdministratorModelView{
      Id = adm.Id,
      Email = adm.Email,
      Perfil = adm.Perfil
    });
  }
  return Results.Ok(adms);
}).WithTags("Administrators");

app.MapGet("/administradores{id}", ([FromRoute]int id, IAdministratorService administratorService) =>{

  var administrator = administratorService.BuscaPorId(id);
  if(administrator == null) return Results.NotFound();

  return Results.Ok(new AdministratorModelView{
      Id = administrator.Id,
      Email = administrator.Email,
      Perfil = administrator.Perfil
    });
}).WithTags("Administrators");

app.MapPost("/administradores", ([FromBody]AdministratorDTO administratorDTO, IAdministratorService administratorService) =>{
  var validacao = new ErrosDeValidacao{
    Mensagens = new List<string>()
  };

  if(string.IsNullOrEmpty(administratorDTO.Email))
    validacao.Mensagens.Add("Email não pode ser vazio");
  if(string.IsNullOrEmpty(administratorDTO.Senha))
    validacao.Mensagens.Add("Senha não pode ser vazia");
  if(administratorDTO.Perfil == null)
    validacao.Mensagens.Add("Perfil não pode ser vazio");

  if(validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  var administrator = new Administrator{
    Email = administratorDTO.Email,
    Senha = administratorDTO.Senha,
    Perfil = administratorDTO.Perfil.ToString() ?? Perfil.Editor.ToString(), 
  };

  administratorService.Incluir(administrator);

  return Results.Created($"/administradores/{administrator.Id}", new AdministratorModelView{
      Id = administrator.Id,
      Email = administrator.Email,
      Perfil = administrator.Perfil
    });
}).WithTags("Administrators");
#endregion

#region  Veiculos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
  var validacao = new ErrosDeValidacao{
    Mensagens = new List<string>()
  };
  if(string.IsNullOrEmpty(veiculoDTO.Nome))
    validacao.Mensagens.Add("O nome não pode ser vazio");

  if(string.IsNullOrEmpty(veiculoDTO.Marca))
    validacao.Mensagens.Add("A marca não pode ficar em branco");

  if(veiculoDTO.Ano < 1900)
    validacao.Mensagens.Add("Veículo muito antigo, aceito somente anos superiores a 1900");
  return validacao;
}

app.MapPost("/veiculos", ([FromBody]VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>{
  var validacao = validaDTO(veiculoDTO);
  if(validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

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

  var validacao = validaDTO(veiculoDTO);
  if(validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  

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


