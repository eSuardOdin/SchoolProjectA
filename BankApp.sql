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




-- DONE AND VALID UP TO THIS POINT




-- Créez une contrainte d'intégrité pour vérifier que chaque Tag associé à une Transaction
-- correspond bien à l'utilisateur faisant la Transaction.
ALTER TABLE Transactions
ADD CONSTRAINT CHK_Tag_User
CHECK (EXISTS (
    SELECT 1
    FROM Tags AS t
    WHERE t.TagID = Transactions.TagID
    AND t.UserID = (
        SELECT UserID
        FROM Accounts AS a
        WHERE a.AccountID = Transactions.AccountID
    )
));
