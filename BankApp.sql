DROP TABLE IF EXISTS Tags_Transactions;
DROP TABLE IF EXISTS Transactions;
DROP TABLE IF EXISTS Tags;
DROP TABLE IF EXISTS BankAccounts;
DROP TABLE IF EXISTS Monis;

CREATE TABLE Monis (
    MoniId      INTEGER PRIMARY KEY AUTO_INCREMENT,
    MoniLogin   VARCHAR(32) UNIQUE NOT NULL,
    MoniPwd     VARCHAR(64) NOT NULL,
    FirstName   VARCHAR(32) NOT NULL,
    LastName    VARCHAR(32) NOT NULL
) ENGINE = InnoDB;

CREATE INDEX idx_MoniLogin ON Monis (MoniLogin);

CREATE TABLE BankAccounts (
    BankAccountId INTEGER PRIMARY KEY AUTO_INCREMENT,
    BankAccountLabel VARCHAR(128) NOT NULL,
    BankAccountBalance DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    MoniId INTEGER NOT NULL,
    CONSTRAINT fk_bankaccounts_monis
        FOREIGN KEY (MoniId) REFERENCES Monis (MoniId)
        ON DELETE CASCADE
) ENGINE = InnoDB;

CREATE TABLE Tags (
    TagId INTEGER PRIMARY KEY AUTO_INCREMENT,
    TagLabel VARCHAR(32) NOT NULL,
    TagDescription VARCHAR(512),
    MoniId INTEGER NOT NULL,
    CONSTRAINT fk_tags_monis
        FOREIGN KEY (MoniId) REFERENCES Monis (MoniId)
        ON DELETE CASCADE 
) ENGINE = InnoDB;

CREATE TABLE Transactions (
    TransactionId INTEGER PRIMARY KEY AUTO_INCREMENT, 
    BankAccountId INTEGER NOT NULL, 
    TransactionAmount DECIMAL(10,2) NOT NULL, 
    TransactionDate DATE NOT NULL, 
    TransactionLabel VARCHAR(128) NOT NULL, 
    TransactionDescription VARCHAR(512),
    CONSTRAINT fk_transactions_bankaccounts
        FOREIGN KEY(BankAccountId) REFERENCES BankAccounts(BankAccountId)
        ON DELETE CASCADE
)ENGINE = InnoDB;

CREATE TABLE Tags_Transactions (
    TransactionId INTEGER NOT NULL,
    TagId INTEGER NOT NULL,
    PRIMARY KEY (TransactionId, TagId),
    CONSTRAINT fk_tags_transactions_tags
        FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId)
        ON DELETE CASCADE,
    CONSTRAINT fk_tags_transactions_transactions
        FOREIGN KEY (TagId) REFERENCES Tags(TagId)
        ON DELETE CASCADE
)ENGINE = InnoDB;

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

DELIMITER //
CREATE TRIGGER Transaction_Delete BEFORE DELETE ON Transactions
FOR EACH ROW
BEGIN
    UPDATE BankAccounts 
    SET BankAccountBalance = 
    (
        SELECT BankAccountBalance
        FROM BankAccounts
        WHERE BankAccountId = OLD.BankAccountId
    ) - OLD.TransactionAmount
    WHERE BankAccountId = OLD.BankAccountId;
END;
//
DELIMITER ;

DELIMITER //
CREATE TRIGGER Transaction_Update BEFORE UPDATE ON Transactions
FOR EACH ROW
BEGIN
    IF OLD.TransactionAmount != NEW.TransactionAmount THEN
        UPDATE BankAccounts 
        SET BankAccountBalance = 
        (
            SELECT BankAccountBalance
            FROM BankAccounts
            WHERE BankAccountId = OLD.BankAccountId
        ) - OLD.TransactionAmount + NEW.TransactionAmount
        WHERE BankAccountId = OLD.BankAccountId;
    END IF;
END;
//
DELIMITER ;


DELIMITER //
CREATE TRIGGER Check_Tag_Moni BEFORE INSERT ON Tags_Transactions
FOR EACH ROW
BEGIN
    IF (
        SELECT COUNT(*)
        FROM Tags AS t
        WHERE t.TagId = New.TagId
        AND t.MoniId = (
            SELECT MoniId FROM BankAccounts AS b
            WHERE b.BankAccountId = (
                SELECT BankAccountId FROM Transactions
                WHERE Transactions.TransactionId = NEW.TransactionId
            )
        )  
    ) = 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'The selected Tag does not belong to the Transaction owner'; 
    END IF;

END;
//
DELIMITER ;





-- DONE AND VALID UP TO THIS POINT


INSERT INTO Monis(MoniLogin, MoniPwd, FirstName, LastName) VALUES
('wan34', '1234', 'Erwann', 'Suard'),
('floflo', 'password', 'Florence', 'Chanoni');

INSERT INTO Tags(TagLabel, TagDescription, MoniId) VALUES
('Tag Erwann A', 'Premier Tag Erwann', 1),
('Tag Erwann B', 'Deuxieme Tag Erwann', 1),
('Tag Florence A', 'Premier Tag Florence', 2),
('Tag Florence B', 'Deuxieme Tag Florence', 2);

INSERT INTO BankAccounts(BankAccountLabel, BankAccountBalance, MoniId) VALUES
('Compte courant Erwann', -250.50, 1),
('Compte epargne Erwann', 10.00, 1),
('Compte courant Florence', 0.00, 2),
('Compte pro Florence', 150.00, 2);

INSERT INTO Transactions(TransactionLabel, TransactionDescription, TransactionDate, TransactionAmount, BankAccountId) VALUES
('Transaction A-B courant Erwann', 'Transaction Compte courant Erwann lié à Tag A et B', '2023-09-15', 40.50, 1),
('Transaction A epargne Erwann', 'Transaction Compte epargne Erwann lié à Tag A', '2023-09-12', 5.75, 2),
('Transaction B courant Erwann', 'Transaction Compte courant Erwann lié à Tag B', '2023-08-06', -80.10, 1),
('Transaction B pro Florence', 'Transaction Compte pro Florence lié à Tag B', '2023-08-06', 37.90, 4),
('Transaction A pro Florence', 'Transaction Compte pro Florence lié à Tag A', '2023-05-18', -54.25, 4),
('Transaction A-B courant Florence', 'Transaction Compte courant Florence lié à Tag A et B', '2023-10-06', -80.10, 3);

INSERT INTO Tags_Transactions(TagId, TransactionId) VALUES
(1, 1), (2, 1),
(1, 2),
(2, 3),
(4, 4),
(3, 5),
(3, 6), (4, 6);
