-- Create Users table
CREATE TABLE Users (
    UserId SERIAL PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Elo INT DEFAULT 0
);

-- Create PushUpRecords table
CREATE TABLE PushUpRecords (
    RecordId SERIAL PRIMARY KEY,
    UserId INT REFERENCES Users(UserId) ON DELETE CASCADE,
    Count INT NOT NULL,
    Duration INTERVAL NOT NULL,
    Timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Tournaments table
CREATE TABLE Tournaments (
    TournamentId SERIAL PRIMARY KEY,
    StartTime TIMESTAMP NOT NULL,
    EndTime TIMESTAMP NOT NULL
);