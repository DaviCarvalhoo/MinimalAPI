using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entitys;
using MinimalAPI.Domain.Interface;
using MinimalAPI.Infra.Db;

namespace MinimalAPI.Domain.Interface
{
    public class AdministratorService : IAdministratorService
    {
        private readonly DbContexto _context;
        public AdministratorService(DbContexto context)
        {
            _context = context;
        }

        public Administrator? BuscaPorId(int id)
        {
            return _context.Administrators.Where(v => v.Id == id).FirstOrDefault();
        }

        public Administrator? Incluir(Administrator administrator)
        {
            _context.Administrators.Add(administrator);
            _context.SaveChanges();

            return administrator;
        }

        public Administrator? Login(LoginDTO loginDTO)
        {
            var adm = _context.Administrators.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
        }

        public List<Administrator> Todos(int? pagina)
        {
            var query = _context.Administrators.AsQueryable();
            
            int itensPorPagina = 10;
            if(pagina != null)
            {
                query = query.Skip(((int)pagina -1)* itensPorPagina).Take(itensPorPagina);
            }
            

            return query.ToList();
        }
    }
}