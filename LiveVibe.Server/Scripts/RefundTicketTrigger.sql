IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_IncreaseSeatsOnRefund')
BEGIN
    EXEC('
    CREATE TRIGGER trg_IncreaseSeatsOnRefund
    ON Ticket_Purchases
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;

        UPDATE est
        SET est.AvailableSeats = est.AvailableSeats + 1
        FROM Event_Seat_Types est
        JOIN Tickets t ON est.Id = t.SeatingCategoryId
        JOIN inserted i ON i.TicketId = t.Id
        JOIN deleted d ON i.Id = d.Id
        WHERE d.WasRefunded = 0 AND i.WasRefunded = 1;
    END;
    ')
END;