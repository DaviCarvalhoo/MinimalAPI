using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entitys;
using MinimalAPI.Domain.Enums;
using MinimalAPI.Domain.Interface;
using MinimalAPI.Domain.ModelViews;
using MinimalAPI.Domain.Services;
using MinimalAPI.Infra.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if(string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option => {
  option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>{
  option.TokenValidationParameters = new TokenValidationParameters{
    ValidateLifetime = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
    ValidateIssuer =false,
    ValidateAudience = false
  };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Insira o token JWT aqui: "
  });
  options.AddSecurityRequirement(new OpenApiSecurityRequirement{
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id= "Bearer"
        }
      },
      new string[]{}
    }
  });
});


builder.Services.AddDbContext<DbContexto>(options => {
  options.UseMySql(
    builder.Configuration.GetConnectionString("mysql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
  );
});
var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administrator

string GerarTokenJwt(Administrator administrator){
  if(string.IsNullOrEmpty(key)) return string.Empty;
  var securityKey= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
  var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

  var claims = new List<Claim>()
  {
    new Claim("Email", administrator.Email),
    new Claim("Perfil", administrator.Perfil),
    new Claim(ClaimTypes.Role, administrator.Perfil),
  };
  var token = new JwtSecurityToken(
    claims: claims,
    expires: DateTime.Now.AddDays(1),
    signingCredentials: credentials
  );

  return new JwtSecurityTokenHandler().WriteToken(token);

}

app.MapPost("/login", ([FromBody]LoginDTO loginDTO, IAdministratorService administratorService) =>{
  var adm = administratorService.Login(loginDTO);
  if(adm != null)
  {
    string token = GerarTokenJwt(adm);
    return Results.Ok(new AdmLogado
    {
      Email = adm.Email,
      Perfil = adm.Perfil,
      Token = token
    }
    );
  }
  else
    return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administrators");

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
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{Roles ="Adm"})
.WithTags("Administrators");

app.MapGet("/administradores{id}", ([FromRoute]int id, IAdministratorService administratorService) =>{

  var administrator = administratorService.BuscaPorId(id);
  if(administrator == null) return Results.NotFound();

  return Results.Ok(new AdministratorModelView{
      Id = administrator.Id,
      Email = administrator.Email,
      Perfil = administrator.Perfil
    });
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{Roles ="Adm"})
.WithTags("Administrators");

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
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{Roles ="Adm"})
.WithTags("Administrators");
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
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{Roles ="Adm,Editor"})
.WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculoService veiculoService) =>{

  var veiculos = veiculoService.Todos(pagina);

  return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos{id}", ([FromRoute]int id, IVeiculoService veiculoService) =>{

  var veiculo = veiculoService.BuscaPorId(id);
  if(veiculo == null) return Results.NotFound();

  return Results.Ok(veiculo);
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{Roles ="Adm, Editor"})
.WithTags("Veiculos");

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
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{Roles ="Adm"})
.WithTags("Veiculos");

app.MapDelete("/veiculos{id}", ([FromRoute]int id, IVeiculoService veiculoService) =>{

  var veiculo = veiculoService.BuscaPorId(id);
  if(veiculo == null) return Results.NotFound();

  veiculoService.Apagar(veiculo);

  return Results.NoContent();
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{Roles ="Adm"})
.WithTags("Veiculos");

#endregion

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.Run();


