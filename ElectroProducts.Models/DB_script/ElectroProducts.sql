-- Tworzenie bazy danych
CREATE DATABASE ElectroProducts;
GO

USE ElectroProducts;
GO

-- Tabela Products
CREATE TABLE Products (
    Id       BIGINT IDENTITY(1,1) PRIMARY KEY,
    SKU      NVARCHAR(100)  NOT NULL,
    Name     NVARCHAR(MAX)  NULL,
    EAN      NVARCHAR(50)   NULL,
    ProducerName NVARCHAR(MAX) NULL,
    Category NVARCHAR(300)  NULL,
    PhotoUrl NVARCHAR(MAX)  NULL

    -- Unikalny indeks na kolumnie SKU, aby zapewnić unikalność wartości
    -- i umożliwić szybkie wyszukiwanie produktów po SKU
    -- Dzięki temu, że SKU jest unikalne, możemy łatwo łączyć tabele Prices i Inventories z tabelą Products za pomocą klucza obcego.
    CONSTRAINT UQ_Products_SKU UNIQUE (SKU)
);
GO

-- Tabela Prices
CREATE TABLE Prices (
    Id                 BIGINT IDENTITY(1,1) PRIMARY KEY,
    SKU                NVARCHAR(100)   NOT NULL,
    LogisticUnitPrice  DECIMAL(18, 2)  NULL,

    CONSTRAINT FK_Prices_Products FOREIGN KEY (SKU)
        REFERENCES Products(SKU)
);
GO

-- Tabela Inventories
CREATE TABLE Inventories (
    Id             BIGINT IDENTITY(1,1) PRIMARY KEY,
    SKU            NVARCHAR(100)  NOT NULL,
    LogisticUnit   NVARCHAR(20)  NULL,
    StockQuantity  DECIMAL(18, 3) NULL,
    ShippingCost   DECIMAL(18, 2) NULL,

    CONSTRAINT FK_Inventories_Products FOREIGN KEY (SKU)
        REFERENCES Products(SKU)
);
GO

-- Indeksy na kolumnie SKU dla lepszej wydajności zapytań
CREATE INDEX IX_Products_SKU    ON Products(SKU);
CREATE INDEX IX_Prices_SKU      ON Prices(SKU);
CREATE INDEX IX_Inventories_SKU ON Inventories(SKU);
GO