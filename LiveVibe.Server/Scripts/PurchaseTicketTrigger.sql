IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_DecreaseSeatsOnPurchase')
BEGIN
    EXEC('
    CREATE TRIGGER trg_DecreaseSeatsOnPurchase
    ON Ticket_Purchases
    AFTER INSERT
    AS
    BEGIN
        SET NOCOUNT ON;

        UPDATE est
        SET est.AvailableSeats = est.AvailableSeats - 1
        FROM Event_Seat_Types est
        JOIN Tickets t ON est.Id = t.SeatingCategoryId
        JOIN inserted i ON i.TicketId = t.Id
        WHERE i.WasRefunded = 0;
    END;
    ')
END;