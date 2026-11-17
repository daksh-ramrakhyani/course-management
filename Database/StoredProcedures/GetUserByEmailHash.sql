DROP PROCEDURE IF EXISTS GetUserByEmailHash;

CREATE PROCEDURE GetUserByEmailHash(IN p_EmailHash varchar(255))
SELECT * FROM Users WHERE EmailHash = p_EmailHash;