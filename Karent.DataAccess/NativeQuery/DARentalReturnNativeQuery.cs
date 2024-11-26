using Karent.DataAccess.Interfaces;
using Karent.DataModel;
using Karent.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Karent.DataAccess.NativeQuery
{
    public class DARentalReturnNativeQuery : IDARentalReturn
    {
        private readonly KarentDBContext _db;

        public DARentalReturnNativeQuery(KarentDBContext db)
        {
            _db = db;
        }

        public VMResponse<List<VMRentalReturn>> GetByFilter(string filter)
        {
            var response = new VMResponse<List<VMRentalReturn>>();

            try
            {
                var sql = @"
                        SELECT 
                            rr.id AS Id,
                            rr.rental_id AS RentalId,
                            u.name AS UserName,
                            c.brand AS CarBrand,
                            c.model AS CarModel,
                            rr.return_date AS ReturnDate,
                            rr.late_fee AS LateFee,
                            rr.total_fee AS TotalFee,
                            r.start_date AS RentalStartDate,
                            r.end_date AS RentalEndDate,
                            r.total_fee AS RentalTotalFee,
                            rr.created_by AS CreatedBy,
                            rr.created_on AS CreatedOn,
                            rr.modified_by AS ModifiedBy,
                            rr.modified_on AS ModifiedOn
                        FROM rental_returns rr
                        JOIN rentals r ON rr.rental_id = r.id
                        JOIN users u ON r.user_id = u.id
                        JOIN cars c ON r.car_id = c.id
                        WHERE c.brand LIKE @p0 OR c.model LIKE @p0 OR u.name LIKE @p0";

                var rentalReturns = _db.VMRentalReturns
                    .FromSqlRaw(sql, $"%{filter}%")
                    .ToList();

                if (rentalReturns.Any())
                {
                    response.Data = rentalReturns;
                    response.Message = $"{HttpStatusCode.OK} - {rentalReturns.Count} Rental Return data(s) successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - No Rental Return found";
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

        public VMResponse<List<VMRentalReturn>> GetByFilter(string filter, int userId)
        {
            var response = new VMResponse<List<VMRentalReturn>>();

            try
            {
                var sql = @"
                SELECT 
                    rr.id AS Id,
                    rr.rental_id AS RentalId,
                    u.name AS UserName,
                    c.brand AS CarBrand,
                    c.model AS CarModel,
                    rr.return_date AS ReturnDate,
                    rr.late_fee AS LateFee,
                    rr.total_fee AS TotalFee,
                    r.start_date AS RentalStartDate,
                    r.end_date AS RentalEndDate,
                    r.total_fee AS RentalTotalFee,
                    rr.created_by AS CreatedBy,
                    rr.created_on AS CreatedOn,
                    rr.modified_by AS ModifiedBy,
                    rr.modified_on AS ModifiedOn
                FROM rental_returns rr
                JOIN rentals r ON rr.rental_id = r.id
                JOIN users u ON r.user_id = u.id
                JOIN cars c ON r.car_id = c.id
                WHERE (c.brand LIKE @p0 OR c.model LIKE @p0 OR u.name LIKE @p0)
                  AND r.user_id = @p1";

                var rentalReturns = _db.VMRentalReturns
                    .FromSqlRaw(sql, $"%{filter}%", userId)
                    .ToList();

                if (rentalReturns.Any())
                {
                    response.Data = rentalReturns;
                    response.Message = $"{HttpStatusCode.OK} - {rentalReturns.Count} Rental Return data(s) successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - No Rental Return found";
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

        public VMResponse<VMRentalReturn> GetById(int id)
        {
            var response = new VMResponse<VMRentalReturn>();

            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Rental Return ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var sql = @"
                        SELECT 
                            rr.id AS Id,
                            rr.rental_id AS RentalId,
                            u.name AS UserName,
                            c.brand AS CarBrand,
                            c.model AS CarModel,
                            rr.return_date AS ReturnDate,
                            rr.late_fee AS LateFee,
                            rr.total_fee AS TotalFee,
                            r.start_date AS RentalStartDate,
                            r.end_date AS RentalEndDate,
                            r.total_fee AS RentalTotalFee,
                            rr.created_by AS CreatedBy,
                            rr.created_on AS CreatedOn,
                            rr.modified_by AS ModifiedBy,
                            rr.modified_on AS ModifiedOn
                        FROM rental_returns rr
                        JOIN rentals r ON rr.rental_id = r.id
                        JOIN users u ON r.user_id = u.id
                        JOIN cars c ON r.car_id = c.id
                        WHERE rr.id = @p0";

                var rentalReturn = _db.VMRentalReturns
                    .FromSqlRaw(sql, id)
                    .FirstOrDefault();

                if (rentalReturn != null)
                {
                    response.Data = rentalReturn;
                    response.Message = $"{HttpStatusCode.OK} - Rental Return data successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - Rental Return not found";
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

        public VMResponse<VMRentalReturn> GetById(int id, int userId)
        {
            var response = new VMResponse<VMRentalReturn>();

            if (id <= 0 || userId <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Rental Return ID or User ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var sql = @"
                SELECT 
                    rr.id AS Id,
                    rr.rental_id AS RentalId,
                    u.name AS UserName,
                    c.brand AS CarBrand,
                    c.model AS CarModel,
                    rr.return_date AS ReturnDate,
                    rr.late_fee AS LateFee,
                    rr.total_fee AS TotalFee,
                    r.start_date AS RentalStartDate,
                    r.end_date AS RentalEndDate,
                    r.total_fee AS RentalTotalFee,
                    rr.created_by AS CreatedBy,
                    rr.created_on AS CreatedOn,
                    rr.modified_by AS ModifiedBy,
                    rr.modified_on AS ModifiedOn
                FROM rental_returns rr
                JOIN rentals r ON rr.rental_id = r.id
                JOIN users u ON r.user_id = u.id
                JOIN cars c ON r.car_id = c.id
                WHERE rr.id = @p0 AND r.user_id = @p1";

                var rentalReturn = _db.VMRentalReturns
                    .FromSqlRaw(sql, id, userId)
                    .FirstOrDefault();

                if (rentalReturn != null)
                {
                    response.Data = rentalReturn;
                    response.Message = $"{HttpStatusCode.OK} - Rental Return data successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - Rental Return not found";
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

        public VMResponse<VMRentalReturn> Create(VMRentalReturn model)
        {
            var response = new VMResponse<VMRentalReturn>();

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
                        INSERT INTO rental_returns (rental_id, return_date, late_fee, total_fee, created_by, created_on)
                        OUTPUT INSERTED.*
                        VALUES (@p0, @p1, @p2, @p3, @p4, @p5);";

                var insertedRentalReturn = _db.RentalReturns
                    .FromSqlRaw(insertSql,
                        model.RentalId,
                        model.ReturnDate,
                        model.LateFee,
                        model.TotalFee,
                        model.CreatedBy ?? (object)DBNull.Value,
                        DateTime.Now)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (insertedRentalReturn == null)
                {
                    response.Message = $"{HttpStatusCode.InternalServerError} - Failed to insert Rental Return";
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

                response.Data = GetById(insertedRentalReturn.Id).Data;
                response.Message = $"{HttpStatusCode.Created} - Rental Return data successfully inserted";
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

        public VMResponse<VMRentalReturn> Update(VMRentalReturn model)
        {
            var response = new VMResponse<VMRentalReturn>();

            if (!model.IsValid(out string validationMessage))
            {
                response.Message = $"{HttpStatusCode.BadRequest} - {validationMessage}";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                var sqlGetRentalReturn = "SELECT COUNT(1) AS id FROM rental_returns WHERE id = @p0";

                var existsCount = _db.RentalReturns
                    .FromSqlRaw(sqlGetRentalReturn, model.Id)
                    .Select(rr => rr.Id)
                    .First();

                if (existsCount == 0)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Rental Return not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                var updateSql = @"
                        UPDATE rental_returns SET
                            rental_id = @p1,
                            return_date = @p2,
                            late_fee = @p3,
                            total_fee = @p4,
                            modified_by = @p5,
                            modified_on = @p6
                        WHERE id = @p0;";

                _db.Database.ExecuteSqlRaw(updateSql,
                    model.Id,
                    model.RentalId,
                    model.ReturnDate,
                    model.LateFee,
                    model.TotalFee,
                    model.ModifiedBy ?? (object)DBNull.Value,
                    DateTime.Now);

                response.Data = GetById(model.Id).Data; ;
                response.Message = $"{HttpStatusCode.OK} - Rental Return data successfully updated";
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

        public VMResponse<VMRentalReturn> Delete(int id)
        {
            var response = new VMResponse<VMRentalReturn>();

            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Rental Return ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                var sqlGetRentalReturn = "SELECT COUNT(1) AS id FROM rental_returns WHERE id = @p0";

                var existsCount = _db.RentalReturns
                    .FromSqlRaw(sqlGetRentalReturn, id)
                    .Select(rr => rr.Id)
                    .First();

                if (existsCount == 0)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Rental Return not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                var deleteSql = "DELETE FROM rental_returns WHERE id = @p0";

                _db.Database.ExecuteSqlRaw(deleteSql, id);

                response.Message = $"{HttpStatusCode.OK} - Rental Return data successfully deleted";
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
