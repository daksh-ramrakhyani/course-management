DROP PROCEDURE IF EXISTS UpdateUser;

CREATE PROCEDURE UpdateUser(
    IN p_Id CHAR(36), 
    IN p_Name LONGTEXT, 
    IN p_EmailHash VARCHAR(255),
    IN p_EmailEncypted LONGTEXT,
    IN p_EmailIV LONGTEXT, 
    IN p_EmailAuthTag LONGTEXT, 
    IN p_UpdatedAt DATETIME(6)
)
UPDATE Users 
SET Name = p_Name, EmailHash = p_EmailHash, EmailEncypted = p_EmailEncypted, EmailIV = p_EmailIV, EmailAuthTag = p_EmailAuthTag, UpdatedAt =p_UpdatedAt
WHERE Id = p_Id;