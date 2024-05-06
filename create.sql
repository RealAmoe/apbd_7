-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2024-04-19 09:16:13.369

-- tables
-- Table: Animal
CREATE TABLE Animal (
    ID int  NOT NULL IDENTITY,
    Name nvarchar(100)  NOT NULL,
    AdmissionDate date  NOT NULL,
    Owner_ID int  NOT NULL,
    Animal_Class_ID int  NOT NULL,
    CONSTRAINT Animal_pk PRIMARY KEY  (ID)
);

-- Table: Animal_Class
CREATE TABLE Animal_Class (
    ID int  NOT NULL IDENTITY,
    Name nvarchar(100)  NOT NULL,
    CONSTRAINT Animal_Class_pk PRIMARY KEY  (ID)
);

-- Table: Owner
CREATE TABLE Owner (
    ID int  NOT NULL IDENTITY,
    FirstName nvarchar(100)  NOT NULL,
    LastName nvarchar(100)  NOT NULL,
    CONSTRAINT Owner_pk PRIMARY KEY  (ID)
);

-- Table: Procedure
CREATE TABLE "Procedure" (
    ID int  NOT NULL IDENTITY,
    Name nvarchar(100)  NOT NULL,
    Description nvarchar(100)  NOT NULL,
    CONSTRAINT Procedure_pk PRIMARY KEY  (ID)
);

-- Table: Procedure_Animal
CREATE TABLE Procedure_Animal (
    Procedure_ID int  NOT NULL,
    Animal_ID int  NOT NULL,
    Date date  NOT NULL,
    CONSTRAINT Procedure_Animal_pk PRIMARY KEY  (Procedure_ID,Animal_ID,Date)
);

-- foreign keys
-- Reference: Animal_Animal_Class (table: Animal)
ALTER TABLE Animal ADD CONSTRAINT Animal_Animal_Class
    FOREIGN KEY (Animal_Class_ID)
    REFERENCES Animal_Class (ID);

-- Reference: Animal_Owner (table: Animal)
ALTER TABLE Animal ADD CONSTRAINT Animal_Owner
    FOREIGN KEY (Owner_ID)
    REFERENCES Owner (ID);

-- Reference: Procedure_Animal_Animal (table: Procedure_Animal)
ALTER TABLE Procedure_Animal ADD CONSTRAINT Procedure_Animal_Animal
    FOREIGN KEY (Animal_ID)
    REFERENCES Animal (ID);

-- Reference: Procedure_Animal_Procedure (table: Procedure_Animal)
ALTER TABLE Procedure_Animal ADD CONSTRAINT Procedure_Animal_Procedure
    FOREIGN KEY (Procedure_ID)
    REFERENCES "Procedure" (ID);

-- End of file.

INSERT INTO Owner
VALUES ('Jan', 'Kowalski');
INSERT INTO Owner
VALUES ('Anna', 'Nowak');

INSERT INTO Animal_Class
VALUES ('Dog');
INSERT INTO Animal_Class
VALUES ('Cat');

INSERT INTO Animal
VALUES ('Wanwan', '2024-04-19', 1, 1);
INSERT INTO Animal
VALUES ('Nyah', '2024-04-19', 2, 2);

INSERT INTO [Procedure]
VALUES ('Name1', 'Desc1');
INSERT INTO [Procedure]
VALUES ('Name2', 'Desc2');
INSERT INTO [Procedure]
VALUES ('Name3', 'Desc3');

INSERT INTO Procedure_Animal
VALUES (1, 1, '2024-04-21')
INSERT INTO Procedure_Animal
VALUES (2, 1, '2024-04-21')
INSERT INTO Procedure_Animal
VALUES (3, 1, '2024-04-21')