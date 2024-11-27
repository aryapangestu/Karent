using Karent.DataAccess.Interfaces;
using Karent.DataAccess.NativeQuery;
using Karent.DataModel;
using Karent.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Karent.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalReturnController : ControllerBase
    {
        private readonly IDARentalReturn _rentalReturnService;
        private readonly ILogger<RentalReturnController> _logger;

        public RentalReturnController(IDARentalReturn rentalReturnService, ILogger<RentalReturnController> logger)
        {
            _rentalReturnService = rentalReturnService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                if (User.IsInRole("admin"))
                {
                    var response = await Task.Run(() => _rentalReturnService.GetByFilter(string.Empty));
                    if (response.Data != null && response.Data.Count > 0)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("Failed to get all rental returns: {Message}", response.Message);
                    return NotFound(response);
                }
                else
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    {
                        _logger.LogWarning("Invalid user ID in token.");
                        return Unauthorized("Invalid user ID.");
                    }

                    var response = await Task.Run(() => _rentalReturnService.GetByFilter(string.Empty, userId));
                    if (response.Data != null && response.Data.Count > 0)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("No rental returns found for user ID '{UserId}'.", userId);
                    return NotFound(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rental returns.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpGet("filter/{filter}")]
        public async Task<ActionResult> GetByFilter(string filter)
        {
            try
            {
                if (User.IsInRole("admin"))
                {
                    var response = await Task.Run(() => _rentalReturnService.GetByFilter(filter));
                    if (response.Data != null && response.Data.Count > 0)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("Failed to get rental returns with filter '{Filter}': {Message}", filter, response.Message);
                    return NotFound(response);
                }
                else
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    {
                        _logger.LogWarning("Invalid user ID in token.");
                        return Unauthorized("Invalid user ID.");
                    }

                    var response = await Task.Run(() => _rentalReturnService.GetByFilter(filter, userId));
                    if (response.Data != null && response.Data.Count > 0)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("No rental returns found for user ID '{UserId}' with filter '{Filter}'.", userId, filter);
                    return NotFound(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rental returns by filter '{Filter}'.", filter);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                if (User.IsInRole("admin"))
                {
                    var response = await Task.Run(() => _rentalReturnService.GetById(id));
                    if (response.Data != null)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("Rental return with ID '{Id}' not found: {Message}", id, response.Message);
                    return NotFound(response);
                }
                else
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    {
                        _logger.LogWarning("Invalid user ID in token.");
                        return Unauthorized("Invalid user ID.");
                    }

                    var response = await Task.Run(() => _rentalReturnService.GetById(id, userId));
                    if (response.Data != null)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("Rental return with ID '{Id}' not found for user ID '{UserId}': {Message}", id, userId, response.Message);
                    return NotFound(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rental return by ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create(VMRentalReturn model)
        {
            if (model == null)
            {
                _logger.LogWarning("Create operation received null model.");
                return BadRequest("Rental return model is null.");
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid user ID in token.");
                    return Unauthorized("Invalid user ID.");
                }

                if (!User.IsInRole("admin"))
                {
                    // Pastikan bahwa RentalId milik user yang sedang login
                    var rentalResponse = await Task.Run(() => _rentalReturnService.GetById(model.RentalId, userId));
                    if (rentalResponse.Data == null)
                    {
                        _logger.LogWarning("Rental with ID '{RentalId}' not found for user ID '{UserId}'.", model.RentalId, userId);
                        return NotFound("Rental not found for the current user.");
                    }
                }

                var response = await Task.Run(() => _rentalReturnService.Create(model));

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
                }
                _logger.LogWarning("Failed to create rental return: {Message}", response.Message);
                return BadRequest(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while creating a rental return.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> Update(VMRentalReturn model)
        {
            if (model == null)
            {
                _logger.LogWarning("Update operation received invalid model.");
                return BadRequest("Invalid rental return data.");
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid user ID in token.");
                    return Unauthorized("Invalid user ID.");
                }

                if (!User.IsInRole("admin"))
                {
                    // Pastikan bahwa rental return milik user yang sedang login
                    var existingReturn = await Task.Run(() => _rentalReturnService.GetById(model.Id, userId));
                    if (existingReturn.Data == null)
                    {
                        _logger.LogWarning("Rental return with ID '{Id}' not found for user ID '{UserId}'.", model.Id, userId);
                        return NotFound("Rental return not found for the current user.");
                    }
                }

                var response = await Task.Run(() => _rentalReturnService.Update(model));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Data);
                }
                _logger.LogWarning("Failed to update rental return: {Message}", response.Message);
                return BadRequest(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while updating a rental return.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid user ID in token.");
                    return Unauthorized("Invalid user ID.");
                }

                if (!User.IsInRole("admin"))
                {
                    // Pastikan bahwa rental return milik user yang sedang login
                    var existingReturn = await Task.Run(() => _rentalReturnService.GetById(id, userId));
                    if (existingReturn.Data == null)
                    {
                        _logger.LogWarning("Rental return with ID '{Id}' not found for user ID '{UserId}'.", id, userId);
                        return NotFound("Rental return not found for the current user.");
                    }
                }

                var response = await Task.Run(() => _rentalReturnService.Delete(id));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Message);
                }
                _logger.LogWarning("Failed to delete rental return with ID '{Id}': {Message}", id, response.Message);
                return BadRequest(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deleting rental return with ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
