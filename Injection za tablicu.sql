

BEGIN TRAN;

------------------------------------------------------------
-- 1) Ensure ComponentTypes (English)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM ComponentType WHERE Name = N'Car Type')
    INSERT INTO ComponentType (Name) VALUES (N'Car Type');

IF NOT EXISTS (SELECT 1 FROM ComponentType WHERE Name = N'Engine')
    INSERT INTO ComponentType (Name) VALUES (N'Engine');

IF NOT EXISTS (SELECT 1 FROM ComponentType WHERE Name = N'Equipment Package')
    INSERT INTO ComponentType (Name) VALUES (N'Equipment Package');

IF NOT EXISTS (SELECT 1 FROM ComponentType WHERE Name = N'Wheel Size')
    INSERT INTO ComponentType (Name) VALUES (N'Wheel Size');

IF NOT EXISTS (SELECT 1 FROM ComponentType WHERE Name = N'Colour')
    INSERT INTO ComponentType (Name) VALUES (N'Colour');

IF NOT EXISTS (SELECT 1 FROM ComponentType WHERE Name = N'Drivetrain')
    INSERT INTO ComponentType (Name) VALUES (N'Drivetrain');

IF NOT EXISTS (SELECT 1 FROM ComponentType WHERE Name = N'Additional Services')
    INSERT INTO ComponentType (Name) VALUES (N'Additional Services');

DECLARE @T_CarType  INT = (SELECT Id FROM ComponentType WHERE Name = N'Car Type');
DECLARE @T_Engine   INT = (SELECT Id FROM ComponentType WHERE Name = N'Engine');
DECLARE @T_Package  INT = (SELECT Id FROM ComponentType WHERE Name = N'Equipment Package');
DECLARE @T_Wheels   INT = (SELECT Id FROM ComponentType WHERE Name = N'Wheel Size');
DECLARE @T_Colour   INT = (SELECT Id FROM ComponentType WHERE Name = N'Colour');
DECLARE @T_Drive    INT = (SELECT Id FROM ComponentType WHERE Name = N'Drivetrain');
DECLARE @T_Service  INT = (SELECT Id FROM ComponentType WHERE Name = N'Additional Services');

------------------------------------------------------------
-- 2) Insert Car Types
------------------------------------------------------------
INSERT INTO CarComponent (Name, Description, Price, ImageUrl, ComponentTypeId)
SELECT v.Name, v.Description, 0, NULL, @T_CarType
FROM (VALUES
 (N'SUV',       N'Sport Utility Vehicle'),
 (N'Sedan',     N'Comfortable passenger car'),
 (N'Hatchback', N'Compact 5-door'),
 (N'Supercar',  N'High-performance sports car')
) v(Name, Description)
WHERE NOT EXISTS (
  SELECT 1 FROM CarComponent c WHERE c.ComponentTypeId=@T_CarType AND c.Name=v.Name
);

------------------------------------------------------------
-- 3) Insert Engines
------------------------------------------------------------
INSERT INTO CarComponent (Name, Description, Price, ImageUrl, ComponentTypeId)
SELECT v.Name, v.Description, v.Price, NULL, @T_Engine
FROM (VALUES
 (N'1.0L Petrol', N'1.0L petrol engine', 1200.00),
 (N'1.5L Petrol', N'1.5L petrol engine', 1600.00),
 (N'2.0L Petrol', N'2.0L petrol engine', 2300.00),
 (N'2.5L Turbo',  N'2.5L turbocharged engine', 3800.00)
) v(Name, Description, Price)
WHERE NOT EXISTS (
  SELECT 1 FROM CarComponent c WHERE c.ComponentTypeId=@T_Engine AND c.Name=v.Name
);

------------------------------------------------------------
-- 4) Insert Equipment Packages
------------------------------------------------------------
INSERT INTO CarComponent (Name, Description, Price, ImageUrl, ComponentTypeId)
SELECT v.Name, v.Description, v.Price, NULL, @T_Package
FROM (VALUES
 (N'Standard', N'Basic equipment package', 0.00),
 (N'Comfort',  N'Comfort features (heated seats, better audio)', 900.00),
 (N'Premium',  N'Premium features (leather, premium audio, assist)', 1800.00)
) v(Name, Description, Price)
WHERE NOT EXISTS (
  SELECT 1 FROM CarComponent c WHERE c.ComponentTypeId=@T_Package AND c.Name=v.Name
);

------------------------------------------------------------
-- 5) Insert Wheel Sizes
------------------------------------------------------------
INSERT INTO CarComponent (Name, Description, Price, ImageUrl, ComponentTypeId)
SELECT v.Name, v.Description, v.Price, NULL, @T_Wheels
FROM (VALUES
 (N'16"', N'16 inch wheels', 500.00),
 (N'18"', N'18 inch wheels', 850.00),
 (N'20"', N'20 inch wheels', 1400.00),
 (N'22"', N'22 inch wheels', 2100.00)
) v(Name, Description, Price)
WHERE NOT EXISTS (
  SELECT 1 FROM CarComponent c WHERE c.ComponentTypeId=@T_Wheels AND c.Name=v.Name
);

------------------------------------------------------------
-- 6) Insert Colours
------------------------------------------------------------
INSERT INTO CarComponent (Name, Description, Price, ImageUrl, ComponentTypeId)
SELECT v.Name, v.Description, v.Price, NULL, @T_Colour
FROM (VALUES
 (N'Solid White',   N'Solid white paint', 0.00),
 (N'Midnight Black',N'Gloss black paint', 450.00),
 (N'Metallic Blue', N'Metallic blue paint', 650.00),
 (N'Race Red',      N'Sport red paint', 650.00)
) v(Name, Description, Price)
WHERE NOT EXISTS (
  SELECT 1 FROM CarComponent c WHERE c.ComponentTypeId=@T_Colour AND c.Name=v.Name
);

------------------------------------------------------------
-- 7) Insert Drivetrains
------------------------------------------------------------
INSERT INTO CarComponent (Name, Description, Price, ImageUrl, ComponentTypeId)
SELECT v.Name, v.Description, v.Price, NULL, @T_Drive
FROM (VALUES
 (N'FWD', N'Front-wheel drive', 0.00),
 (N'RWD', N'Rear-wheel drive', 300.00),
 (N'AWD', N'All-wheel drive', 900.00)
) v(Name, Description, Price)
WHERE NOT EXISTS (
  SELECT 1 FROM CarComponent c WHERE c.ComponentTypeId=@T_Drive AND c.Name=v.Name
);

------------------------------------------------------------
-- 8) Insert Additional Services (fun + realistic)
------------------------------------------------------------
INSERT INTO CarComponent (Name, Description, Price, ImageUrl, ComponentTypeId)
SELECT v.Name, v.Description, v.Price, NULL, @T_Service
FROM (VALUES
 (N'Annual Maintenance Plan', N'1-year scheduled maintenance subscription', 399.00),
 (N'Connected Services',      N'1-year connected services subscription (remote lock, status)', 149.00),
 (N'Track Day Experience',    N'One track day with instructor (limited availability)', 699.00),
 (N'Extended Warranty (2y)',  N'2-year extended warranty', 499.00)
) v(Name, Description, Price)
WHERE NOT EXISTS (
  SELECT 1 FROM CarComponent c WHERE c.ComponentTypeId=@T_Service AND c.Name=v.Name
);

------------------------------------------------------------
-- 9) Compatibility links: Car Type -> everything else
--    IMPORTANT: CarComponentId1 = Car Type, CarComponentId2 = other component
------------------------------------------------------------
DECLARE @Pairs TABLE (CarTypeName NVARCHAR(100), OtherName NVARCHAR(100));

-- SUV
INSERT INTO @Pairs VALUES
(N'SUV', N'1.5L Petrol'),
(N'SUV', N'2.0L Petrol'),
(N'SUV', N'2.5L Turbo'),
(N'SUV', N'Standard'),
(N'SUV', N'Comfort'),
(N'SUV', N'Premium'),
(N'SUV', N'18"'),
(N'SUV', N'20"'),
(N'SUV', N'22"'),
(N'SUV', N'Solid White'),
(N'SUV', N'Midnight Black'),
(N'SUV', N'Metallic Blue'),
(N'SUV', N'Race Red'),
(N'SUV', N'FWD'),
(N'SUV', N'AWD'),
(N'SUV', N'Annual Maintenance Plan'),
(N'SUV', N'Connected Services'),
(N'SUV', N'Extended Warranty (2y)');

-- Sedan
INSERT INTO @Pairs VALUES
(N'Sedan', N'1.0L Petrol'),
(N'Sedan', N'1.5L Petrol'),
(N'Sedan', N'2.0L Petrol'),
(N'Sedan', N'Standard'),
(N'Sedan', N'Comfort'),
(N'Sedan', N'Premium'),
(N'Sedan', N'16"'),
(N'Sedan', N'18"'),
(N'Sedan', N'20"'),
(N'Sedan', N'Solid White'),
(N'Sedan', N'Midnight Black'),
(N'Sedan', N'Metallic Blue'),
(N'Sedan', N'Race Red'),
(N'Sedan', N'FWD'),
(N'Sedan', N'AWD'),
(N'Sedan', N'Annual Maintenance Plan'),
(N'Sedan', N'Connected Services'),
(N'Sedan', N'Extended Warranty (2y)');

-- Hatchback
INSERT INTO @Pairs VALUES
(N'Hatchback', N'1.0L Petrol'),
(N'Hatchback', N'1.5L Petrol'),
(N'Hatchback', N'Standard'),
(N'Hatchback', N'Comfort'),
(N'Hatchback', N'16"'),
(N'Hatchback', N'18"'),
(N'Hatchback', N'Solid White'),
(N'Hatchback', N'Midnight Black'),
(N'Hatchback', N'Metallic Blue'),
(N'Hatchback', N'FWD'),
(N'Hatchback', N'Annual Maintenance Plan'),
(N'Hatchback', N'Connected Services');

-- Supercar
INSERT INTO @Pairs VALUES
(N'Supercar', N'2.0L Petrol'),
(N'Supercar', N'2.5L Turbo'),
(N'Supercar', N'Premium'),
(N'Supercar', N'20"'),
(N'Supercar', N'22"'),
(N'Supercar', N'Midnight Black'),
(N'Supercar', N'Race Red'),
(N'Supercar', N'RWD'),
(N'Supercar', N'AWD'),
(N'Supercar', N'Track Day Experience'),
(N'Supercar', N'Extended Warranty (2y)');

-- Insert pairs (skip if missing component names)
;WITH Resolved AS (
    SELECT
        ct.Id AS CarTypeId,
        o.Id  AS OtherId
    FROM @Pairs p
    JOIN CarComponent ct ON ct.Name = p.CarTypeName AND ct.ComponentTypeId = @T_CarType
    JOIN CarComponent o  ON o.Name  = p.OtherName
),
ToInsert AS (
    SELECT CarTypeId AS Id1, OtherId AS Id2
    FROM Resolved
    WHERE CarTypeId <> OtherId
)
INSERT INTO CarComponentCompatibility (CarComponentId1, CarComponentId2)
SELECT t.Id1, t.Id2
FROM ToInsert t
WHERE NOT EXISTS (
    SELECT 1
    FROM CarComponentCompatibility c
    WHERE c.CarComponentId1 = t.Id1 AND c.CarComponentId2 = t.Id2
);

COMMIT TRAN;
GO

------------------------------------------------------------
-- Quick checks
------------------------------------------------------------
SELECT ct.Name AS ComponentType, COUNT(*) AS Components
FROM CarComponent c
JOIN ComponentType ct ON ct.Id = c.ComponentTypeId
GROUP BY ct.Name
ORDER BY ct.Name;

SELECT TOP 50
  c1.Name AS CarType,
  c2.Name AS CompatibleWith
FROM CarComponentCompatibility cc
JOIN CarComponent c1 ON c1.Id = cc.CarComponentId1
JOIN CarComponent c2 ON c2.Id = cc.CarComponentId2
ORDER BY c1.Name, c2.Name;
GO
