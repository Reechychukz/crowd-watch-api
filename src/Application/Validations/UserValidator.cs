using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations
{
    public class UserValidator //: AbstractValidator<CreateUserDTO>
    {
        public UserValidator()
        {
            //RuleFor(x => x.Email).NotEmpty().EmailAddress();
            //RuleFor(x => x.FirstName).NotEmpty();
            //RuleFor(x => x.LastName).NotEmpty();
            //RuleFor(x => x.Password).Matches(@"(?-i)(?=^.{8,}$)((?!.*\s)(?=.*[A-Z])(?=.*[a-z]))((?=(.*\d){1,})|(?=(.*\W){1,}))^.*$")
            //    .WithMessage(@"Password must be at least 8 characters, at least 1 upper case letters (A – Z), Atleast 1 lower case letters (a – z), Atleast 1 number (0 – 9) or non-alphanumeric symbol (e.g. @ '$%£! ')");
        }
    }
}
