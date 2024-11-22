using Karent.DataAccess.Interfaces;
using Karent.DataModel;
using Karent.ViewModel;
using Karent.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Karent.DataAccess.ORM
{
    public class DAUserOrm : IDAUser
    {
        private readonly KarentDBContext _db;

        public DAUserOrm(KarentDBContext db)
        {
            _db = db;
        }

        public VMResponse<List<VMUser>> GetByFilter(string filter)
        {
            VMResponse<List<VMUser>> response = new VMResponse<List<VMUser>>();

            try
            {
                var users = (
                    from u in _db.Users
                    where u.Name.Contains(filter) || u.Email.Contains(filter)
                    select VMUser.FromDataModel(u)
                ).ToList();

                if (users.Count > 0)
                {
                    response.Data = users;
                    response.Message =
                        $"{HttpStatusCode.OK} - {users.Count} User data(s) successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - No User found";
                    response.StatusCode = HttpStatusCode.NoContent;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMUser> GetById(int id)
        {
            VMResponse<VMUser> response = new VMResponse<VMUser>();

            // Validasi awal untuk ID
            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid User ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var user = (
                    from u in _db.Users
                    where u.Id == id
                    select VMUser.FromDataModel(u)
                ).FirstOrDefault();

                if (user != null)
                {
                    response.Data = user;
                    response.Message = $"{HttpStatusCode.OK} - User data successfully fetched";
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.Message = $"{HttpStatusCode.NoContent} - User not found";
                    response.StatusCode = HttpStatusCode.NoContent;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMUser> Create(VMUser model)
        {
            var response = new VMResponse<VMUser>();

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
                bool isDuplicate = _db.Users.Any(u =>
                    u.Email.ToLower() == model.Email.ToLower()
                    && u.PhoneNumber == model.PhoneNumber
                    && u.DrivingLicenseNumber == model.DrivingLicenseNumber
                );

                if (isDuplicate)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Duplicate User exists.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Pembuatan record baru
                var newUser = new User
                {
                    Id = model.Id,
                    Name = model.Name,
                    Email = model.Email,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    DrivingLicenseNumber = model.DrivingLicenseNumber,
                    Password = PasswordHasher.HashPassword(model.Password),
                    UserType = model.UserType,
                    CreatedBy = model.CreatedBy,
                    CreatedOn = DateTime.Now,
                };

                _db.Users.Add(newUser);
                _db.SaveChanges();
                dbTran.Commit();

                response.Data = VMUser.FromDataModel(newUser);
                response.Message = $"{HttpStatusCode.Created} - User data successfully inserted";
                response.StatusCode = HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMUser> Update(VMUser model)
        {
            var response = new VMResponse<VMUser>();

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
                var userToUpdate = _db.Users.Find(model.Id);
                if (userToUpdate == null)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - User not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Pengecekan duplikasi (kecuali pada record yang sedang diupdate)
                bool isDuplicate = _db.Users.Any(u =>
                    u.Id != model.Id
                    && // Pastikan tidak memeriksa dirinya sendiri
                    u.Email.ToLower() == model.Email.ToLower()
                    && u.PhoneNumber == model.PhoneNumber
                    && u.DrivingLicenseNumber == model.DrivingLicenseNumber
                );

                if (isDuplicate)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Duplicate User exists.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Update data
                userToUpdate.Name = model.Name;
                userToUpdate.Email = model.Email;
                userToUpdate.Address = model.Address;
                userToUpdate.PhoneNumber = model.PhoneNumber;
                userToUpdate.DrivingLicenseNumber = model.DrivingLicenseNumber;
                userToUpdate.UserType = model.UserType;
                userToUpdate.ModifiedBy = model.ModifiedBy;
                userToUpdate.ModifiedOn = DateTime.Now;

                _db.Users.Update(userToUpdate);
                _db.SaveChanges();
                dbTran.Commit();

                response.Data = VMUser.FromDataModel(userToUpdate);
                response.Message = $"{HttpStatusCode.OK} - User data successfully updated";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMUser> Delete(int id)
        {
            var response = new VMResponse<VMUser>();

            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid User ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                // Cari entri berdasarkan ID
                var userToDelete = _db.Users.Find(id);
                if (userToDelete == null)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - User not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // Periksa apakah entri sedang digunakan
                bool isInUse = _db.Rentals.Any(r => r.UserId == id);
                if (isInUse)
                {
                    response.Message =
                        $"{HttpStatusCode.Conflict} - User is currently in use and cannot be deleted.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                // Hapus entri
                _db.Users.Remove(userToDelete);
                _db.SaveChanges();
                dbTran.Commit();

                response.Message = $"{HttpStatusCode.OK} - User data successfully deleted";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                response.Message = $"{HttpStatusCode.InternalServerError} - {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMUser> Login(string email, string password)
        {
            VMResponse<VMUser> response = new VMResponse<VMUser>();

            try
            {
                // Cari user berdasarkan email
                var user = _db.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    response.Message = "Invalid email or password.";
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    return response;
                }

                // Verifikasi password
                bool isPasswordValid = PasswordHasher.VerifyPassword(password, user.Password);
                if (!isPasswordValid)
                {
                    response.Message = "Invalid email or password.";
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    return response;
                }

                // Jika login berhasil
                response.Data = VMUser.FromDataModel(user);
                response.Message = "Login successful.";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
    }
}
