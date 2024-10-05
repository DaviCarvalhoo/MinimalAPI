using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.Entitys;

namespace MinimalAPI.Infra.Db
{
    public class DbContexto : DbContext
    {
        private readonly IConfiguration _configuracaoAppSettings;
        public DbContexto(IConfiguration configuracaoAppSetings)
        {
            _configuracaoAppSettings = configuracaoAppSetings;
        }
        public DbSet<Administrator> Administrators {get; set;} = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrator>().HasData(
                new Administrator{
                    Id = 1,
                    Email = "admin@admin.com",
                    Senha = "admin",
                    Perfil = "Adm"

                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                var stringConexao = _configuracaoAppSettings.GetConnectionString("mysql")?.ToString();
                if(!string.IsNullOrEmpty(stringConexao))
                {
                    optionsBuilder.UseMySql(
                        stringConexao, 
                        ServerVersion.AutoDetect(stringConexao)
                    );
                }
            }
            
            
        }
    }
}