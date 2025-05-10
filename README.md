# GacWmsIntegration
Gac Wms Integration


Create SQL table

-- Create table for Customer Master Data
CREATE TABLE CustomerMaster (
    CustomerID INT PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255) NOT NULL
);

-- Create table for Product Master Data
CREATE TABLE ProductMaster (
    ProductCode NVARCHAR(50) PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Dimensions NVARCHAR(100)
);

-- Create table for Purchase Orders (POs)
CREATE TABLE PurchaseOrders (
    OrderID INT PRIMARY KEY,
    ProcessingDate DATETIME NOT NULL,
    CustomerID INT NOT NULL,
    FOREIGN KEY (CustomerID) REFERENCES CustomerMaster(CustomerID)
);

-- Create table for Purchase Order Details
CREATE TABLE PurchaseOrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    ProductCode NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL,
    FOREIGN KEY (OrderID) REFERENCES PurchaseOrders(OrderID),
    FOREIGN KEY (ProductCode) REFERENCES ProductMaster(ProductCode)
);

-- Create table for Sales Orders (SOs)
CREATE TABLE SalesOrders (
    OrderID INT PRIMARY KEY,
    ProcessingDate DATETIME NOT NULL,
    CustomerID INT NOT NULL,
    ShipmentAddress NVARCHAR(255) NOT NULL,
    FOREIGN KEY (CustomerID) REFERENCES CustomerMaster(CustomerID)
);
-- Create table for Sales Order Details
CREATE TABLE SalesOrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    ProductCode NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL,
    FOREIGN KEY (OrderID) REFERENCES SalesOrders(OrderID),
    FOREIGN KEY (ProductCode) REFERENCES ProductMaster(ProductCode)
);

-- Create table for tracking file processing
CREATE TABLE FileProcessingLog (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    ProcessingStatus NVARCHAR(50) NOT NULL, -- 'Pending', 'Processing', 'Completed', 'Failed'
    ProcessedDate DATETIME,
    ErrorMessage NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Create table for tracking WMS synchronization
CREATE TABLE WmsSyncLog (
    SyncID INT IDENTITY(1,1) PRIMARY KEY,
    EntityType NVARCHAR(50) NOT NULL, -- 'Customer', 'Product', 'PurchaseOrder', 'SalesOrder'
    EntityID NVARCHAR(50) NOT NULL,
    SyncStatus NVARCHAR(50) NOT NULL, -- 'Pending', 'Synced', 'Failed'
    SyncDate DATETIME,
    RetryCount INT DEFAULT 0,
    ErrorMessage NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Create table for scheduler configuration
CREATE TABLE SchedulerConfig (
    ConfigID INT IDENTITY(1,1) PRIMARY KEY,
    JobName NVARCHAR(100) NOT NULL,
    CronExpression NVARCHAR(100) NOT NULL,
    IsEnabled BIT DEFAULT 1,
    LastRunTime DATETIME,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);

1. Customer Master Data - Stores basic customer information
2. Product Master Data - Stores product catalog information
3. Purchase Orders - Tracks inbound orders with customer reference
4. Purchase Order Details - Stores line items for purchase orders
5. Sales Orders - Tracks outbound orders with shipping information
6. Sales Order Details - Stores line items for sales orders
7. FileProcessingLog - Tracking the processing of XML files from legacy systems
8. WmsSyncLog - Monitoring the synchronization status with the GAC WMS
9. SchedulerConfig - Managing the CRON-based scheduling for file polling

Explanation of Database Design
This simplified database design provides the essential structure for the GAC WMS Integration system:

Master Data Tables:

CustomerMaster stores information about clients utilizing GAC warehouse services
ProductMaster contains the product catalog with basic information
Transaction Tables:

PurchaseOrders and PurchaseOrderDetails track inbound orders from ERP to WMS
SalesOrders and SalesOrderDetails track outbound orders from WMS to retail outlets
Operational Tables (recommended additions):

FileProcessingLog tracks the processing of XML files from legacy systems
WmsSyncLog monitors the synchronization status with the GAC WMS
SchedulerConfig manages the CRON-based scheduling for file polling
This structure supports both real-time API integration and file-based integration for legacy systems. The database will store all incoming data, allowing for validation, transformation, and synchronization with the GAC WMS.
