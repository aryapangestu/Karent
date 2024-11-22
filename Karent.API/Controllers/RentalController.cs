using Karent.DataAccess.ORM;
using Karent.DataModel;
using Karent.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Karent.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private DARentalOrm _rentalService;
        private readonly ILogger<RentalController> _logger;

        public RentalController(KarentDBContext db, ILogger<RentalController> logger)
        {
            _rentalService = new DARentalOrm(db);
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                VMResponse<List<VMRental>> response = await Task.Run(() => _rentalService.GetByFilter(string.Empty));

                if (response.Data != null && response.Data.Count > 0)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to get all rentals: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching all rentals.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("filter/{filter}")]
        public async Task<ActionResult> GetByFilter(string filter)
        {
            try
            {
                VMResponse<List<VMRental>> response = await Task.Run(() => _rentalService.GetByFilter(filter));

                if (response.Data != null && response.Data.Count > 0)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to get rentals with filter '{Filter}': {Message}", filter, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rentals by filter '{Filter}'.", filter);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                VMResponse<VMRental> response = await Task.Run(() => _rentalService.GetById(id));

                if (response.Data != null)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Rental with ID '{Id}' not found: {Message}", id, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rental by ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

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
                VMResponse<VMRental> response = await Task.Run(() => _rentalService.Create(model));

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
                }
                else
                {
                    _logger.LogWarning("Failed to create rental: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while creating a rental.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPut]
        public async Task<ActionResult> Update(VMRental model)
        {
            if (model == null)
            {
                _logger.LogWarning("Update operation received invalid model.");
                return BadRequest("Invalid rental data.");
            }

            try
            {

                VMResponse<VMRental> response = await Task.Run(() => _rentalService.Update(model));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to update rental: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while updating a rental.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                VMResponse<VMRental> response = await Task.Run(() => _rentalService.Delete(id));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Message);
                }
                else
                {
                    _logger.LogWarning("Failed to delete rental with ID '{Id}': {Message}", id, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deleting rental with ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
