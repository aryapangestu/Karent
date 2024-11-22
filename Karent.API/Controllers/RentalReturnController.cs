using Karent.DataAccess.Interfaces;
using Karent.DataAccess.NativeQuery;
using Karent.DataAccess.ORM;
using Karent.DataModel;
using Karent.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Karent.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalReturnController : ControllerBase
    {
        private IDARentalReturn _rentalReturnService;
        private readonly ILogger<RentalReturnController> _logger;

        public RentalReturnController(KarentDBContext db, ILogger<RentalReturnController> logger)
        {
            //_rentalReturnService = new DARentalReturnOrm(db);
            _rentalReturnService = new DARentalReturnNativeQuery(db);
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                VMResponse<List<VMRentalReturn>> response = await Task.Run(() => _rentalReturnService.GetByFilter(string.Empty));

                if (response.Data != null && response.Data.Count > 0)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to get all rental returns: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching all rental returns.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("filter/{filter}")]
        public async Task<ActionResult> GetByFilter(string filter)
        {
            try
            {
                VMResponse<List<VMRentalReturn>> response = await Task.Run(() => _rentalReturnService.GetByFilter(filter));

                if (response.Data != null && response.Data.Count > 0)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to get rental returns with filter '{Filter}': {Message}", filter, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rental returns by filter '{Filter}'.", filter);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                VMResponse<VMRentalReturn> response = await Task.Run(() => _rentalReturnService.GetById(id));

                if (response.Data != null)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Rental return with ID '{Id}' not found: {Message}", id, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching rental return by ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

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
                VMResponse<VMRentalReturn> response = await Task.Run(() => _rentalReturnService.Create(model));

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
                }
                else
                {
                    _logger.LogWarning("Failed to create rental return: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while creating a rental return.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

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

                VMResponse<VMRentalReturn> response = await Task.Run(() => _rentalReturnService.Update(model));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to update rental return: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while updating a rental return.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                VMResponse<VMRentalReturn> response = await Task.Run(() => _rentalReturnService.Delete(id));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Message);
                }
                else
                {
                    _logger.LogWarning("Failed to delete rental return with ID '{Id}': {Message}", id, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deleting rental return with ID '{Id}'.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
