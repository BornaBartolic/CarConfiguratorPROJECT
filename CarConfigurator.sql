-- ======================================
-- BAZA: CarConfiguratorDB
-- ======================================

Create database AutoConfigDB	
use AutoConfigDB

-- Tablica: Role
CREATE TABLE Role (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

-- Inicijalne role
INSERT INTO Role (Name) VALUES ('Admin'), ('User');


-- Tablica: ComponentType (1-na-N entitet)
CREATE TABLE ComponentType (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);



-- Tablica: CarComponent (primarni entitet)
CREATE TABLE CarComponent (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Price DECIMAL(10,2) NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(255) NULL,
    ComponentTypeId INT NOT NULL,
    CONSTRAINT FK_CarComponent_ComponentType FOREIGN KEY (ComponentTypeId)
        REFERENCES ComponentType(Id)
);



-- Tablica: CarComponentCompatibility (M-na-N)
CREATE TABLE CarComponentCompatibility (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CarComponentId1 INT NOT NULL,
    CarComponentId2 INT NOT NULL,
    CONSTRAINT UQ_CarComponentCompatibility UNIQUE (CarComponentId1, CarComponentId2),
    CONSTRAINT FK_CarComponentCompatibility_1 FOREIGN KEY (CarComponentId1)
        REFERENCES CarComponent(Id),
    CONSTRAINT FK_CarComponentCompatibility_2 FOREIGN KEY (CarComponentId2)
        REFERENCES CarComponent(Id),
    CONSTRAINT CHK_CarComponentCompatibility_NoSelfCheck CHECK (CarComponentId1 <> CarComponentId2)
);





-- Tablica: User (s RoleId)
CREATE TABLE [User] (
    Id INT IDENTITY(1,1) PRIMARY KEY,         
    Username NVARCHAR(50) NOT NULL UNIQUE,    
    Email NVARCHAR(100) NOT NULL UNIQUE,      
    PasswordHash NVARCHAR(255) NOT NULL,      
    Salt NVARCHAR(255) NOT NULL DEFAULT NEWID(), 
    RoleId INT NOT NULL DEFAULT 2,  -- default = User
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleId)
        REFERENCES Role(Id)
);



-- Tablica: UserCarComponentSelection
CREATE TABLE UserCarComponentSelection (
    UserId INT NOT NULL,
    CarComponentId INT NOT NULL,
    PRIMARY KEY (UserId, CarComponentId),
    CONSTRAINT FK_UserCarComponentSelection_User FOREIGN KEY (UserId)
        REFERENCES [User](Id),
    CONSTRAINT FK_UserCarComponentSelection_CarComponent FOREIGN KEY (CarComponentId)
        REFERENCES CarComponent(Id)
);


-- Tablica: Log (zapisnik)
CREATE TABLE Log (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Timestamp] DATETIME NOT NULL DEFAULT GETDATE(),
    [Level] NVARCHAR(20) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL
);

-- Primjeri log zapisa
INSERT INTO Log ([Level], Message) VALUES
('Info', 'Sistem pokrenut.'),
('Info', 'Korisnik admin je kreiran.'),
('Warning', 'Neuspjela prijava korisnika user1.');


-- Provjera podataka
SELECT * FROM CarComponent;
SELECT * FROM ComponentType;
SELECT u.Id, u.Username, u.Email, r.Name AS RoleName
FROM [User] u
JOIN Role r ON u.RoleId = r.Id;

CREATE TABLE CarConfiguration (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL DEFAULT 'Unnamed Configuration',       
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE CarConfigurationComponent (
    CarConfigurationId INT NOT NULL,
    CarComponentId INT NOT NULL,
    PRIMARY KEY (CarConfigurationId, CarComponentId),
    CONSTRAINT FK_ConfigComponent_Config
        FOREIGN KEY (CarConfigurationId) REFERENCES CarConfiguration(Id),
    CONSTRAINT FK_ConfigComponent_Component
        FOREIGN KEY (CarComponentId) REFERENCES CarComponent(Id)
);

INSERT INTO [User] (Username, Email, PasswordHash, Salt, RoleId)
VALUES
('a', 'a@example.com',
 'if/ECQHzDEs7RbRT46G35lEX2oknGeRPpJA0rPjphOY=',
 'txEfJEdOC6hJxUjNvj5HUQ==',
 1),

('b', 'b@example.com',
 'fw6XGrq6Ymx6mfTXo5ccPLFhV70YFL7GOXp6Fd05brk=',
 'dmrVAgQzCGIaQPTyum1W5g==',
 2);

