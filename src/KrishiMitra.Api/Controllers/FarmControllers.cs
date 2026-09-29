using System.ComponentModel.DataAnnotations;
using KrishiMitra.Core;
using KrishiMitra.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KrishiMitra.Api.Controllers;

internal static class ApiDtos
{
    public static FarmResponse Farm(Farm x) => new(x.Id, x.FarmName, x.FarmerName, x.Location, x.District, x.State, x.CropType, x.AreaInAcres, x.PlantingDate, x.Notes, x.IsDemo, x.CreatedAt, x.UpdatedAt);
    public static ActivityResponse Activity(FarmActivity x) => new(x.Id, x.FarmId, x.ActivityType, x.ActivityDate, x.Description, x.Quantity, x.Unit, x.ExpenseAmount, x.Notes, x.IsDemo, x.CreatedAt);
    public static ReminderResponse Reminder(FarmReminder x) => new(x.Id, x.FarmId, x.Title, x.Description, x.DueDate, x.IsCompleted, x.IsDemo, x.CreatedAt);
}

[ApiController, Route("api/farms")]
public sealed class FarmsController(FarmDbContext db) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await db.Farms.AsNoTracking().OrderBy(x => x.Id).Select(x => new FarmResponse(x.Id, x.FarmName, x.FarmerName, x.Location, x.District, x.State, x.CropType, x.AreaInAcres, x.PlantingDate, x.Notes, x.IsDemo, x.CreatedAt, x.UpdatedAt)).ToListAsync(ct));
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id, CancellationToken ct) { var farm = await db.Farms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct); return farm is null ? NotFound() : Ok(ApiDtos.Farm(farm)); }
    [HttpPost] public async Task<IActionResult> Create(FarmInput input, CancellationToken ct)
    {
        if (!ValidFarm(input, out var error)) return BadRequest(new { error });
        var farm = NewFarm(input); db.Farms.Add(farm); await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(Get), new { id = farm.Id }, ApiDtos.Farm(farm));
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, FarmInput input, CancellationToken ct)
    {
        if (!ValidFarm(input, out var error)) return BadRequest(new { error });
        var farm = await db.Farms.FindAsync([id], ct); if (farm is null) return NotFound();
        Copy(input, farm); farm.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); return Ok(ApiDtos.Farm(farm));
    }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) { var farm = await db.Farms.FindAsync([id], ct); if (farm is null) return NotFound(); db.Farms.Remove(farm); await db.SaveChangesAsync(ct); return NoContent(); }
    private static bool ValidFarm(FarmInput x, out string error) { error = "Check required fields, field lengths, planting date and positive farm area."; return !string.IsNullOrWhiteSpace(x.FarmName) && x.FarmName.Trim().Length <= 100 && !string.IsNullOrWhiteSpace(x.FarmerName) && x.FarmerName.Trim().Length <= 100 && !string.IsNullOrWhiteSpace(x.Location) && x.Location.Trim().Length <= 100 && (x.District?.Length ?? 0) <= 100 && (x.State?.Length ?? 0) <= 100 && !string.IsNullOrWhiteSpace(x.CropType) && x.CropType.Trim().Length <= 80 && x.AreaInAcres > 0 && x.AreaInAcres <= 100000 && (x.PlantingDate is null || (x.PlantingDate.Value.Year >= 1900 && x.PlantingDate <= DateOnly.FromDateTime(DateTime.Today))) && (x.Notes?.Length ?? 0) <= 1000; }
    private static Farm NewFarm(FarmInput x) { var f = new Farm(); Copy(x, f); return f; }
    private static void Copy(FarmInput x, Farm f) { f.FarmName = x.FarmName.Trim(); f.FarmerName = x.FarmerName.Trim(); f.Location = x.Location.Trim(); f.District = x.District?.Trim() ?? ""; f.State = x.State?.Trim() ?? "Karnataka"; f.CropType = x.CropType.Trim(); f.AreaInAcres = x.AreaInAcres; f.PlantingDate = x.PlantingDate; f.Notes = x.Notes; }
}

[ApiController, Route("api/farms/{farmId:int}/activities")]
public sealed class ActivitiesController(FarmDbContext db) : ControllerBase
{
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int farmId, int id, CancellationToken ct) { var item = await db.Activities.AsNoTracking().FirstOrDefaultAsync(x => x.FarmId == farmId && x.Id == id, ct); return item is null ? NotFound() : Ok(ApiDtos.Activity(item)); }
    [HttpGet] public async Task<IActionResult> List(int farmId, [FromQuery] string? type, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        if (!await db.Farms.AnyAsync(x => x.Id == farmId, ct)) return NotFound();
        if (from.HasValue && to.HasValue && from > to) return BadRequest(new { error = "The start date must be on or before the end date." });
        var q = db.Activities.AsNoTracking().Where(x => x.FarmId == farmId);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(x => x.ActivityType == type);
        if (from.HasValue) q = q.Where(x => x.ActivityDate >= from);
        if (to.HasValue) q = q.Where(x => x.ActivityDate <= to);
        var items = await q.OrderByDescending(x => x.ActivityDate).ToListAsync(ct);
        // SQLite stores EF Core decimal values as TEXT; aggregate them in .NET for correct decimal arithmetic.
        return Ok(new { items = items.Select(ApiDtos.Activity).ToArray(), totalExpenses = items.Sum(x => x.ExpenseAmount ?? 0) });
    }
    [HttpPost] public async Task<IActionResult> Create(int farmId, ActivityInput input, CancellationToken ct)
    {
        if (!await db.Farms.AnyAsync(x => x.Id == farmId, ct)) return NotFound();
        if (string.IsNullOrWhiteSpace(input.ActivityType) || input.ActivityType.Length > 50 || string.IsNullOrWhiteSpace(input.Description) || input.Description.Length > 500 || input.ExpenseAmount < 0 || input.ExpenseAmount > 100000000 || input.Quantity < 0 || input.Quantity > 1000000 || input.Unit?.Length > 30 || (input.Notes?.Length ?? 0) > 1000 || input.ActivityDate == default || input.ActivityDate > DateOnly.FromDateTime(DateTime.Today)) return BadRequest(new { error = "Check activity fields, date and non-negative quantity and expense." });
        var item = new FarmActivity { FarmId = farmId, ActivityType = input.ActivityType.Trim(), ActivityDate = input.ActivityDate, Description = input.Description.Trim(), Quantity = input.Quantity, Unit = input.Unit, ExpenseAmount = input.ExpenseAmount, Notes = input.Notes };
        db.Activities.Add(item); await db.SaveChangesAsync(ct); return Created($"/api/farms/{farmId}/activities/{item.Id}", ApiDtos.Activity(item));
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int farmId, int id, ActivityInput input, CancellationToken ct) { var x = await db.Activities.FirstOrDefaultAsync(x => x.Id == id && x.FarmId == farmId, ct); if (x is null) return NotFound(); if (input.ExpenseAmount < 0 || input.ExpenseAmount > 100000000 || input.Quantity < 0 || input.Quantity > 1000000 || string.IsNullOrWhiteSpace(input.ActivityType) || input.ActivityType.Length > 50 || string.IsNullOrWhiteSpace(input.Description) || input.Description.Length > 500 || input.Unit?.Length > 30 || (input.Notes?.Length ?? 0) > 1000 || input.ActivityDate == default || input.ActivityDate > DateOnly.FromDateTime(DateTime.Today)) return BadRequest(new { error = "Activity values must be valid and non-negative." }); x.ActivityType = input.ActivityType; x.ActivityDate = input.ActivityDate; x.Description = input.Description; x.Quantity = input.Quantity; x.Unit = input.Unit; x.ExpenseAmount = input.ExpenseAmount; x.Notes = input.Notes; await db.SaveChangesAsync(ct); return Ok(ApiDtos.Activity(x)); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int farmId, int id, CancellationToken ct) { var x = await db.Activities.FirstOrDefaultAsync(x => x.Id == id && x.FarmId == farmId, ct); if (x is null) return NotFound(); db.Activities.Remove(x); await db.SaveChangesAsync(ct); return NoContent(); }
}

[ApiController, Route("api/farms/{farmId:int}/reminders")]
public sealed class RemindersController(FarmDbContext db) : ControllerBase
{
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int farmId, int id, CancellationToken ct) { var item = await db.Reminders.AsNoTracking().FirstOrDefaultAsync(x => x.FarmId == farmId && x.Id == id, ct); return item is null ? NotFound() : Ok(ApiDtos.Reminder(item)); }
    [HttpGet] public async Task<IActionResult> List(int farmId, [FromQuery] string? status, CancellationToken ct) { if (!await db.Farms.AnyAsync(x => x.Id == farmId, ct)) return NotFound(); var q = db.Reminders.AsNoTracking().Where(x => x.FarmId == farmId); if (status == "completed") q = q.Where(x => x.IsCompleted); if (status == "upcoming") q = q.Where(x => !x.IsCompleted && x.DueDate >= DateOnly.FromDateTime(DateTime.Today)); if (status == "overdue") q = q.Where(x => !x.IsCompleted && x.DueDate < DateOnly.FromDateTime(DateTime.Today)); return Ok((await q.OrderBy(x => x.IsCompleted).ThenBy(x => x.DueDate).ToListAsync(ct)).Select(ApiDtos.Reminder)); }
    [HttpPost] public async Task<IActionResult> Create(int farmId, ReminderInput input, CancellationToken ct) { if (!await db.Farms.AnyAsync(x => x.Id == farmId, ct)) return NotFound(); if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Length > 120 || (input.Description?.Length ?? 0) > 500 || input.DueDate == default) return BadRequest(new { error = "Reminder title, due date and description lengths must be valid." }); var x = new FarmReminder { FarmId = farmId, Title = input.Title.Trim(), Description = input.Description, DueDate = input.DueDate, IsCompleted = input.IsCompleted }; db.Reminders.Add(x); await db.SaveChangesAsync(ct); return Created($"/api/farms/{farmId}/reminders/{x.Id}", ApiDtos.Reminder(x)); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int farmId, int id, ReminderInput input, CancellationToken ct) { var x = await db.Reminders.FirstOrDefaultAsync(x => x.Id == id && x.FarmId == farmId, ct); if (x is null) return NotFound(); if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Length > 120 || (input.Description?.Length ?? 0) > 500 || input.DueDate == default) return BadRequest(new { error = "Reminder title, due date and description lengths must be valid." }); x.Title = input.Title; x.Description = input.Description; x.DueDate = input.DueDate; x.IsCompleted = input.IsCompleted; await db.SaveChangesAsync(ct); return Ok(ApiDtos.Reminder(x)); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int farmId, int id, CancellationToken ct) { var x = await db.Reminders.FirstOrDefaultAsync(x => x.Id == id && x.FarmId == farmId, ct); if (x is null) return NotFound(); db.Reminders.Remove(x); await db.SaveChangesAsync(ct); return NoContent(); }
}

[ApiController, Route("api/knowledge")]
public sealed class KnowledgeController(FarmDbContext db) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await db.Knowledge.AsNoTracking().OrderBy(x => x.Title).ToListAsync(ct));
    [HttpGet("{id}")] public async Task<IActionResult> Get(string id, CancellationToken ct) { var x = await db.Knowledge.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct); return x is null ? NotFound() : Ok(x); }
}

[ApiController, Route("api/dashboard/{farmId:int}")]
public sealed class DashboardController(FarmDbContext db) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Get(int farmId, CancellationToken ct) { var farm = await db.Farms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == farmId, ct); if (farm is null) return NotFound(); var today = DateOnly.FromDateTime(DateTime.Today); var upcoming = await db.Reminders.AsNoTracking().Where(x => x.FarmId == farmId && !x.IsCompleted && x.DueDate >= today).OrderBy(x => x.DueDate).Take(5).ToListAsync(ct); var overdue = await db.Reminders.AsNoTracking().Where(x => x.FarmId == farmId && !x.IsCompleted && x.DueDate < today).OrderBy(x => x.DueDate).ToListAsync(ct); var activities = await db.Activities.AsNoTracking().Where(x => x.FarmId == farmId).OrderByDescending(x => x.ActivityDate).Take(5).ToListAsync(ct); return Ok(new DashboardResponse(ApiDtos.Farm(farm), upcoming.Select(ApiDtos.Reminder).ToArray(), overdue.Select(ApiDtos.Reminder).ToArray(), activities.Select(ApiDtos.Activity).ToArray())); }
}

[ApiController, Route("api/ai/chat")]
public sealed class AiController(FarmDbContext db, IKnowledgeRetriever retrieval, IAiAssistant assistant) : ControllerBase
{
    [HttpPost] public async Task<IActionResult> Chat(ChatInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Question) || input.Question.Length > 2000) return BadRequest(new { error = "Enter a question up to 2,000 characters." });
        var farm = await db.Farms.OrderBy(x => x.Id).FirstOrDefaultAsync(ct); if (farm is null) return Conflict(new { error = "Create a farm profile before asking a question." });
        var docs = await retrieval.RetrieveAsync(input.Question, ct);
        await db.ChatMessages.AddAsync(new ChatMessage { Role = "user", Content = input.Question.Trim() }, ct);
        var (answer, demo) = await assistant.AskAsync(input.Question.Trim(), farm, docs, ct);
        await db.ChatMessages.AddAsync(new ChatMessage { Role = "assistant", Content = answer }, ct);
        var old = await db.ChatMessages.OrderByDescending(x => x.CreatedAt).Skip(100).ToListAsync(ct); db.ChatMessages.RemoveRange(old); await db.SaveChangesAsync(ct);
        return Ok(new ChatOutput(answer, demo, docs));
    }
    [HttpGet] public async Task<IActionResult> History(CancellationToken ct) => Ok(await db.ChatMessages.AsNoTracking().OrderBy(x => x.CreatedAt).Take(100).Select(x => new { x.Id, x.Role, x.Content, x.CreatedAt }).ToListAsync(ct));
    [HttpDelete] public async Task<IActionResult> Clear(CancellationToken ct) { db.ChatMessages.RemoveRange(await db.ChatMessages.ToListAsync(ct)); await db.SaveChangesAsync(ct); return NoContent(); }
}
