using Karent.DataAccess.Interfaces;
using Karent.DataModel;
using Karent.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Karent.DataAccess.NativeQuery
{
    public class DACarNativeQuery : IDACar
    {
        private readonly KarentDBContext _db;

        public DACarNativeQuery(KarentDBContext db)
        {
            _db = db;
        }

        public VMResponse<List<VMCar>> GetByFilter(string filter)
        {
            var response = new VMResponse<List<VMCar>>();

            try
            {
                var sql = @"
                        SELECT * FROM cars
                        WHERE brand LIKE @p0 OR model LIKE @p0"; // @p Parameterized Queries

                var cars = _db.Cars
                    .FromSqlRaw(sql, $"%{filter}%")
                    .Select(c => VMCar.FromDataModel(c))
                    .ToList();

                if (cars.Any())
                {
                    response.Data = cars;
                    response.Message = $"{HttpStatusCode.OK} - {cars.Count} Car data(s) successfully fetched";
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
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public VMResponse<VMCar> GetById(int id)
        {
            var response = new VMResponse<VMCar>();

            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Car ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var sql = "SELECT * FROM cars WHERE id = @p0";

                var car = _db.Cars
                    .FromSqlRaw(sql, id)
                    .Select(c => VMCar.FromDataModel(c))
                    .FirstOrDefault();

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
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public VMResponse<VMCar> Create(VMCar model)
        {
            var response = new VMResponse<VMCar>();

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
                var sqlDuplicateCheck = @"
                        SELECT COUNT(1) as Id FROM cars
                        WHERE LOWER(brand) = @p0 AND LOWER(model) = @p1 AND year = @p2";

                var duplicateCount = _db.Cars
                    .FromSqlRaw(sqlDuplicateCheck, model.Brand.ToLower(), model.Model.ToLower(), model.Year)
                    .Select(c => c.Id)
                    .First();

                if (duplicateCount > 0)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Duplicate Car exists.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Pembuatan record baru menggunakan native query
                var insertSql = @"
                        INSERT INTO cars (brand, model, year, plate_number, rental_rate_per_day, late_rate_per_day, status, created_by, created_on)
                        VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8);
                        SELECT CAST(SCOPE_IDENTITY() as int) AS Id;
                    ";

                var newId = _db.Database
                    .ExecuteSqlRaw(insertSql,
                        model.Brand,
                        model.Model,
                        model.Year,
                        model.PlateNumber,
                        model.RentalRatePerDay,
                        model.LateRatePerDay,
                        model.Status,
                        model.CreatedBy ?? (object)DBNull.Value,
                        DateTime.Now);

                if (newId == 0)
                {
                    response.Message = $"{HttpStatusCode.InternalServerError} - Failed to insert Car";
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

                model.Id = newId;
                response.Data = model;
                response.Message = $"{HttpStatusCode.Created} - Car data successfully inserted";
                response.StatusCode = HttpStatusCode.Created;

                dbTran.Commit();
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public VMResponse<VMCar> Update(VMCar model)
        {
            var response = new VMResponse<VMCar>();

            if (!model.IsValid(out string validationMessage))
            {
                response.Message = $"{HttpStatusCode.BadRequest} - {validationMessage}";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                // Cek keberadaan data
                var sqlGetCar = "SELECT COUNT(1) as Id FROM cars WHERE id = @p0";

                var existsCount = _db.Cars
                    .FromSqlRaw(sqlGetCar, model.Id)
                    .Select(c => c.Id)
                    .First();

                if (existsCount == 0)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Car not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Pengecekan duplikasi
                var sqlDuplicateCheck = @"
                        SELECT COUNT(1) as Id FROM cars
                        WHERE LOWER(brand) = @p0 AND LOWER(model) = @p1 AND year = @p2";

                var duplicateCount = _db.Cars
                    .FromSqlRaw(sqlDuplicateCheck, model.Brand.ToLower(), model.Model.ToLower(), model.Year)
                    .Select(c => c.Id)
                    .First();

                if (duplicateCount > 0)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Duplicate Car exists.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Update data menggunakan native query
                var updateSql = @"
                        UPDATE cars SET
                            brand = @p1,
                            model = @p2,
                            year = @p3,
                            plate_number = @p4,
                            rental_rate_per_day = @p5,
                            late_rate_per_day = @p6,
                            status = @p7,
                            modified_by = @p8,
                            modified_on = @p9
                        WHERE id = @p0;
                    ";

                _db.Database.ExecuteSqlRaw(updateSql,
                    model.Id,
                    model.Brand,
                    model.Model,
                    model.Year,
                    model.PlateNumber,
                    model.RentalRatePerDay,
                    model.LateRatePerDay,
                    model.Status,
                    model.ModifiedBy ?? (object)DBNull.Value,
                    DateTime.Now);

                response.Data = model;
                response.Message = $"{HttpStatusCode.OK} - Car data successfully updated";
                response.StatusCode = HttpStatusCode.OK;

                dbTran.Commit();
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
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
                // Cek keberadaan data
                var sqlGetCar = "SELECT COUNT(1) as Id FROM cars WHERE id = @p0";

                var existsCount = _db.Cars
                    .FromSqlRaw(sqlGetCar, id)
                    .Select(c => c.Id)
                    .First();

                if (existsCount == 0)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Car not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Periksa apakah entri sedang digunakan
                var sqlCheckInUse = "SELECT COUNT(1) as Id FROM rentals WHERE car_id = @p0";

                var inUseCount = _db.Rentals
                    .FromSqlRaw(sqlCheckInUse, id)
                    .Select(c => c.Id)
                    .First();

                if (inUseCount > 0)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Car is currently in use and cannot be deleted.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Hapus entri menggunakan native query
                var deleteSql = "DELETE FROM cars WHERE id = @p0";

                _db.Database.ExecuteSqlRaw(deleteSql, id);

                response.Message = $"{HttpStatusCode.OK} - Car data successfully deleted";
                response.StatusCode = HttpStatusCode.OK;

                dbTran.Commit();
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
    }
}
