using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/v{version:apiVersion}/UsersAuth")]
    //lets assume that the logic of this controller doesnt change
    //in that case it will remain like a neutral version, without changes
    //this controller will display in both versions
    [ApiVersionNeutral]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly APIResponse apiResponse;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            apiResponse = new APIResponse();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var loginResponse = await _userRepository.Login(model);
            if (loginResponse.User == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResponse.IsSuccess = false;
                apiResponse.ErrorMessages.Add("Username or password is incorrect");
                return BadRequest(apiResponse);

            }
            apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
            apiResponse.IsSuccess = true;
            apiResponse.Result = loginResponse;
            return Ok(apiResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
        {
            bool ifUserNameUnique = _userRepository.IsUniqueUser(model.UserName);
            if (!ifUserNameUnique)
            {
                apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResponse.IsSuccess = false;
                apiResponse.ErrorMessages.Add("Username already exists");
                return BadRequest(apiResponse);
            }

            var user = await _userRepository.Register(model);

            if (user == null)
            {
                apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResponse.IsSuccess = false;
                apiResponse.ErrorMessages.Add("Error while registering");
                return BadRequest(apiResponse);
            }

            apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
            apiResponse.IsSuccess = true;
            return Ok(apiResponse);
        }
    }
}
