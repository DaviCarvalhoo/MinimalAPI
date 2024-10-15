using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalAPI.Domain.Enums;

namespace MinimalAPI.Domain.ModelViews
{
    public record AdministratorModelView
    {
        public int Id { get; set; }= default!;
        public string Email { get; set; } = default!;
        public string Perfil { get; set; } = default!;
    }
}