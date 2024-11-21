using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Karent.DataModel;
using Karent.ViewModel;
using Microsoft.EntityFrameworkCore.Storage;

namespace Karent.DataAccess
{
    public class DACar
    {
        private readonly KarentDBContext _db;

        public DACar(KarentDBContext db)
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
                    where (c.Brand.Contains(filter) || c.Model.Contains(filter))
                    select VMCar.FromDataModel(c)
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
                    select VMCar.FromDataModel(c)
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

                response.Data = VMCar.FromDataModel(newCar);
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

                response.Data = VMCar.FromDataModel(carToUpdate);
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
