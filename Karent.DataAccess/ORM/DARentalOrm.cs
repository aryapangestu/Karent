﻿using Karent.DataAccess.Interfaces;
using Karent.DataModel;
using Karent.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Karent.DataAccess.ORM
{
    public class DARentalOrm : IDARental
    {
        private readonly KarentDBContext _db;

        public DARentalOrm(KarentDBContext db)
        {
            _db = db;
        }

        public VMResponse<List<VMRental>> GetByFilter(string filter)
        {
            VMResponse<List<VMRental>> response = new VMResponse<List<VMRental>>();

            try
            {
                var rentals = (
                    from r in _db.Rentals
                    join u in _db.Users on r.UserId equals u.Id
                    join c in _db.Cars on r.CarId equals c.Id
                    where c.Brand.Contains(filter) || c.Model.Contains(filter) || u.Name.Contains(filter)
                    select VMRental.FromDataModel(r, u, c)
                ).ToList();

                if (rentals.Count > 0)
                {
                    response.Data = rentals;
                    response.Message =
                        $"{HttpStatusCode.OK} - {rentals.Count} Rental data(s) successfully fetched";
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
            }

            return response;
        }

        public VMResponse<VMRental> GetById(int id)
        {
            VMResponse<VMRental> response = new VMResponse<VMRental>();

            // Validasi awal untuk ID
            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid Rental ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var rental = (
                    from r in _db.Rentals
                    join u in _db.Users on r.UserId equals u.Id
                    join c in _db.Cars on r.CarId equals c.Id
                    where r.Id == id
                    select VMRental.FromDataModel(r, u, c)
                ).FirstOrDefault();

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
            }

            return response;
        }

        public VMResponse<VMRental> Create(VMRental model)
        {
            var response = new VMResponse<VMRental>();

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
                var newRental = new Rental
                {
                    Id = model.Id,
                    UserId = model.UserId,
                    CarId = model.CarId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    TotalFee = model.TotalFee,
                    CreatedBy = model.CreatedBy,
                    CreatedOn = DateTime.Now,
                };

                _db.Rentals.Add(newRental);
                _db.SaveChanges();
                dbTran.Commit();

                response.Data = GetById(newRental.Id).Data;
                response.Message = $"{HttpStatusCode.Created} - Rental data successfully inserted";
                response.StatusCode = HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMRental> Update(VMRental model)
        {
            var response = new VMResponse<VMRental>();

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
                var rentalToUpdate = _db.Rentals.Find(model.Id);
                if (rentalToUpdate == null)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Rental not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Update data
                rentalToUpdate.Id = model.Id;
                rentalToUpdate.UserId = model.UserId;
                rentalToUpdate.CarId = model.CarId;
                rentalToUpdate.StartDate = model.StartDate;
                rentalToUpdate.EndDate = model.EndDate;
                rentalToUpdate.TotalFee = model.TotalFee;
                rentalToUpdate.ModifiedBy = model.ModifiedBy;
                rentalToUpdate.ModifiedOn = DateTime.Now;

                _db.Rentals.Update(rentalToUpdate);
                _db.SaveChanges();
                dbTran.Commit();

                response.Data = GetById(rentalToUpdate.Id).Data;
                response.Message = $"{HttpStatusCode.OK} - Rental data successfully updated";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
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
                // Cari entri berdasarkan ID
                var rentalToDelete = _db.Rentals.Find(id);
                if (rentalToDelete == null)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - Rental not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Periksa apakah entri sedang digunakan
                bool isInUse = _db.RentalReturns.Any(r => r.RentalId == id);
                if (isInUse)
                {
                    response.Message =
                        $"{HttpStatusCode.Conflict} - Rental is currently in use and cannot be deleted.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Hapus entri
                _db.Rentals.Remove(rentalToDelete);
                _db.SaveChanges();
                dbTran.Commit();

                response.Message = $"{HttpStatusCode.OK} - Rental data successfully deleted";
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