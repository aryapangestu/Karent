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
    public class RentalController : ControllerBase
    {
        private IDARental _rentalService;
        private readonly ILogger<RentalController> _logger;

        // Konstruktor untuk menginisialisasi layanan data akses dan logger
        public RentalController(IDARental rentalService, ILogger<RentalController> logger)
        {
            _rentalService = rentalService;
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
                    var response = await Task.Run(() => _rentalService.GetByFilter(string.Empty));
                    if (response.Data != null && response.Data.Count > 0)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("Failed to get all rentals: {Message}", response.Message);
                    return NotFound(response);
                }
                else
                {
                    // Mendapatkan userId dari klaim token
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    {
                        _logger.LogWarning("Invalid user ID in token.");
                        return Unauthorized("Invalid user ID.");
                    }

                    var response = await Task.Run(() => _rentalService.GetByFilter(string.Empty, userId));
                    if (response.Data != null && response.Data.Count > 0)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("No rentals found for user ID '{UserId}'.", userId);
                    return NotFound(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rentals.");
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
                    var response = await Task.Run(() => _rentalService.GetByFilter(filter));
                    if (response.Data != null && response.Data.Count > 0)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("Failed to get rentals with filter '{Filter}': {Message}", filter, response.Message);
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

                    var response = await Task.Run(() => _rentalService.GetByFilter(filter, userId));
                    if (response.Data != null && response.Data.Count > 0)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("No rentals found for user ID '{UserId}' with filter '{Filter}'.", userId, filter);
                    return NotFound(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rentals by filter '{Filter}'.", filter);
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
                    var response = await Task.Run(() => _rentalService.GetById(id));
                    if (response.Data != null)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("Rental with ID '{Id}' not found: {Message}", id, response.Message);
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

                    var response = await Task.Run(() => _rentalService.GetById(id, userId));
                    if (response.Data != null)
                    {
                        return Ok(response);
                    }
                    _logger.LogWarning("Rental with ID '{Id}' not found for user ID '{UserId}': {Message}", id, userId, response.Message);
                    return NotFound(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rental by ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create(VMRental model)
        {
            if (model == null)
            {
                _logger.LogWarning("Create operation received null model.");
                return BadRequest("Rental model is null.");
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
                    // Pastikan bahwa UserId pada model adalah user yang sedang login
                    model.UserId = userId;
                }

                var response = await Task.Run(() => _rentalService.Create(model));

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
                }
                _logger.LogWarning("Failed to create rental: {Message}", response.Message);
                return BadRequest(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while creating a rental.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> Update(VMRental model)
        {
            if (model == null)
            {
                _logger.LogWarning("Update operation received null model.");
                return BadRequest("Invalid rental data.");
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
                    // Pastikan bahwa user hanya dapat mengupdate rental miliknya
                    var existingRental = await Task.Run(() => _rentalService.GetById(model.Id, userId));
                    if (existingRental.Data == null)
                    {
                        _logger.LogWarning("Rental with ID '{Id}' not found for user ID '{UserId}'.", model.Id, userId);
                        return NotFound("Rental not found for the current user.");
                    }
                    model.UserId = userId; // Memastikan UserId tidak berubah
                }

                var response = await Task.Run(() => _rentalService.Update(model));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Data);
                }
                _logger.LogWarning("Failed to update rental: {Message}", response.Message);
                return BadRequest(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while updating a rental.");
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
                    // Pastikan bahwa user hanya dapat menghapus rental miliknya
                    var existingRental = await Task.Run(() => _rentalService.GetById(id, userId));
                    if (existingRental.Data == null)
                    {
                        _logger.LogWarning("Rental with ID '{Id}' not found for user ID '{UserId}'.", id, userId);
                        return NotFound("Rental not found for the current user.");
                    }
                }

                var response = await Task.Run(() => _rentalService.Delete(id));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Message);
                }
                _logger.LogWarning("Failed to delete rental with ID '{Id}': {Message}", id, response.Message);
                return BadRequest(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deleting rental with ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
