# E-commerce App

## Description

This project is a fullstack application built with:

- Angular (frontend)
- ASP.NET Core Web API (backend)
- SQL Server (database)

It implements a basic product catalog with data fetched from a SQL Server database and displayed in an Angular UI.

## Features (so far)

- Product catalog
  - Fetch products from SQL Server database
  - Expose REST API endpoint: GET /api/products
  - Display products in Angular UI
- Backend built without ORM (ADO.NET)

## How to run

## Backend Setup

1. Navigate to the backend folder:
   cd backend

2. Run the API:
   dotnet run

3. The API will be available at:
   http://localhost:5261/api/products

## Database Setup

1. Install SQL Server and SSMS
2. Create a database named: QuestDb
3. Create table:

CREATE TABLE Products (
Id INT PRIMARY KEY IDENTITY,
Name NVARCHAR(100),
Price DECIMAL(10,2)
);

4. Insert sample data into Products table

## Frontend Setup

1. Navigate to frontend folder:
   cd frontend

2. Install dependencies:
   npm install

3. Run the app:
   ng serve

4. Open:
   http://localhost:4200

## Notes

- Backend calculates and provides product data
- No ORM is used (ADO.NET with SqlConnection)
- Angular consumes the API via HttpClient
