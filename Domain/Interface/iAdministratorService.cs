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
    }
}