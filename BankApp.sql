DROP TABLE IF EXISTS Monis;
DROP TABLE IF EXISTS BankAccounts;
DROP TABLE IF EXISTS Tags;
DROP TABLE IF EXISTS Transactions;
DROP TABLE IF EXISTS Tags_Transactions;



CREATE TABLE Monis (
    MoniId      INTEGER PRIMARY KEY AUTO_INCREMENT,
    MoniLogin   VARCHAR(32) UNIQUE NOT NULL,
    MoniPwd     VARCHAR(64) NOT NULL,
    FirstName   VARCHAR(32) NOT NULL,
    LastName    VARCHAR(32) NOT NULL,
);

CREATE INDEX idx_MoniLogin ON Monis (MoniLogin);

CREATE TABLE BankAccounts (
    BankAccountId 	    INTEGER PRIMARY KEY AUTO_INCREMENT,
    BankAccountLabel 	VARCHAR(128) NOT NULL,
    BankAccountBalance 	DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    MoniId 		        INTEGER NOT NULL,
    FOREIGN KEY (MoniId) REFERENCES Monis (MoniId)
);


CREATE TABLE Tags (
    TagId           INTEGER PRIMARY KEY AUTO_INCREMENT,
    TagLabel        VARCHAR(32) NOT NULL,
    TagDescription  VARCHAR(512),
    MoniId          INTEGER NOT NULL,
    FOREIGN KEY (MoniId) REFERENCES Monis (MoniId)
);

CREATE TABLE Transactions (
    TransactionId           INTEGER PRIMARY KEY AUTO_INCREMENT, 
    BankAccountId           INTEGER NOT NULL, 
    TransactionAmount       DECIMAL(10,2) NOT NULL, 
    TransactionDate         DATE NOT NULL, 
    TransactionLabel        VARCHAR(128) NOT NULL, 
    TransactionDescription  VARCHAR(512), 
    FOREIGN KEY(BankAccountId) REFERENCES BankAccounts(BankAccountId)
);


DELIMITER //
CREATE TRIGGER Transaction_Insert AFTER INSERT ON Transactions
FOR EACH ROW
BEGIN
    UPDATE BankAccounts 
    SET BankAccountBalance = 
    (
        SELECT BankAccountBalance
        FROM BankAccounts
        WHERE BankAccountId = NEW.BankAccountId
    ) + NEW.TransactionAmount
    WHERE BankAccountId = NEW.BankAccountId;
END;
//
DELIMITER ;
-- DONE AND VALID UP TO THIS POINT