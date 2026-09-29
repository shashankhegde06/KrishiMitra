using KrishiMitra.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KrishiMitra.Infrastructure.Migrations;

[DbContext(typeof(FarmDbContext))]
[Migration("202609280001_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(@"
CREATE TABLE Farms (
 Id INTEGER NOT NULL CONSTRAINT PK_Farms PRIMARY KEY AUTOINCREMENT,
 FarmName TEXT NOT NULL, FarmerName TEXT NOT NULL, Location TEXT NOT NULL,
 District TEXT NOT NULL, State TEXT NOT NULL, CropType TEXT NOT NULL,
 AreaInAcres TEXT NOT NULL, PlantingDate TEXT NULL, Notes TEXT NULL,
 IsDemo INTEGER NOT NULL, CreatedAt TEXT NOT NULL, UpdatedAt TEXT NOT NULL);
CREATE INDEX IX_Farms_FarmName ON Farms (FarmName);
CREATE TABLE Knowledge (
 Id TEXT NOT NULL CONSTRAINT PK_Knowledge PRIMARY KEY, Title TEXT NOT NULL,
 CropType TEXT NOT NULL, Language TEXT NOT NULL, Content TEXT NOT NULL,
 SourceName TEXT NOT NULL, SourceUrl TEXT NULL, LastReviewedAt TEXT NULL);
CREATE TABLE Activities (
 Id INTEGER NOT NULL CONSTRAINT PK_Activities PRIMARY KEY AUTOINCREMENT,
 FarmId INTEGER NOT NULL, ActivityType TEXT NOT NULL, ActivityDate TEXT NOT NULL,
 Description TEXT NOT NULL, Quantity TEXT NULL, Unit TEXT NULL, ExpenseAmount TEXT NULL,
 Notes TEXT NULL, CreatedAt TEXT NOT NULL,
 CONSTRAINT FK_Activities_Farms_FarmId FOREIGN KEY (FarmId) REFERENCES Farms (Id) ON DELETE CASCADE);
CREATE INDEX IX_Activities_FarmId_ActivityDate ON Activities (FarmId, ActivityDate);
CREATE TABLE Reminders (
 Id INTEGER NOT NULL CONSTRAINT PK_Reminders PRIMARY KEY AUTOINCREMENT,
 FarmId INTEGER NOT NULL, Title TEXT NOT NULL, Description TEXT NULL, DueDate TEXT NOT NULL,
 IsCompleted INTEGER NOT NULL, CreatedAt TEXT NOT NULL,
 CONSTRAINT FK_Reminders_Farms_FarmId FOREIGN KEY (FarmId) REFERENCES Farms (Id) ON DELETE CASCADE);
CREATE INDEX IX_Reminders_FarmId_DueDate_IsCompleted ON Reminders (FarmId, DueDate, IsCompleted);
CREATE TABLE ChatMessages (
 Id INTEGER NOT NULL CONSTRAINT PK_ChatMessages PRIMARY KEY AUTOINCREMENT,
 Role TEXT NOT NULL, Content TEXT NOT NULL, CreatedAt TEXT NOT NULL);
INSERT INTO Farms (Id, FarmName, FarmerName, Location, District, State, CropType, AreaInAcres, IsDemo, CreatedAt, UpdatedAt)
VALUES (1, 'Demo Arecanut Farm', 'Ravi Kumar', 'Shivamogga', 'Shivamogga', 'Karnataka', 'Arecanut', '2', 1, '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z');
INSERT INTO Activities (Id, FarmId, ActivityType, ActivityDate, Description, CreatedAt)
VALUES (1, 1, 'General maintenance', '2026-09-25', 'Farm activity review', '2026-09-25T08:00:00Z');
INSERT INTO Reminders (Id, FarmId, Title, DueDate, IsCompleted, CreatedAt)
VALUES (1, 1, 'Review farm activity log', '2026-09-29', 0, '2026-09-25T08:00:00Z');
INSERT INTO Knowledge (Id, Title, CropType, Language, Content, SourceName) VALUES
('record-keeping', 'Basic arecanut farm record keeping', 'Arecanut', 'en', 'Keep a dated record of farm work, observations, quantities where known, and expenses. A consistent log can help you discuss your farm with an agricultural officer. This is general educational demo content and needs local review.', 'General educational demo content'),
('activity-log', 'How to maintain an activity log', 'General', 'en', 'Record what activity took place, when it happened, where relevant, what materials were used if known, and any cost. Keep observations separate from assumptions.', 'General educational demo content'),
('report-problem', 'Reporting a crop problem', 'General', 'en', 'When reporting a crop problem, note when it began, which plants or area are affected, recent observations, and any actions already taken. Photos and records can help a qualified local specialist assess it. Do not rely on a text assistant for diagnosis.', 'General educational demo content'),
('expenses', 'Documenting farm expenses', 'General', 'en', 'Record each expense with a date, a short description, amount, and any useful receipt reference. Review a consistent date range to understand recorded costs.', 'General educational demo content'),
('extension-guidance', 'Local agricultural guidance', 'General', 'en', 'Local agricultural officers and university extension specialists can consider local crop, soil, weather and current guidance. Seek qualified advice for serious crop problems or potentially harmful interventions.', 'General educational demo content');
");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE IF EXISTS ChatMessages; DROP TABLE IF EXISTS Reminders; DROP TABLE IF EXISTS Activities; DROP TABLE IF EXISTS Knowledge; DROP TABLE IF EXISTS Farms;");
}
