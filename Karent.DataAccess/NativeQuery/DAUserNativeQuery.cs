﻿using Karent.DataAccess.Interfaces;
using Karent.DataModel;
using Karent.ViewModel;
using Karent.ViewModel.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Karent.DataAccess.NativeQuery
{
    public class DAUserNativeQuery : IDAUser
    {
        private readonly KarentDBContext _db;

        // Konstruktor untuk menginisialisasi konteks database
        public DAUserNativeQuery(KarentDBContext db)
        {
            _db = db;
        }

        public VMResponse<List<VMUser>> GetByFilter(string filter)
        {
            var response = new VMResponse<List<VMUser>>();

            try
            {
                var sql = @"
                        SELECT u.id AS Id,
                            u.name AS Name,
                            u.email AS Email,
                            u.address AS Address,
                            u.phone_number AS PhoneNumber,
                            u.driving_license_number AS DrivingLicenseNumber,
                            u.user_type AS UserType,
                            u.created_by AS CreatedBy,
                            u.created_on AS CreatedOn,
                            u.modified_by AS ModifiedBy,
                            u.modified_on AS ModifiedOn
                        FROM users as u
                        WHERE u.name LIKE @p0 OR u.email LIKE @p0";

                var users = _db.VMUsers
                    .FromSqlRaw(sql, $"%{filter}%")
                    .ToList();

                if (users.Any())
                {
                    response.Data = users;
                    response.Message = $"{HttpStatusCode.OK} - {users.Count} User data(s) successfully fetched";
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
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public VMResponse<VMUser> GetById(int id)
        {
            var response = new VMResponse<VMUser>();

            if (id <= 0)
            {
                response.Message = $"{HttpStatusCode.BadRequest} - Invalid User ID";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var sql = @"
                        SELECT u.id AS Id,
                            u.name AS Name,
                            u.email AS Email,
                            u.address AS Address,
                            u.phone_number AS PhoneNumber,
                            u.driving_license_number AS DrivingLicenseNumber,
                            u.user_type AS UserType,
                            u.created_by AS CreatedBy,
                            u.created_on AS CreatedOn,
                            u.modified_by AS ModifiedBy,
                            u.modified_on AS ModifiedOn
                        FROM users as u 
                        WHERE u.id = @p0";

                var user = _db.VMUsers
                    .FromSqlRaw(sql, id)
                    .FirstOrDefault();

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
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public VMResponse<VMUser> Create(VMUser model)
        {
            var response = new VMResponse<VMUser>();

            if (!model.IsValid(out string validationMessage))
            {
                response.Message = $"{HttpStatusCode.BadRequest} - {validationMessage}";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                var sqlDuplicateCheck = @"
                        SELECT COUNT(1) AS id FROM users
                        WHERE LOWER(email) = @p0 AND phone_number = @p1 AND driving_license_number = @p2";

                var duplicateCount = _db.Users
                    .FromSqlRaw(sqlDuplicateCheck, model.Email.ToLower(), model.PhoneNumber, model.DrivingLicenseNumber)
                    .Select(u => u.Id)
                    .First();

                if (duplicateCount > 0)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Duplicate User exists.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                var insertSql = @"
                        INSERT INTO users (name, email, address, phone_number, driving_license_number, password, user_type, created_by, created_on)
                        OUTPUT INSERTED.*
                        VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8);";

                var insertedUser = _db.Users
                    .FromSqlRaw(insertSql,
                        model.Name,
                        model.Email,
                        model.Address ?? (object)DBNull.Value,
                        model.PhoneNumber ?? (object)DBNull.Value,
                        model.DrivingLicenseNumber ?? (object)DBNull.Value,
                        PasswordHasher.HashPassword(model.Password),
                        model.UserType,
                        model.CreatedBy ?? (object)DBNull.Value,
                        DateTime.Now)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (insertedUser == null)
                {
                    response.Message = $"{HttpStatusCode.InternalServerError} - Failed to insert User";
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

                response.Data = GetById(insertedUser.Id).Data;
                response.Message = $"{HttpStatusCode.Created} - User data successfully inserted";
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

        public VMResponse<VMUser> Update(VMUser model)
        {
            var response = new VMResponse<VMUser>();

            if (!model.IsValid(out string validationMessage))
            {
                response.Message = $"{HttpStatusCode.BadRequest} - {validationMessage}";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var dbTran = _db.Database.BeginTransaction();
            try
            {
                var sqlGetUser = "SELECT COUNT(1) AS id FROM users WHERE id = @p0";

                var existsCount = _db.Users
                    .FromSqlRaw(sqlGetUser, model.Id)
                    .Select(u => u.Id)
                    .First();

                if (existsCount == 0)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - User not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                var sqlDuplicateCheck = @"
                        SELECT COUNT(1) AS id FROM users
                        WHERE id != @p0 AND LOWER(email) = @p1 AND phone_number = @p2 AND driving_license_number = @p3";

                var duplicateCount = _db.Users
                    .FromSqlRaw(sqlDuplicateCheck, model.Id, model.Email.ToLower(), model.PhoneNumber, model.DrivingLicenseNumber)
                    .Select(u => u.Id)
                    .First();

                if (duplicateCount > 0)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - Duplicate User exists.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                var updateSql = @"
                        UPDATE users SET
                            name = @p1,
                            email = @p2,
                            address = @p3,
                            phone_number = @p4,
                            driving_license_number = @p5,
                            user_type = @p6,
                            modified_by = @p7,
                            modified_on = @p8
                        WHERE id = @p0;";

                _db.Database.ExecuteSqlRaw(updateSql,
                    model.Id,
                    model.Name,
                    model.Email,
                    model.Address ?? (object)DBNull.Value,
                    model.PhoneNumber ?? (object)DBNull.Value,
                    model.DrivingLicenseNumber ?? (object)DBNull.Value,
                    model.UserType,
                    model.ModifiedBy ?? (object)DBNull.Value,
                    DateTime.Now);

                response.Data = model;
                response.Message = $"{HttpStatusCode.OK} - User data successfully updated";
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
                var sqlGetUser = "SELECT COUNT(1) AS id FROM users WHERE id = @p0";

                var existsCount = _db.Users
                    .FromSqlRaw(sqlGetUser, id)
                    .Select(u => u.Id)
                    .First();

                if (existsCount == 0)
                {
                    response.Message = $"{HttpStatusCode.NotFound} - User not found";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                var sqlCheckInUse = "SELECT COUNT(1) AS id FROM rentals WHERE user_id = @p0";

                var inUseCount = _db.Rentals
                    .FromSqlRaw(sqlCheckInUse, id)
                    .Select(r => r.Id)
                    .First();

                if (inUseCount > 0)
                {
                    response.Message = $"{HttpStatusCode.Conflict} - User is currently in use and cannot be deleted.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                var deleteSql = "DELETE FROM users WHERE id = @p0";

                _db.Database.ExecuteSqlRaw(deleteSql, id);

                response.Message = $"{HttpStatusCode.OK} - User data successfully deleted";
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

        public VMResponse<VMUser> Login(string email, string password)
        {
            var response = new VMResponse<VMUser>();

            try
            {
                var sql = "SELECT * FROM users WHERE email = @p0";

                var user = _db.Users
                    .FromSqlRaw(sql, email)
                    .FirstOrDefault();

                if (user == null)
                {
                    response.Message = "Invalid email or password.";
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    return response;
                }

                bool isPasswordValid = PasswordHasher.VerifyPassword(password, user.Password);
                if (!isPasswordValid)
                {
                    response.Message = "Invalid email or password.";
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    return response;
                }

                response.Data = new VMUser
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber,
                    DrivingLicenseNumber = user.DrivingLicenseNumber,
                    //Hide password
                    UserType = user.UserType,
                    CreatedBy = user.CreatedBy,
                    CreatedOn = user.CreatedOn,
                    ModifiedBy = user.ModifiedBy,
                    ModifiedOn = user.ModifiedOn
                };
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
