IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_IncreaseSeatsOnRefund')
BEGIN
    EXEC('
    CREATE TRIGGER trg_IncreaseSeatsOnRefund
    ON Orders
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;

        -- Update available seats based on newly refunded tickets
        UPDATE est
        SET est.AvailableSeats = est.AvailableSeats + ticket_count.count
        FROM Event_Seat_Types est
        JOIN (
            SELECT t.SeatingCategoryId, COUNT(*) as count
            FROM Tickets t
            JOIN inserted i ON i.Id = t.OrderId
            JOIN deleted d ON i.Id = d.Id
            WHERE d.WasRefunded = 0 AND i.WasRefunded = 1
            GROUP BY t.SeatingCategoryId
        ) ticket_count ON est.Id = ticket_count.SeatingCategoryId;
    END;
    ')
END;