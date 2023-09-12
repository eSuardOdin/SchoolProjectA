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





-- STILL NOT WORKING
ALTER TABLE Tags_Transactions
ADD CONSTRAINT Check_Tag_Moni
CHECK( 
    EXISTS(
        SELECT 1
        FROM Tags as t
        WHERE t.TagId = Tags_Transactions.TagId 
        AND t.MoniId = (
            SELECT MoniId FROM BankAccounts AS b
            WHERE b.BankAccountId = (
                SELECT BankAccountId FROM Transactions
                WHERE Transactions.TransactionId = Tags_Transactions.TransactionId
            )
        )   
    )
);



