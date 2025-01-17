﻿using Karent.DataAccess.Interfaces;
using Karent.DataModel;
using Karent.ViewModel;
using System.Net;

namespace Karent.DataAccess.ORM
{
    public class DARentalReturnOrm : IDARentalReturn
    {
        private readonly KarentDBContext _db;

        // Konstruktor untuk menginisialisasi konteks database
        public DARentalReturnOrm(KarentDBContext db)
        {
            _db = db;
        }

        public VMResponse<List<VMRentalReturn>> GetByFilter(string filter)
        {
            VMResponse<List<VMRentalReturn>> response = new VMResponse<List<VMRentalReturn>>();

            try
            {
                var rentalReturns = (
                    from rr in _db.RentalReturns
                    join r in _db.Rentals on rr.RentalId equals r.Id
                    join u in _db.Users on r.UserId equals u.Id
                    join c in _db.Cars on r.CarId equals c.Id
                    where
                        c.Brand.Contains(filter)
                        || c.Model.Contains(filter)
                        || u.Name.Contains(filter)
                    select new VMRentalReturn
                    {
                        Id = rr.Id,
                        RentalId = rr.RentalId,
                        UserName = u.Name,
                        CarBrand = c.Brand,
                        CarModel = c.Model,
                        ReturnDate = rr.ReturnDate,
                        LateFee = rr.LateFee,
                        TotalFee = rr.TotalFee,
                        RentalStartDate = r.StartDate,
                        RentalEndDate = r.EndDate,
                        RentalTotalFee = r.TotalFee,
                        CreatedBy = rr.CreatedBy,
                        CreatedOn = rr.CreatedOn,
                        ModifiedBy = rr.ModifiedBy,
                        ModifiedOn = rr.ModifiedOn
                    }
                ).ToList();

                if (rentalReturns.Count > 0)
                {
                    response.Data = rentalReturns;
                    response.Message =
                        $"{HttpStatusCode.OK} - {rentalReturns.Count} Rental Return data(s) successfully fetched";
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
            }

            return response;
        }

        public VMResponse<List<VMRentalReturn>> GetByFilter(string filter, int userId)
        {
            var response = new VMResponse<List<VMRentalReturn>>();

            try
            {
                var rentalReturns = (
                    from rr in _db.RentalReturns
                    join r in _db.Rentals on rr.RentalId equals r.Id
                    join u in _db.Users on r.UserId equals u.Id
                    join c in _db.Cars on r.CarId equals c.Id
                    where (c.Brand.Contains(filter) || c.Model.Contains(filter) || u.Name.Contains(filter))
                          && r.UserId == userId
                    select new VMRentalReturn
                    {
                        Id = rr.Id,
                        RentalId = rr.RentalId,
                        UserName = u.Name,
                        CarBrand = c.Brand,
                        CarModel = c.Model,
                        ReturnDate = rr.ReturnDate,
                        LateFee = rr.LateFee,
                        TotalFee = rr.TotalFee,
                        RentalStartDate = r.StartDate,
                        RentalEndDate = r.EndDate,
                        RentalTotalFee = r.TotalFee,
                        CreatedBy = rr.CreatedBy,
                        CreatedOn = rr.CreatedOn,
                        ModifiedBy = rr.ModifiedBy,
                        ModifiedOn = rr.ModifiedOn
                    }
                ).ToList();

                if (rentalReturns.Count > 0)
                {
                    response.Data = rentalReturns;
                    response.Message = $"{HttpStatusCode.OK} - {rentalReturns.Count} Rental Return(s) successfully fetched";
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
            VMResponse<VMRentalReturn> response = new VMResponse<VMRentalReturn>();

            // Validasi awal untuk ID
            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Rental Return ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var rentalReturn = (
                    from rr in _db.RentalReturns
                    join r in _db.Rentals on rr.RentalId equals r.Id
                    join u in _db.Users on r.UserId equals u.Id
                    join c in _db.Cars on r.CarId equals c.Id
                    where rr.Id == id
                    select new VMRentalReturn
                    {
                        Id = rr.Id,
                        RentalId = rr.RentalId,
                        UserName = u.Name,
                        CarBrand = c.Brand,
                        CarModel = c.Model,
                        ReturnDate = rr.ReturnDate,
                        LateFee = rr.LateFee,
                        TotalFee = rr.TotalFee,
                        RentalStartDate = r.StartDate,
                        RentalEndDate = r.EndDate,
                        RentalTotalFee = r.TotalFee,
                        CreatedBy = rr.CreatedBy,
                        CreatedOn = rr.CreatedOn,
                        ModifiedBy = rr.ModifiedBy,
                        ModifiedOn = rr.ModifiedOn
                    }
                ).FirstOrDefault();

                if (rentalReturn != null)
                {
                    response.Data = rentalReturn;
                    response.Message =
                        $"{HttpStatusCode.OK} - Rental Return data successfully fetched";
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
            }

            return response;
        }

        public VMResponse<VMRentalReturn> GetById(int id, int userId)
        {
            var response = new VMResponse<VMRentalReturn>();

            // Validasi ID
            if (id <= 0 || userId <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Rental Return ID or User ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var rentalReturn = (
                    from rr in _db.RentalReturns
                    join r in _db.Rentals on rr.RentalId equals r.Id
                    join u in _db.Users on r.UserId equals u.Id
                    join c in _db.Cars on r.CarId equals c.Id
                    where rr.Id == id && r.UserId == userId
                    select new VMRentalReturn
                    {
                        Id = rr.Id,
                        RentalId = rr.RentalId,
                        UserName = u.Name,
                        CarBrand = c.Brand,
                        CarModel = c.Model,
                        ReturnDate = rr.ReturnDate,
                        LateFee = rr.LateFee,
                        TotalFee = rr.TotalFee,
                        RentalStartDate = r.StartDate,
                        RentalEndDate = r.EndDate,
                        RentalTotalFee = r.TotalFee,
                        CreatedBy = rr.CreatedBy,
                        CreatedOn = rr.CreatedOn,
                        ModifiedBy = rr.ModifiedBy,
                        ModifiedOn = rr.ModifiedOn
                    }
                ).FirstOrDefault();

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
                // Pembuatan record baru
                var newRentalReturn = new RentalReturn
                {
                    Id = model.Id,
                    RentalId = model.RentalId,
                    ReturnDate = model.ReturnDate,
                    LateFee = model.LateFee,
                    TotalFee = model.TotalFee,
                    CreatedBy = model.CreatedBy,
                    CreatedOn = DateTime.Now,
                };

                _db.RentalReturns.Add(newRentalReturn);
                _db.SaveChanges();
                dbTran.Commit();

                response.Data = GetById(newRentalReturn.Id).Data;
                response.Message =
                    $"{HttpStatusCode.Created} - Rental Return data successfully inserted";
                response.StatusCode = HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMRentalReturn> Update(VMRentalReturn model)
        {
            var response = new VMResponse<VMRentalReturn>();

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
                var rentalReturnToUpdate = _db.RentalReturns.Find(model.Id);
                if (rentalReturnToUpdate == null)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Rental Return not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Update data
                rentalReturnToUpdate.Id = model.Id;
                rentalReturnToUpdate.RentalId = model.RentalId;
                rentalReturnToUpdate.ReturnDate = model.ReturnDate;
                rentalReturnToUpdate.LateFee = model.LateFee;
                rentalReturnToUpdate.TotalFee = model.TotalFee;
                rentalReturnToUpdate.ModifiedBy = model.ModifiedBy;
                rentalReturnToUpdate.ModifiedOn = DateTime.Now;

                _db.RentalReturns.Update(rentalReturnToUpdate);
                _db.SaveChanges();
                dbTran.Commit();

                response.Data = GetById(rentalReturnToUpdate.Id).Data;
                response.Message = $"{HttpStatusCode.OK} - Rental Return data successfully updated";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
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
                // Cari entri berdasarkan ID
                var rentalReturnToDelete = _db.RentalReturns.Find(id);
                if (rentalReturnToDelete == null)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Rental Return not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Hapus entri
                _db.RentalReturns.Remove(rentalReturnToDelete);
                _db.SaveChanges();
                dbTran.Commit();

                response.Message = $"{HttpStatusCode.OK} - Rental Return data successfully deleted";
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
