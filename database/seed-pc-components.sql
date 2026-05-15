-- Refurbished PC components catalog (SQL Server)
-- Run against QuestDb after schema exists.
-- WARNING: deletes all orders and order lines, then replaces all products (dev / reset).

USE QuestDb;
GO

DELETE FROM dbo.OrderItems;
DELETE FROM dbo.Orders;
DELETE FROM dbo.Products;
DBCC CHECKIDENT ('dbo.Products', RESEED, 0);
GO

INSERT INTO dbo.Products (Name, Price) VALUES
  -- CPUs
  (N'Intel Core i5-12400F CPU refurbished', 649.00),
  (N'Intel Core i7-12700 CPU refurbished', 1129.00),
  (N'AMD Ryzen 5 5600 CPU refurbished', 579.00),
  (N'AMD Ryzen 7 5700X CPU refurbished', 749.00),
  (N'Intel Pentium Gold G6400 CPU refurbished', 189.00),
  -- RAM
  (N'DDR4 8GB RAM 3200MHz refurbished', 129.00),
  (N'DDR4 16GB RAM 3200MHz kit refurbished', 239.00),
  (N'DDR4 32GB RAM 3200MHz kit refurbished', 449.00),
  (N'DDR5 16GB RAM 5600MHz refurbished', 329.00),
  (N'SODIMM DDR4 16GB laptop RAM refurbished', 259.00),
  -- GPUs
  (N'NVIDIA GeForce RTX 3060 12GB GPU refurbished', 1549.00),
  (N'NVIDIA GeForce GTX 1660 Super 6GB GPU refurbished', 749.00),
  (N'AMD Radeon RX 6600 8GB GPU refurbished', 999.00),
  (N'NVIDIA GeForce RTX 4060 8GB GPU refurbished', 1899.00),
  -- Storage
  (N'NVMe M.2 SSD 512GB refurbished', 229.00),
  (N'NVMe M.2 SSD 1TB refurbished', 379.00),
  (N'SATA SSD 256GB refurbished', 119.00),
  (N'HDD 1TB 7200rpm refurbished', 149.00),
  (N'HDD 2TB 7200rpm refurbished', 229.00),
  -- Motherboards
  (N'ASUS B550 motherboard AM4 refurbished', 429.00),
  (N'MSI B660 motherboard LGA1700 refurbished', 399.00),
  (N'Gigabyte H610 motherboard LGA1700 refurbished', 319.00),
  -- Power supplies
  (N'650W 80 Plus Bronze PSU modular refurbished', 279.00),
  (N'750W 80 Plus Gold PSU refurbished', 389.00),
  (N'550W power supply ATX refurbished', 189.00),
  -- Cases
  (N'Mid tower ATX PC case tempered glass refurbished', 249.00),
  (N'Micro ATX PC case compact refurbished', 159.00),
  (N'Full tower PC case black refurbished', 329.00),
  -- Cooling
  (N'CPU air cooler 120mm refurbished', 89.00),
  (N'120mm case fan PWM 4-pack refurbished', 79.00),
  (N'AIO liquid cooler 240mm refurbished', 349.00),
  -- Monitors
  (N'24 inch IPS monitor 1080p 75Hz refurbished', 449.00),
  (N'27 inch monitor 144Hz refurbished', 699.00),
  (N'22 inch office monitor refurbished', 299.00),
  -- Peripherals
  (N'Mechanical keyboard RGB refurbished', 199.00),
  (N'Wireless mouse ergonomic refurbished', 89.00),
  (N'USB headset with microphone refurbished', 129.00),
  (N'Gaming mouse pad XXL refurbished', 49.00);
GO
