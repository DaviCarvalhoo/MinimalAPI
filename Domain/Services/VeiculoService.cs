using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.Entitys;
using MinimalAPI.Domain.Interface;
using MinimalAPI.Infra.Db;

namespace MinimalAPI.Domain.Services
{
    public class VeiculoService : IVeiculoService
    {
        private readonly DbContexto _context;
        public VeiculoService(DbContexto context)
        {
            _context = context;
        }
        public void Apagar(Veiculo veiculo)
        {
            _context.Veiculos.Remove(veiculo);
            _context.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _context.Veiculos.Update(veiculo);
            _context.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
           return _context.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Incluir(Veiculo veiculo)
        {
            _context.Veiculos.Add(veiculo);
            _context.SaveChanges();
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _context.Veiculos.AsQueryable();
            if(!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome.ToLower()}%"));
            }

            int itensPorPagina = 10;
            if(pagina != null)
            {
                query = query.Skip(((int)pagina -1)* itensPorPagina).Take(itensPorPagina);
            }
            

            return query.ToList();
        }
    }
}