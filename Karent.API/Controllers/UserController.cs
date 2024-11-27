using Karent.API.Helpers;
using Karent.DataAccess.Interfaces;
using Karent.DataAccess.NativeQuery;
using Karent.DataModel;
using Karent.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Karent.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IDAUser _userService;
        private JwtHelper _jwtHelper;
        private readonly ILogger<UserController> _logger;

        public UserController(IDAUser userService, JwtHelper jwtHelper, ILogger<UserController> logger)
        {
            _userService = userService;

            _jwtHelper = jwtHelper;
            _logger = logger;

        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                VMResponse<List<VMUser>> response = await Task.Run(() => _userService.GetByFilter(string.Empty));

                if (response.Data != null && response.Data.Count > 0)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to get all users: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching all users.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("filter/{filter}")]
        public async Task<ActionResult> GetByFilter(string filter)
        {
            try
            {
                VMResponse<List<VMUser>> response = await Task.Run(() => _userService.GetByFilter(filter));

                if (response.Data != null && response.Data.Count > 0)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to get users with filter '{Filter}': {Message}", filter, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching users by filter '{Filter}'.", filter);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                VMResponse<VMUser> response = await Task.Run(() => _userService.GetById(id));

                if (response.Data != null)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("User with ID '{Id}' not found: {Message}", id, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching user by ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login(VMLogin model)
        {
            if (model == null)
            {
                _logger.LogWarning("Login operation received null model.");
                return BadRequest("Login model is null.");
            }

            try
            {
                VMResponse<VMUser> response = await Task.Run(() => _userService.Login(model.Email, model.Password));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var token = _jwtHelper.GenerateToken(response.Data);
                    return Ok(token);
                }
                else
                {
                    _logger.LogWarning("Failed to login user: {Message}", response.Message);
                    return Unauthorized(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while logging in a user.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Create(VMUser model)
        {
            if (model == null)
            {
                _logger.LogWarning("Create operation received null model.");
                return BadRequest("User model is null.");
            }

            try
            {
                // Trim the input fields to avoid whitespaces
                model.Name = model.Name?.Trim();
                model.Email = model.Email?.Trim();
                model.PhoneNumber = model.PhoneNumber?.Trim();
                model.DrivingLicenseNumber = model.DrivingLicenseNumber?.Trim();
                model.Password = model.Password?.Trim();

                VMResponse<VMUser> response = await Task.Run(() => _userService.Create(model));

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
                }
                else
                {
                    _logger.LogWarning("Failed to create user: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while creating a user.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> Update(VMUser model)
        {
            if (model == null)
            {
                _logger.LogWarning("Update operation received invalid model.");
                return BadRequest("Invalid user data.");
            }

            try
            {
                // Trim the input fields to avoid whitespaces
                model.Name = model.Name?.Trim();
                model.Email = model.Email?.Trim();
                model.PhoneNumber = model.PhoneNumber?.Trim();
                model.DrivingLicenseNumber = model.DrivingLicenseNumber?.Trim();
                model.Password = model.Password?.Trim();

                VMResponse<VMUser> response = await Task.Run(() => _userService.Update(model));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to update user: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while updating a user.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                VMResponse<VMUser> response = await Task.Run(() => _userService.Delete(id));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Message);
                }
                else
                {
                    _logger.LogWarning("Failed to delete user with ID '{Id}': {Message}", id, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deleting user with ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
