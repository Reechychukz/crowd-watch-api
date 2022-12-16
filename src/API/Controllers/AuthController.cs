using Application.DTOs;
using Application.Helpers;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Endpoint to register a user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(SuccessResponse<UserDto>), 201)]
        public async Task<IActionResult> RegisterUser(UserSignupDto model)
        {
            var response = await _userService.CreateUser(model);

            return CreatedAtAction(nameof(GetUserById), new { id = response.Data.Id }, response);
        }

        /// <summary>
        /// Endpoint to verify a user email address
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("complete-registration")]
        [ProducesResponseType(typeof(SuccessResponse<object>), 201)]
        public async Task<IActionResult> ComfirmUserEmailAddres(VerifyTokenDTO model)
        {
            var response = await _userService.CompleteUserRegistration(model);

            return Ok(response);
        }

        /// <summary>
        /// Endpoint to login a user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(SuccessResponse<UserLoginResponse>), 200)]
        public async Task<IActionResult> LoginUser(UserLoginDTO model)
        {
            var response = await _userService.UserLogin(model);

            return Ok(response);
        }

        /// <summary>
        /// Endpoint to get a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = nameof(GetUserById))]
        [ProducesResponseType(typeof(SuccessResponse<UserByIdDto>), 200)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var response = await _userService.GetUserById(id);

            return Ok(response);
        }
    }
}
