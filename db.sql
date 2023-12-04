CREATE TABLE [dbo].[Inventory] (
    [SKU]          VARCHAR (255)   NOT NULL,
    [Unit]         VARCHAR (255)   NULL,
    [Qty]          INT             NOT NULL,
    [ShippingCost] DECIMAL (18, 2) NULL,
    PRIMARY KEY CLUSTERED ([SKU] ASC)
);

CREATE TABLE [dbo].[Prices] (
    [SKU]       VARCHAR (255)   NOT NULL,
    [NettPrice] DECIMAL (18, 2) NULL
);

CREATE TABLE [dbo].[Products] (
    [SKU]          NVARCHAR (255) NOT NULL,
    [Name]         NVARCHAR (255) NOT NULL,
    [EAN]          NVARCHAR (255) NULL,
    [ProducerName] NVARCHAR (255) NULL,
    [Category]     NVARCHAR (255) NULL,
    [DefaultImage] NVARCHAR (255) NULL,
    PRIMARY KEY CLUSTERED ([SKU] ASC)
);

