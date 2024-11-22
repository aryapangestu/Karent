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
                        SELECT rr.*, r.*, u.*, c.*
                        FROM rental_returns rr
                        JOIN rentals r ON rr.rental_id = r.id
                        JOIN users u ON r.user_id = u.id
                        JOIN cars c ON r.car_id = c.id
                        WHERE c.brand LIKE @p0 OR c.model LIKE @p0 OR u.name LIKE @p0";

                var rentalReturns = _db.RentalReturns
                    .FromSqlRaw(sql, $"%{filter}%")
                    .Select(rr => VMRentalReturn.FromDataModel(rr, rr.Rental, rr.Rental.User, rr.Rental.Car))
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
                        SELECT rr.*, r.*, u.*, c.*
                        FROM rental_returns rr
                        JOIN rentals r ON rr.rental_id = r.id
                        JOIN users u ON r.user_id = u.id
                        JOIN cars c ON r.car_id = c.id
                        WHERE rr.id = @p0";

                var rentalReturn = _db.RentalReturns
                    .FromSqlRaw(sql, id)
                    .Select(rr => VMRentalReturn.FromDataModel(rr, rr.Rental, rr.Rental.User, rr.Rental.Car))
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
                        VALUES (@p0, @p1, @p2, @p3, @p4, @p5);
                        SELECT CAST(SCOPE_IDENTITY() as int) AS Id;";

                var newId = _db.Database.ExecuteSqlRaw(insertSql,
                    model.RentalId,
                    model.ReturnDate,
                    model.LateFee,
                    model.TotalFee,
                    model.CreatedBy ?? (object)DBNull.Value,
                    DateTime.Now);

                if (newId == 0)
                {
                    response.Message = $"{HttpStatusCode.InternalServerError} - Failed to insert Rental Return";
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

                model.Id = newId;
                response.Data = model;
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
                var sqlGetRentalReturn = "SELECT COUNT(1) as Id FROM rental_returns WHERE id = @p0";

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

                response.Data = model;
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
                var sqlGetRentalReturn = "SELECT COUNT(1) as Id FROM rental_returns WHERE id = @p0";

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
