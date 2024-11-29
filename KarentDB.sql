CREATE DATABASE KarentDB
GO

USE KarentDB
GO

-- Tabel users
CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,  
    email VARCHAR(255) NOT NULL, 
    address VARCHAR(255) NULL, 
    phone_number VARCHAR(20) NULL, 
    driving_license_number VARCHAR(50) NULL, 
    password VARCHAR(255) NOT NULL, 
    user_type VARCHAR(10) CHECK (user_type IN ('admin', 'customer')) DEFAULT 'customer' NOT NULL, 
    created_by INT NULL, 
    created_on DATETIME DEFAULT GETDATE(), 
    modified_by INT NULL, 
    modified_on DATETIME NULL
);

-- Tabel cars
CREATE TABLE cars (
    id INT IDENTITY(1,1) PRIMARY KEY,
    brand VARCHAR(100) NOT NULL,
    model VARCHAR(100) NOT NULL,
    year INT NOT NULL,
    plate_number VARCHAR(20) NOT NULL,
    rental_rate_per_day DECIMAL(10, 2) NOT NULL,
    late_rate_per_day DECIMAL(10, 2) NOT NULL,
    status VARCHAR(20) CHECK (status IN ('available', 'rented', 'maintenance')) DEFAULT 'available' NOT NULL,
    created_by INT NULL, 
    created_on DATETIME DEFAULT GETDATE(), 
    modified_by INT NULL, 
    modified_on DATETIME NULL
);

-- Tabel rentals
CREATE TABLE rentals (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    car_id INT NOT NULL,
    start_date DATETIME NOT NULL,
    end_date DATETIME NOT NULL,
    total_fee DECIMAL(10, 2) NOT NULL,
    created_by INT NULL, 
    created_on DATETIME DEFAULT GETDATE(), 
    modified_by INT NULL, 
    modified_on DATETIME NULL
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (car_id) REFERENCES cars(id)
);

-- Tabel rental_returns
CREATE TABLE rental_returns (
    id INT IDENTITY(1,1) PRIMARY KEY,
    rental_id INT NOT NULL,
    return_date DATETIME NOT NULL,
    late_fee DECIMAL(10, 2) NOT NULL,
    total_fee DECIMAL(10, 2) NOT NULL,
    created_by INT NULL,
    created_on DATETIME DEFAULT GETDATE(),
    modified_by INT NULL,
    modified_on DATETIME NULL,
    FOREIGN KEY (rental_id) REFERENCES rentals(id)
);