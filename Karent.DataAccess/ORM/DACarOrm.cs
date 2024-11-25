using Karent.DataAccess.Interfaces;
using Karent.DataModel;
using Karent.ViewModel;
using System.Net;

namespace Karent.DataAccess.ORM
{
    public class DACarOrm : IDACar
    {
        private readonly KarentDBContext _db;

        public DACarOrm(KarentDBContext db)
        {
            _db = db;
        }

        public VMResponse<List<VMCar>> GetByFilter(string filter)
        {
            VMResponse<List<VMCar>> response = new VMResponse<List<VMCar>>();

            try
            {
                var cars = (
                    from c in _db.Cars
                    where c.Brand.Contains(filter) || c.Model.Contains(filter)
                    select new VMCar
                    {
                        Id = c.Id,
                        Brand = c.Brand,
                        Model = c.Model,
                        Year = c.Year,
                        PlateNumber = c.PlateNumber,
                        RentalRatePerDay = c.RentalRatePerDay,
                        LateRatePerDay = c.LateRatePerDay,
                        Status = c.Status,
                        CreatedBy = c.CreatedBy,
                        CreatedOn = c.CreatedOn,
                        ModifiedBy = c.ModifiedBy,
                        ModifiedOn = c.ModifiedOn
                    }
                ).ToList();

                if (cars.Count > 0)
                {
                    response.Data = cars;
                    response.Message =
                        $"{HttpStatusCode.OK} - {cars.Count} Car data(s) successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - No Car found";
                    response.StatusCode = HttpStatusCode.NoContent;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMCar> GetById(int id)
        {
            VMResponse<VMCar> response = new VMResponse<VMCar>();

            // Validasi awal untuk ID
            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Car ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var car = (
                    from c in _db.Cars
                    where c.Id == id
                    select new VMCar
                    {
                        Id = c.Id,
                        Brand = c.Brand,
                        Model = c.Model,
                        Year = c.Year,
                        PlateNumber = c.PlateNumber,
                        RentalRatePerDay = c.RentalRatePerDay,
                        LateRatePerDay = c.LateRatePerDay,
                        Status = c.Status,
                        CreatedBy = c.CreatedBy,
                        CreatedOn = c.CreatedOn,
                        ModifiedBy = c.ModifiedBy,
                        ModifiedOn = c.ModifiedOn
                    }
                ).FirstOrDefault();

                if (car != null)
                {
                    response.Data = car;
                    response.Message = $"{HttpStatusCode.OK} - Car data successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - Car not found";
                    response.StatusCode = HttpStatusCode.NoContent;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMCar> Create(VMCar model)
        {
            var response = new VMResponse<VMCar>();

            // Validasi input
            if (!model.IsValid(out string validationMessage))
            {
                response.Message = $"{HttpStatusCode.BadRequest} - {validationMessage}";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                // Pengecekan duplikasi
                bool isDuplicate = _db.Cars.Any(c =>
                    c.Brand.ToLower() == model.Brand.ToLower()
                    && c.Model.ToLower() == model.Model.ToLower()
                    && c.Year == model.Year
                );

                if (isDuplicate)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Duplicate Car exists.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Pembuatan record baru
                var newCar = new Car
                {
                    Brand = model.Brand,
                    Model = model.Model,
                    Year = model.Year,
                    PlateNumber = model.PlateNumber,
                    RentalRatePerDay = model.RentalRatePerDay,
                    LateRatePerDay = model.LateRatePerDay,
                    Status = model.Status,
                    CreatedBy = model.CreatedBy,
                    CreatedOn = DateTime.Now,
                };

                _db.Cars.Add(newCar);
                _db.SaveChanges();
                dbTran.Commit();

                response.Data = new VMCar
                {
                    Id = newCar.Id,
                    Brand = newCar.Brand,
                    Model = newCar.Model,
                    Year = newCar.Year,
                    PlateNumber = newCar.PlateNumber,
                    RentalRatePerDay = newCar.RentalRatePerDay,
                    LateRatePerDay = newCar.LateRatePerDay,
                    Status = newCar.Status,
                    CreatedBy = newCar.CreatedBy,
                    CreatedOn = newCar.CreatedOn,
                    ModifiedBy = newCar.ModifiedBy,
                    ModifiedOn = newCar.ModifiedOn
                };
                response.Message = $"{HttpStatusCode.Created} - Car data successfully inserted";
                response.StatusCode = HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMCar> Update(VMCar model)
        {
            var response = new VMResponse<VMCar>();

            // Validasi input
            if (!model.IsValid(out string validationMessage))
            {
                response.Message = $"{HttpStatusCode.BadRequest} - {validationMessage}";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                // Cari data yang akan diupdate
                var carToUpdate = _db.Cars.Find(model.Id);
                if (carToUpdate == null)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Car not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Pengecekan duplikasi (kecuali pada record yang sedang diupdate)
                bool isDuplicate = _db.Cars.Any(c =>
                    c.Id != model.Id
                    && // Pastikan tidak memeriksa dirinya sendiri
                    c.Brand.ToLower() == model.Brand.ToLower()
                    && c.Model.ToLower() == model.Model.ToLower()
                    && c.Year == model.Year
                );

                if (isDuplicate)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Duplicate Car exists.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Update data
                carToUpdate.Brand = model.Brand;
                carToUpdate.Model = model.Model;
                carToUpdate.Year = model.Year;
                carToUpdate.PlateNumber = model.PlateNumber;
                carToUpdate.RentalRatePerDay = model.RentalRatePerDay;
                carToUpdate.LateRatePerDay = model.LateRatePerDay;
                carToUpdate.Status = model.Status;
                carToUpdate.ModifiedBy = model.ModifiedBy;
                carToUpdate.ModifiedOn = DateTime.Now;

                _db.Cars.Update(carToUpdate);
                _db.SaveChanges();
                dbTran.Commit();

                response.Data = new VMCar
                {
                    Id = carToUpdate.Id,
                    Brand = carToUpdate.Brand,
                    Model = carToUpdate.Model,
                    Year = carToUpdate.Year,
                    PlateNumber = carToUpdate.PlateNumber,
                    RentalRatePerDay = carToUpdate.RentalRatePerDay,
                    LateRatePerDay = carToUpdate.LateRatePerDay,
                    Status = carToUpdate.Status,
                    CreatedBy = carToUpdate.CreatedBy,
                    CreatedOn = carToUpdate.CreatedOn,
                    ModifiedBy = carToUpdate.ModifiedBy,
                    ModifiedOn = carToUpdate.ModifiedOn
                };
                response.Message = $"{HttpStatusCode.OK} - Car data successfully updated";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMCar> Delete(int id)
        {
            var response = new VMResponse<VMCar>();

            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Car ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                // Cari entri berdasarkan ID
                var carToDelete = _db.Cars.Find(id);
                if (carToDelete == null)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Car not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Periksa apakah entri sedang digunakan
                bool isInUse = _db.Rentals.Any(r => r.CarId == id);
                if (isInUse)
                {
                    response.Message =
                        $"{HttpStatusCode.Conflict} - Car is currently in use and cannot be deleted.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Hapus entri
                _db.Cars.Remove(carToDelete);
                _db.SaveChanges();
                dbTran.Commit();

                response.Message = $"{HttpStatusCode.OK} - Car data successfully deleted";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }
    }
}
