using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }
    public class UserSignupDto
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ComfirmPassword { get; set; }
    }  

    public class UserByIdDto : UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public bool Verified { get; set; }
        public IList<string> Roles { get; set; }
    }
}