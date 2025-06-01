IF NOT EXISTS (
    SELECT * FROM sys.check_constraints 
    WHERE name = 'CHK_AvailableSeats_NonNegative'
)
BEGIN
    ALTER TABLE Event_Seat_Types
    ADD CONSTRAINT CHK_AvailableSeats_NonNegative CHECK (AvailableSeats >= 0);
END;