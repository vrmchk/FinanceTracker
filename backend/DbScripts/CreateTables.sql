create table [User]
(
    [Id]             int primary key identity,
    [Email]          nvarchar(100)  not null unique,
    [UserName]       nvarchar(100)  not null unique,
    [PasswordHash]   nvarchar(1000) not null,
    [EmailConfirmed] bit            not null default 0
)
go

create table [Account]
(
    [Id]     int primary key identity,
    [Name]   nvarchar(100) not null,
    [UserId] int           not null foreign key references [User] ([Id])
)
go

create table [Category]
(
    [Id]          int primary key identity,
    [Name]        nvarchar(100)  not null,
    [Description] nvarchar(1000) not null,
    [ParentId]    int foreign key references [Category] ([Id]),
    [AccountId]   int            not null foreign key references [Account] ([Id])
)
go

create table [Currency]
(
    [Id]     int primary key identity,
    [Code]   nvarchar(10)  not null,
    [Name]   nvarchar(100) not null,
    [Symbol] nvarchar(10)  not null
)

create table [CurrencyExchangeRate]
(
    [Id]           int primary key identity,
    [ExchangeRate] decimal  not null,
    [UpdateTime]   datetime not null,
    [CurrencyId]   int      not null foreign key references [Currency] ([Id]),
)

create table [AccountCurrency]
(
    [AccountId]  int not null foreign key references [Account] ([Id]),
    [CurrencyId] int not null foreign key references [Currency] ([Id]),
    PRIMARY KEY ([AccountId], [CurrencyId])
)

create table [Transaction]
(
    [Id]             int primary key identity,
    [Type]           int            not null,
    [Amount]         decimal        not null,
    [ExecutionTime]  datetime       not null,
    [Description]    nvarchar(1000) not null,
    [AccountId]      int            not null foreign key references [Account] ([Id]),
    [CategoryId]     int            not null foreign key references [Category] ([Id]),
    [ExchangeRateId] int            not null foreign key references [CurrencyExchangeRate] ([Id]),
)