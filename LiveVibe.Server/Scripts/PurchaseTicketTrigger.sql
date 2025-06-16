IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_DecreaseSeatsOnPurchase')
BEGIN
    EXEC('
    CREATE TRIGGER trg_DecreaseSeatsOnPurchase
    ON Tickets
    AFTER INSERT
    AS
    BEGIN
        SET NOCOUNT ON;

        -- Update available seats based on newly inserted tickets
        UPDATE est
        SET est.AvailableSeats = est.AvailableSeats - ticket_count.count
        FROM Event_Seat_Types est
        JOIN (
            SELECT SeatingCategoryId, COUNT(*) as count
            FROM inserted
            WHERE WasRefunded = 0
            GROUP BY SeatingCategoryId
        ) ticket_count ON est.Id = ticket_count.SeatingCategoryId;
    END;
    ')
END;