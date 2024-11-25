using Karent.DataAccess.Interfaces;
using Karent.DataModel;
using Karent.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Karent.DataAccess.NativeQuery
{
    public class DARentalNativeQuery : IDARental
    {
        private readonly KarentDBContext _db;

        public DARentalNativeQuery(KarentDBContext db)
        {
            _db = db;
        }

        public VMResponse<List<VMRental>> GetByFilter(string filter)
        {
            var response = new VMResponse<List<VMRental>>();

            try
            {
                var sql = @"
                    SELECT r.id AS Id,
                           r.user_id AS UserId,
                           r.car_id AS CarId,
                           r.start_date AS StartDate,
                           r.end_date AS EndDate,
                           r.total_fee AS TotalFee,
                           r.created_by AS CreatedBy,
                           r.created_on AS CreatedOn,
                           r.modified_by AS ModifiedBy,
                           r.modified_on AS ModifiedOn,
                           u.name AS UserName,
                           c.brand AS CarBrand, 
                           c.model AS CarModel
                    FROM rentals r
                    JOIN users u ON r.user_id = u.id
                    JOIN cars c ON r.car_id = c.id
                    WHERE c.brand LIKE @p0 OR c.model LIKE @p0 OR u.name LIKE @p0";

                var rentals = _db.VMRentals
                    .FromSqlRaw(sql, $"%{filter}%")
                    .ToList();

                if (rentals.Any())
                {
                    response.Data = rentals;
                    response.Message = $"{HttpStatusCode.OK} - {rentals.Count} Rental data(s) successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - No Rental found";
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

        public VMResponse<VMRental> GetById(int id)
        {
            var response = new VMResponse<VMRental>();

            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Rental ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var sql = @"
                    SELECT r.id AS Id,
                           r.user_id AS UserId,
                           r.car_id AS CarId,
                           r.start_date AS StartDate,
                           r.end_date AS EndDate,
                           r.total_fee AS TotalFee,
                           r.created_by AS CreatedBy,
                           r.created_on AS CreatedOn,
                           r.modified_by AS ModifiedBy,
                           r.modified_on AS ModifiedOn,
                           u.name AS UserName,
                           c.brand AS CarBrand, 
                           c.model AS CarModel
                        FROM rentals r
                        JOIN users u ON r.user_id = u.id
                        JOIN cars c ON r.car_id = c.id
                        WHERE r.id = @p0";

                var rental = _db.VMRentals
                    .FromSqlRaw(sql, id)
                    .FirstOrDefault();

                if (rental != null)
                {
                    response.Data = rental;
                    response.Message = $"{HttpStatusCode.OK} - Rental data successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - Rental not found";
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

        public VMResponse<VMRental> Create(VMRental model)
        {
            var response = new VMResponse<VMRental>();

            if (!model.IsValid(out string validationMessage))
            {
                response.Message = $"{HttpStatusCode.BadRequest} - {validationMessage}";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                var insertSql = @"
                    INSERT INTO rentals (user_id, car_id, start_date, end_date, total_fee, created_by, created_on)
                    OUTPUT INSERTED.*
                    VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);";

                // Execute the SQL and retrieve the new Id
                var insertedRental = _db.Rentals
                    .FromSqlRaw(insertSql,
                        model.UserId,
                        model.CarId,
                        model.StartDate,
                        model.EndDate,
                        model.TotalFee,
                        model.CreatedBy ?? (object)DBNull.Value,
                        DateTime.Now)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (insertedRental == null)
                {
                    response.Message = $"{HttpStatusCode.InternalServerError} - Failed to insert Rental";
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

                response.Data = GetById(insertedRental.Id).Data;
                response.Message = $"{HttpStatusCode.Created} - Rental data successfully inserted";
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

        public VMResponse<VMRental> Update(VMRental model)
        {
            var response = new VMResponse<VMRental>();

            if (!model.IsValid(out string validationMessage))
            {
                response.Message = $"{HttpStatusCode.BadRequest} - {validationMessage}";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                var sqlGetRental = "SELECT COUNT(1) AS id FROM rentals WHERE id = @p0";

                var existsCount = _db.Rentals
                    .FromSqlRaw(sqlGetRental, model.Id)
                    .Select(r => r.Id)
                    .First();

                if (existsCount == 0)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Rental not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                var updateSql = @"
                        UPDATE rentals SET
                            user_id = @p1,
                            car_id = @p2,
                            start_date = @p3,
                            end_date = @p4,
                            total_fee = @p5,
                            modified_by = @p6,
                            modified_on = @p7
                        WHERE id = @p0;";

                _db.Database.ExecuteSqlRaw(updateSql,
                    model.Id,
                    model.UserId,
                    model.CarId,
                    model.StartDate,
                    model.EndDate,
                    model.TotalFee,
                    model.ModifiedBy ?? (object)DBNull.Value,
                    DateTime.Now);

                response.Data = GetById(model.Id).Data;
                response.Message = $"{HttpStatusCode.OK} - Rental data successfully updated";
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

        public VMResponse<VMRental> Delete(int id)
        {
            var response = new VMResponse<VMRental>();

            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Rental ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                var sqlGetRental = "SELECT COUNT(1) AS id FROM rentals WHERE id = @p0";

                var existsCount = _db.Rentals
                    .FromSqlRaw(sqlGetRental, id)
                    .Select(r => r.Id)
                    .First();

                if (existsCount == 0)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Rental not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                var sqlCheckInUse = "SELECT COUNT(1) AS id FROM rental_returns WHERE rental_id = @p0";

                var inUseCount = _db.RentalReturns
                    .FromSqlRaw(sqlCheckInUse, id)
                    .Select(r => r.Id)
                    .First();

                if (inUseCount > 0)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Rental is currently in use and cannot be deleted.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                var deleteSql = "DELETE FROM rentals WHERE id = @p0";

                _db.Database.ExecuteSqlRaw(deleteSql, id);

                response.Message = $"{HttpStatusCode.OK} - Rental data successfully deleted";
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
