# <p align="center">GAC Warehouse Management System Integration</p>
  

#### Case Study Overview
This project implements an integration system between an enterprise application and the GAC Warehouse Management System (WMS). The integration handles data synchronization for customers, products, purchase orders, and sales orders between the two systems.

----------

### Architecture Diagarm
- GAC WMS Integration System Architecture Diagram

![alt text](https://github.com/nehakushwahaeaton/GacWmsIntegration/blob/main/Architecture_Flow_Diagarm.png?raw=true)
The GAC WMS Integration System connects different systems to GAC's Warehouse Management System (WMS). It handles both real-time data from modern ERP systems and scheduled data from older systems that use files.

## Components
### External Systems:
- **ERP Systems:** These are modern systems that send data like customer info, products, purchase orders, and sales orders directly to our system through APIs.   
- **Legacy Systems:** These are older systems that generate XML files with data and place them in specific folders for our system to pick up.
- **External Client Applications:** These are third-party apps that use our system's APIs to interact with the warehouse data.

### Integration Components:
- **Web API:** This is the part of our system that receives real-time data from ERP systems through APIs. It validates the data and sends it to the core services for processing.
- **File Processor:** This component regularly checks folders for new XML files from legacy systems, reads and processes these files, and sends the data to the core services.
- **Core Services:** These are the brains of our system. They handle all the business logic, like validating data and coordinating between different parts of the system.
- **Data Access:** This part manages how data is stored and retrieved from the database. It ensures data is saved correctly and efficiently.
- **WMS Client:** This component communicates with the GAC WMS, sending the processed data to the warehouse system and handling any responses.

### Target Systems:
- **SQL Server Database:** This is where all the data is stored, including customer info, products, orders, and logs.
- **GAC Warehouse Management System (WMS):** This is the final destination for the data. It manages warehouse operations based on the data it receives.
----------
### Data Flow
#### Real-time Integration:
<img src="https://github.com/nehakushwahaeaton/GacWmsIntegration/blob/main/Real-time-Integration.png" width="400" height="900">
- ERP systems send data to our .NET Web API.
- The API validates and processes the data, then sends it to the core services.
- The core services handle the business logic and save the data to the SQL Server database.
- The .NET WMS Client sends the data to the GAC WMS.

#### File-based Integration:
<img src="https://github.com/user-attachments/assets/63a34b9e-a536-42e0-80bc-c2a60d018bef" width="400" height="1100">
- Legacy systems generate XML files and place them in specific folders.
- The .NET File Processor detects new files, reads and processes them.
- The processed data is sent to the core services for validation and business logic.
- The data is saved to the SQL Server database and sent to the GAC WMS by the .NET WMS Client.

#### Client Application Flow:
<img src="https://github.com/nehakushwahaeaton/GacWmsIntegration/blob/main/ClientApplicationFlow.png" width="400" height="900">
- External applications call our .NET Web API to interact with the warehouse data.
- The API processes these requests through the core services.
- Data is retrieved or modified in the SQL Server database and results are returned to the client application.
----------
## Project Structure

#### GacWmsIntegration/
#### ├── GacWmsIntegration/                  # API Host project
#### ├── GacWmsIntegration.Core/             # Core business logic and domain models
#### ├── GacWmsIntegration.Infrastructure/    # Data access and external services
#### ├── GacWmsIntegration.FileProcessor/     # File processing service
#### └── GacWmsIntegration.Tests/            # Unit and integration tests
----------
## 🛠️ Getting Started
### Prerequisites
```bash
- .NET 6.0 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 or similar IDE
```

**### Database Setup**
#### 1. Create a new database in SQL Server:

CREATE DATABASE GacWmsIntegration;

#### 2. Run the database schema creation script:

| Table detail | Table query | 
| -------- | -------- 
| Create table for Customer Master Data    | CREATE TABLE CustomerMaster (CustomerID INT PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Address NVARCHAR(255) NOT NULL, CreatedDate DATETIME DEFAULT GETDATE(), CreatedBy NVARCHAR(50) DEFAULT SYSTEM_USER, ModifiedDate DATETIME DEFAULT GETDATE(), ModifiedBy NVARCHAR(50) DEFAULT SYSTEM_USER);   
| Create table for Product Master Data   | CREATE TABLE ProductMaster ( ProductCode NVARCHAR(50) PRIMARY KEY, Title NVARCHAR(100) NOT NULL, Description NVARCHAR(255), Dimensions NVARCHAR(100), CreatedDate DATETIME DEFAULT GETDATE(), CreatedBy NVARCHAR(50) DEFAULT SYSTEM_USER, ModifiedDate DATETIME DEFAULT GETDATE(), ModifiedBy NVARCHAR(50) DEFAULT SYSTEM_USER);    
| Create table for Purchase Orders (POs)    | CREATE TABLE PurchaseOrders ( OrderID INT PRIMARY KEY, ProcessingDate DATETIME NOT NULL, CustomerID INT NOT NULL, CreatedDate DATETIME DEFAULT GETDATE(), CreatedBy NVARCHAR(50) DEFAULT SYSTEM_USER, FOREIGN KEY (CustomerID) REFERENCES CustomerMaster(CustomerID) );    
| Create table for Purchase Order Details    | CREATE TABLE PurchaseOrderDetails ( OrderDetailID INT IDENTITY(1,1) PRIMARY KEY, OrderID INT NOT NULL, ProductCode NVARCHAR(50) NOT NULL, Quantity INT NOT NULL, CreatedDate DATETIME DEFAULT GETDATE(), FOREIGN KEY (OrderID) REFERENCES PurchaseOrders(OrderID), FOREIGN KEY (ProductCode) REFERENCES ProductMaster(ProductCode));
| Create table for Sales Orders (SOs)    | CREATE TABLE SalesOrders ( OrderID INT PRIMARY KEY, ProcessingDate DATETIME NOT NULL, CustomerID INT NOT NULL, ShipmentAddress NVARCHAR(255) NOT NULL, CreatedDate DATETIME DEFAULT GETDATE(), CreatedBy NVARCHAR(50) DEFAULT SYSTEM_USER,FOREIGN KEY (CustomerID) REFERENCES CustomerMaster(CustomerID) );
| Create table for Sales Order Details    | CREATE TABLE SalesOrderDetails ( OrderDetailID INT IDENTITY(1,1) PRIMARY KEY, OrderID INT NOT NULL, ProductCode NVARCHAR(50) NOT NULL, Quantity INT NOT NULL, CreatedDate DATETIME DEFAULT GETDATE(), FOREIGN KEY (OrderID) REFERENCES SalesOrders(OrderID), FOREIGN KEY (ProductCode) REFERENCES ProductMaster(ProductCode));
| Create table for tracking file processing(TODO)    | CREATE TABLE FileProcessingLog ( LogID INT IDENTITY(1,1) PRIMARY KEY, FileName NVARCHAR(255) NOT NULL, FilePath NVARCHAR(500) NOT NULL, ProcessingStatus NVARCHAR(50) NOT NULL, -- 'Pending', 'Processing', 'Completed', 'Failed' ProcessedDate DATETIME, ErrorMessage NVARCHAR(MAX), CreatedDate DATETIME DEFAULT GETDATE());
Create table for tracking WMS synchronization(TODO)| CREATE TABLE WmsSyncLog ( SyncID INT IDENTITY(1,1) PRIMARY KEY, EntityType NVARCHAR(50) NOT NULL, -- 'Customer', 'Product', 'PurchaseOrder', 'SalesOrder' EntityID NVARCHAR(50) NOT NULL, SyncStatus NVARCHAR(50) NOT NULL, -- 'Pending', 'Synced', 'Failed' SyncDate DATETIME, RetryCount INT DEFAULT 0, ErrorMessage NVARCHAR(MAX), CreatedDate DATETIME DEFAULT GETDATE() );
        
#### Table details
1. Customer Master Data - Stores basic customer information
2. Product Master Data - Stores product catalog information
3. Purchase Orders - Tracks inbound orders with customer reference
4. Purchase Order Details - Stores line items for purchase orders
5. Sales Orders - Tracks outbound orders with shipping information
6. Sales Order Details - Stores line items for sales orders
7. FileProcessingLog - Tracking the processing of XML files from legacy systems
8. WmsSyncLog - Monitoring the synchronization status with the GAC WMS

**Explanation of Database Design**

This simplified database design provides the essential structure for the GAC WMS Integration system:

**Master Data Tables:**

- CustomerMaster : stores information about clients utilizing GAC warehouse services

- ProductMaster :  contains the product catalog with basic information

Transaction Tables:

- PurchaseOrders and PurchaseOrderDetails : track inbound orders from ERP to WMS

- SalesOrders and SalesOrderDetails : track outbound orders from WMS to retail outlets

Operational Tables (recommended additions TODO):

- FileProcessingLog :  tracks the processing of XML files from legacy systems

- WmsSyncLog :  monitors the synchronization status with the GAC WMS

- SchedulerConfig :  manages the CRON-based scheduling for file polling

This structure supports both real-time API integration and file-based integration for legacy systems. The database will store all incoming data, allowing for validation, transformation, and synchronization with the GAC WMS.

#### 3. Clone the Repository: Clone the repository to your local machine :

```
https://github.com/nehakushwahaeaton/GacWmsIntegration.git
cd GacWmsIntegration
```

#### 4. Cron Schedule Documentation

Cron jobs are scheduled at recurring intervals using a format based on Unix-cron. The schedule is defined using a string format consisting of five fields separated by spaces, indicating when the job should be executed. 

The fields are as follows:

- Minute (0-59): Specifies the minute of the hour when the job should run.
- Hour (0-23): Specifies the hour of the day when the job should run.
- Day of the month (1-31): Specifies the day of the month when the job should run.
- Month (1-12 or JAN-DEC): Specifies the month when the job should run.
- Day of the week (0-6 or SUN-SAT): Specifies the day of the week when the job should run.

The format is:

> \* * * * *

Each asterisk (*) can be replaced with a specific value or a range of values to define the schedule. Here are some examples:

- */1 * * * *: Runs every minute.
- 0 0 * * *: Runs at midnight every day.
- 0 12 * * MON: Runs at noon every Monday.
- 0 0 1 * *: Runs at midnight on the first day of every month.

#### 5. Update the file processing config's:
setup to be done in GacWmsIntegration.FileProcessor appsettings.json, and create DirectoryPath
```
Configure file watchers for different file types under FileProcessing
"FileProcessing": {
  "FileWatchers": [
    {
      "Name": "CustomerFiles",
      "DirectoryPath": "C:\\Users\\e0648555\\source\\repos\\GacWmsIntegration\\FileProcessorTesting\\Import\\Customers",
      "FilePattern": "*.xml",
      "CronSchedule": "*/1 * * * *",
      "FileType": "Customer",
      "ArchiveProcessedFiles": true,
      "ArchivePath": "C:\\Users\\e0648555\\source\\repos\\GacWmsIntegration\\FileProcessorTesting\\Archive\\Customers",
      "MaxRetryAttempts": 3
    },
    {
      "Name": "ProductFiles",
      "DirectoryPath": "C:\\Users\\e0648555\\source\\repos\\GacWmsIntegration\\FileProcessorTesting\\Import\\Products",
      "FilePattern": "*.xml",
      "CronSchedule": "*/1 * * * *",
      "FileType": "Product",
      "ArchiveProcessedFiles": true,
      "ArchivePath": "C:\\Users\\e0648555\\source\\repos\\GacWmsIntegration\\FileProcessorTesting\\Archive\\Products",
      "MaxRetryAttempts": 3
    },
    {
      "Name": "PurchaseOrderFiles",
      "DirectoryPath": "C:\\Users\\e0648555\\source\\repos\\GacWmsIntegration\\FileProcessorTesting\\Import\\PurchaseOrders",
      "FilePattern": "*.xml",
      "CronSchedule": "*/1 * * * *",
      "FileType": "PurchaseOrder",
      "ArchiveProcessedFiles": true,
      "ArchivePath": "C:\\Users\\e0648555\\source\\repos\\GacWmsIntegration\\FileProcessorTesting\\Archive\\PurchaseOrders",
      "MaxRetryAttempts": 3
    },
    {
      "Name": "SalesOrderFiles",
      "DirectoryPath": "C:\\Users\\e0648555\\source\\repos\\GacWmsIntegration\\FileProcessorTesting\\Import\\SalesOrders",
      "FilePattern": "*.xml",
      "CronSchedule": "*/1 * * * *",
      "FileType": "SalesOrder",
      "ArchiveProcessedFiles": true,
      "ArchivePath": "C:\\Users\\e0648555\\source\\repos\\GacWmsIntegration\\FileProcessorTesting\\Archive\\SalesOrders",
      "MaxRetryAttempts": 3
    }
  ]
}

```

#### 6. Update the SQL connection string in appsettings.json:
setup to be done in GacWmsIntegration and GacWmsIntegration.FileProcessor appsettings.json
```
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:gacserverwarehouse.database.windows.net,1433;Initial Catalog=gacwmsdb;Persist Security Info=False;User ID=eatonadmin;Password=Qwertyuiop@12;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
```

#### 7. Running the API Host Project:
```
cd GacWmsIntegration/GacWmsIntegration
dotnet run
```
This will start the API Host project, which includes the Swagger and REST API.

#### 8. Running the File Processing Service:
Navigate to the GacWmsIntegration.FileProcessor directory and run the console application using the .NET CLI:
```
cd GacWmsIntegration/GacWmsIntegration.FileProcessor
dotnet run
```
#### 9. Swagger API look:

