using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Transversal.DTOs;
using Transversal.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Transversal.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region Usuario
            CreateMap<Usuario, UsuarioRegistroRequestDTO>().ReverseMap();

            CreateMap<Usuario, UsuarioLoginRequestDTO>().ReverseMap();

            CreateMap<Usuario, UsuarioResponseDTO>()
                .ForMember(d => d.NombreUsuario, accion => accion.MapFrom(o => o.NombreApellido))
                .ReverseMap()
                .ForMember(d => d.NombreApellido, accion => accion.MapFrom(o => o.NombreUsuario));

            CreateMap<UsuarioGoogleRequestDTO, Usuario>()
                .ForMember(d => d.NombreApellido, accion => accion.MapFrom(o => o.Nombre))
                .ReverseMap()
                .ForMember(d => d.Nombre, accion => accion.MapFrom(o => o.NombreApellido));
            #endregion Usuario
        }

    }
}