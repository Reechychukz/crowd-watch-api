using Application.DTOs;
using AutoMapper;
using Domain.Entities.Identities;

namespace Application.Mapper
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<UserSignupDto, User>().AfterMap((src, dest) =>
            {
                dest.Email = src.Email.Trim().ToLower();
            });

            CreateMap<User, UserByIdDto>();
            CreateMap<User, UserDto>();
        }
    }
}
