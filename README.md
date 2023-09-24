# Moni Watch

  

-  ## Présentation de l'application

  

Application de gestion de compte bancaire en client lourd. Cette application doit pouvoir gérer **plusieurs comptes utilisateur** appelés "**Monies**". Un Moni peut CRUD un/plusieurs **BankAccount** (Un car création du premier prévu pour être obligatoire à la création de l'utilisateur).

Un utilisateur peut CRUD des **Transactions**, ces transactions sont liées à UN BankAccount et doivent avoir de un à plusieurs **Tags**.

Ces Tags peuvent être crées/supprimés par l'utilisateur. Je dois penser à la possibilité de les fournir par défaut, ce qui est plus génant que prévu :
- Je créé des Tag par défaut à la création de chaque utilisateur, problème : Information redondante
- Je créé un utilisateur "Fantome" qui sera le propriétaires de Tags et chaque autre utilisateur aura accès à ses Tags, problème : Je romps ma contrainte de propriété exclusive des Tags pour UN utilisateur et dois l'implementer.
- Je ne mets pas de contrainte de clé étrangère entre Moni et Tag mais j'en *simule* une avec un attribut MoniId qui pourra être nullable. Dans le cas où l'attribut est null, il s'agira d'un Tag par défaut utilisable par tous. Correspond à peu près à la solution 1 mais implique de créer un trigger pour gérer la suppression en cascade lors de la suppression d'un Moni. Il faut aussi faire attention à empècher la suppression des Tags par défaut. 
- Dernière solution -> Ne pas créer de Tags par défaut (Finalement peut être la plus logique en terme d'archi autant qu'en terme d'experience utilisateur)

Mon application tournera avec une API REST sur mon raspberry qui sera aussi le serveur de ma base de données (je pense utiliser MySQL).

*Les applications clientes ne sont pas encore définies en terme d'OS accepté. Je partirai peut être sur du WinForms par rapport à la facilité d'apprentissage (vu en cours). Mais vu que je dev à moitié sous Ubuntu et que la machine que je risque d'amener pour l'épreuve est sous Ubuntu, je pourrais me tourner ver AvaloniaUI (à voir par rapport à la courbe d'apprentissage).*

  

> ***Ajout bonus**

>_ Ajouter un système de transaction direct entre des comptes de différents utilisateurs "contacts" qui pourraient s'ajouter par recherche de login, pour le Moni source, qui envoie une demande au Moni cible.

>_ Rendre possible la répétition automatique d'une transaction à des dates données.*

  

-  ## Entités

-  ### Moni :

Les utilisateurs de l'application, ils auront un FirstName, LastName, un Login (unique) et un Password. Ce sont les propriétaires des BankAccount et des Tag.

-  ### BankAccount :

Les comptes en banque qui seront la propriété des Moni, ils contiendront les transactions. Ils auront un Name, une CreationDate, une Balance.

-  ### Tag :

Les Tag sont des balises créées par l'utilisateur pour trier leurs transactions. Elle ne possède **à priori** qu'un Label ainsi qu'une Description. Elles sont la propriété d'un (et un seul) Moni.

-  ### Transaction :

L'entité avec laquelle on fera le plus de traitement, cette entité contient un Label, une Description, un Amount, une TransactionDate. Elle est la propriété d'un BankAccount. Quand un utilisateur en rajoute une, le BankAccount associé voit son attribut Balance évoluer.

**ATTENTION**: Je pense gérer l'évolution du solde du compte associé par un trigger à l'insertion d'un transaction dans la base de données.
Cela me permettra :
- Plus de cohérence en terme d'intégrité de la donnée, quelle que soit la source de l'insertion.
- Simplification du code applicatif.
- Sécurité et maintenabilité (la mise à jour du solde sera centralisée)

Au détriment de :
- L'évolutivité -> Une insertion d'une transaction aura toujours le même effet sur le solde d'un compte (Tant mieux, c'est voulu).
- La visibilité -> Il faudra bien documenter le fait que cette composante est prise en charge dans la base de données.
	
Je suis aussi "obligé" de partir du principe qu'un utilisateur ne peut pas modifier le compte auquel appartient une transaction car plus compliqué à gérer avec TRIGGER.

----------------------------------------------


```mermaid
classDiagram
class BankAccount {
		-String Label
		-int Balance
		-Date CreationDate
		-SetBalance()
		}
class Moni {
		-String FirstName
		-String LastName
		-String Login
		-String Password
		-AddBankAccount() BankAccount
		-AddTransaction() Transaction
		}
class Transaction {
	-int Amount
	-String Label
	-Date TransactionDate	
}
class Tag {
	-String Label
	-String Description
}
class Transaction_Tag {
	
}
Moni  "1" --> "1..*" BankAccount
BankAccount "0..*" --> "1" Transaction
Moni "1" --> "1..*" Tag
Tag "1" ..> "0..*" Transaction_Tag
Transaction "1" ..> "0..*" Transaction_Tag
		
```

### MLD :
MONIS (<u>MoniId</u>, MoniLogin, MoniPwd, FirstName, LastName)
    - MoniId est la clé primaire
BANKACCOUNTS (<u>BankAccountId</u>, BankAccountLabel, BankAccountBalance, #MoniId)
    - BankAccount est la clé primaire
    - MoniId est une clé primaire de MONIS
TAGS (<u>TagId</u>, TagLabel, TagDescription, #MoniId)
    - TagId est la clé primaire
    - MoniId est une clé étrangère de MONIS
TRANSACTIONS (<u>TransactionId</u>, TransactionAmount, TransactionDate, TransactionLabel, TransactionDescription, #BankAccountId)
    - TransactionId est la clé primaire
    - BankAccountId est une clé étrangère de BANKACCOUNTS
TAGS_TRANSACTIONS(<u>#TransactionId, #TagId</u>)
    - Le couple (TransactionId, TagId) est la clé primaire
    - TransactionId est une clé étrangère de TRANSACTIONS
    - TagId est une clé étrangère de TAGS


## TODO :

- [X] Ajouter la table ManyToMany TAGS_TRANSACTIONS
- [X] Ajouter la deletion ON CASCADE
- [X] Ajouter une contrainte sur la table Transactions_Tags pour vérifier que la Transaction et le Tag viennent bien du même utilisateur.
- [X] Ajouter le TRIGGER modifiant le BankAccount.Balance associé à une Transaction inserée
- [X] Ajouter le TRIGGER modifiant le BankAccount.Balance associé à une Transaction supprimée 
- [X] Ajouter le TRIGGER modifiant le BankAccount.Balance associé à une Transaction modifiée 
- [ ] Utiliser le Secret Manager tool : https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=linux#secret-manager


## LINKS :
- Connexion à la base de données avec EFCore : https://mysqlconnector.net/tutorials/efcore/
- Documentation MariaDB : https://mariadb.com/kb/en/documentation/
- IMPORTANT -> Doc ADO.NET : https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/

## OUTILS :
### Utilisation de EFCore :
Je vais dans le sens BDD -> Code (reverse engineering)
- Etape 1 : Ajouter le package de database provider au projet(pour les differents db providers : https://learn.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli)

- Etape 2 : Scaffolding -> On ajoute le package Microsoft.EntityFrameworkCore.Design dans le project

- Etape 3 : La commande de scaffolding a besoin de 2 arguments -> la connection string et le database provider

```dotnet ef dbcontext scaffold 'Server=localhost;User=wan;Password=pswd;Database=MoniWatchI' Pomelo.EntityFrameworkCore.MySql```

- Etape 4 : Création de l'API avec : https://learn.microsoft.com/fr-fr/aspnet/core/tutorials/first-web-api?view=aspnetcore-7.0&tabs=visual-studio-code
