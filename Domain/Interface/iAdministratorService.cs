using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entitys;

namespace MinimalAPI.Domain.Interface
{
    public interface IAdministratorService
    {
        Administrator? Login(LoginDTO loginDTO);
        Administrator? Incluir(Administrator administrator);
        Administrator? BuscaPorId(int id);
        List<Administrator> Todos(int? pagina );
    }
}