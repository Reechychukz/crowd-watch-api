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
    public class UserFriendController: ControllerBase
    {
        private readonly IUserService _userService;

        public UserFriendController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Endpoint to send a friend request
        /// </summary>
        /// <param name="loggedInUserId"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        //[AllowAnonymous]
        [HttpPost("send-friend-request")]
        [ProducesResponseType(typeof(SuccessResponse<object>), 200)]
        public async Task<IActionResult> SendFriendRequest(Guid loggedInUserId, string emailAddress)
        {
            var response = await _userService.AddFriend(loggedInUserId, emailAddress);

            return Ok(response);
        }
    }
}
