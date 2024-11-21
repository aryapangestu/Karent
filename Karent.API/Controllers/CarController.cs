using Karent.DataAccess;
using Karent.DataModel;
using Karent.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Karent.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private DACar _carService;
        private readonly ILogger<CarController> _logger;

        public CarController(KarentDBContext db, ILogger<CarController> logger)
        {
            _carService = new DACar(db);
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                VMResponse<List<VMCar>> response = await Task.Run(() => _carService.GetByFilter(string.Empty));

                if (response.Data != null && response.Data.Count > 0)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("CarController.GetAll: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "CarController.GetAll: An error occurred.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("filter/{filter}")]
        public async Task<ActionResult> GetByFilter(string filter)
        {
            try
            {
                VMResponse<List<VMCar>> response = await Task.Run(() => _carService.GetByFilter(filter));

                if (response.Data != null && response.Data.Count > 0)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("CarController.GetByFilter: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "CarController.GetByFilter: An error occurred.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                VMResponse<VMCar> response = await Task.Run(() => _carService.GetById(id));

                if (response.Data != null)
                {
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("CarController.GetById: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "CarController.GetById: An error occurred.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(VMCar model)
        {
            if (model == null)
            {
                _logger.LogWarning("Create operation received null model.");
                return BadRequest("Car model is null.");
            }

            try
            {
                // Trim the input fields to avoid whitespaces
                model.Brand = model.Brand?.Trim();
                model.Model = model.Model?.Trim();
                model.PlateNumber = model.PlateNumber?.Trim();

                VMResponse<VMCar> response = await Task.Run(() => _carService.Create(model));

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
                }
                else
                {
                    _logger.LogWarning("CarController.Create: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "CarController.Create: An error occurred.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPut]
        public async Task<ActionResult> Update(VMCar model)
        {
            if (model == null)
            {
                _logger.LogWarning("Update operation received invalid model.");
                return BadRequest("Invalid car data.");
            }

            try
            {
                // Trim the input fields to avoid whitespaces
                model.Brand = model.Brand?.Trim();
                model.Model = model.Model?.Trim();
                model.PlateNumber = model.PlateNumber?.Trim();

                VMResponse<VMCar> response = await Task.Run(() => _carService.Update(model));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Data);
                }
                else
                {
                    _logger.LogWarning("CarController.Update: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "CarController.Update: An error occurred.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                VMResponse<VMCar> response = await Task.Run(() => _carService.Delete(id));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Message);
                }
                else
                {
                    _logger.LogWarning("CarController.Delete: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "CarController.Delete: An error occurred.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
