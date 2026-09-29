using System.ComponentModel.DataAnnotations;

namespace KrishiMitra.Core;

public sealed class Farm
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string FarmName { get; set; } = "";
    [Required, MaxLength(100)] public string FarmerName { get; set; } = "";
    [Required, MaxLength(100)] public string Location { get; set; } = "";
    [MaxLength(100)] public string District { get; set; } = "";
    [MaxLength(100)] public string State { get; set; } = "Karnataka";
    [Required, MaxLength(80)] public string CropType { get; set; } = "Arecanut";
    [Range(0.01, 100000)] public decimal AreaInAcres { get; set; }
    public DateOnly? PlantingDate { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
    public bool IsDemo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<FarmActivity> Activities { get; set; } = [];
    public List<FarmReminder> Reminders { get; set; } = [];
}

public sealed class FarmActivity
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public Farm? Farm { get; set; }
    [Required, MaxLength(50)] public string ActivityType { get; set; } = "Other";
    public DateOnly ActivityDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Required, MaxLength(500)] public string Description { get; set; } = "";
    [Range(0, 1000000)] public decimal? Quantity { get; set; }
    [MaxLength(30)] public string? Unit { get; set; }
    [Range(0, 100000000)] public decimal? ExpenseAmount { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
    public bool IsDemo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class FarmReminder
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public Farm? Farm { get; set; }
    [Required, MaxLength(120)] public string Title { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
    public DateOnly DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDemo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class KnowledgeDocument
{
    [MaxLength(80)] public string Id { get; set; } = "";
    [Required, MaxLength(150)] public string Title { get; set; } = "";
    [MaxLength(80)] public string CropType { get; set; } = "General";
    [MaxLength(10)] public string Language { get; set; } = "en";
    [Required, MaxLength(4000)] public string Content { get; set; } = "";
    [MaxLength(150)] public string SourceName { get; set; } = "General educational demo content";
    [MaxLength(500)] public string? SourceUrl { get; set; }
    public DateOnly? LastReviewedAt { get; set; }
}

public sealed class ChatMessage
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string Role { get; set; } = "user";
    [Required, MaxLength(4000)] public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record FarmInput(string FarmName, string FarmerName, string Location, string District, string State, string CropType, decimal AreaInAcres, DateOnly? PlantingDate, string? Notes);
public record FarmResponse(int Id, string FarmName, string FarmerName, string Location, string District, string State, string CropType, decimal AreaInAcres, DateOnly? PlantingDate, string? Notes, bool IsDemo, DateTime CreatedAt, DateTime UpdatedAt);
public record ActivityInput(string ActivityType, DateOnly ActivityDate, string Description, decimal? Quantity, string? Unit, decimal? ExpenseAmount, string? Notes);
public record ActivityResponse(int Id, int FarmId, string ActivityType, DateOnly ActivityDate, string Description, decimal? Quantity, string? Unit, decimal? ExpenseAmount, string? Notes, bool IsDemo, DateTime CreatedAt);
public record ReminderInput(string Title, string? Description, DateOnly DueDate, bool IsCompleted = false);
public record ReminderResponse(int Id, int FarmId, string Title, string? Description, DateOnly DueDate, bool IsCompleted, bool IsDemo, DateTime CreatedAt);
public record DashboardResponse(FarmResponse Farm, IReadOnlyList<ReminderResponse> UpcomingReminders, IReadOnlyList<ReminderResponse> OverdueReminders, IReadOnlyList<ActivityResponse> RecentActivities);
public record ChatInput(string Question);
public record ChatOutput(string Answer, bool IsDemo, IReadOnlyList<KnowledgeDocument> Sources);

public interface IKnowledgeRetriever
{
    Task<IReadOnlyList<KnowledgeDocument>> RetrieveAsync(string question, CancellationToken cancellationToken = default);
}

public interface IAiAssistant
{
    Task<(string Answer, bool IsDemo)> AskAsync(string question, Farm farm, IReadOnlyList<KnowledgeDocument> documents, CancellationToken cancellationToken = default);
}
