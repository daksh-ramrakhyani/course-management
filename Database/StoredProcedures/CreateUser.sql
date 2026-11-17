DROP PROCEDURE IF EXISTS CreateUser;

CREATE PROCEDURE CreateUser(
    IN p_Id CHAR(36), 
    IN p_Name LONGTEXT, 
    IN p_EmailHash VARCHAR(255),
    IN p_EmailEncypted LONGTEXT,
    IN p_EmailIV LONGTEXT, 
    IN p_EmailAuthTag LONGTEXT, 
    IN p_PasswordHash LONGTEXT, 
    IN p_Role INT, 
    IN p_CreatedAt DATETIME(6), 
    IN p_UpdatedAt DATETIME(6) )
INSERT INTO Users (Id, Name, EmailHash, EmailEncypted, EmailIV, EmailAuthTag, PasswordHash, Role, CreatedAt, UpdatedAt )
VALUES (p_Id, p_Name, p_EmailHash, p_EmailEncypted, p_EmailIV, p_EmailAuthTag, p_PasswordHash, p_Role, p_CreatedAt, p_UpdatedAt);