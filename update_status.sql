USE SmartMarketDB;
GO
UPDATE dbo.support_tickets SET status = 'RESOLVED' WHERE status = 'DONE';
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK__support_t__statu__09A971A2')
BEGIN
    ALTER TABLE dbo.support_tickets DROP CONSTRAINT CK__support_t__statu__09A971A2;
END
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Ticket_Status')
BEGIN
    ALTER TABLE dbo.support_tickets DROP CONSTRAINT CK_Ticket_Status;
END
GO
ALTER TABLE dbo.support_tickets ADD CONSTRAINT CK_Ticket_Status CHECK (status IN ('PENDING','PROCESSING','RESOLVED','CLOSED'));
GO
