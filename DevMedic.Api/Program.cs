using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. REGISTER THE DATABASE (SQLite)
builder.Services.AddDbContext<PulseDbContext>(options => 
    options.UseSqlite("Data Source=devmedic.db"));

var app = builder.Build();

// 2. AUTO-MIGRATE ON STARTUP (Creates devmedic.db if missing)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PulseDbContext>();
    db.Database.EnsureCreated(); // Simple auto-setup for PoC
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. INJECT THE DB CONTEXT INTO THE ENDPOINT
app.MapPost("/api/pulse", async (PulseData input, PulseDbContext db) =>
{
    string activityState = ClassifyActivity(input.Keys, input.Mouse);

    // Create the DB Record
    var logEntry = new PulseLog 
    {
        Keys = input.Keys,
        Mouse = input.Mouse,
        Source = input.Source,
        Timestamp = input.Timestamp,
        Classification = activityState
    };

    // Save to DB
    db.Pulses.Add(logEntry);
    await db.SaveChangesAsync();

    Console.WriteLine($"[SAVED] {input.Source}: {activityState}");
    
    return Results.Ok(new { status = "Saved", id = logEntry.Id });
})
.WithName("ReceivePulse");

app.Run();

// LOGIC HELPERS
static string ClassifyActivity(int keys, int mouse)
{
    if (keys > 10 && keys > mouse) return "CODING 💻";
    if (mouse > 50 && mouse > keys * 2) return "BROWSING 🌐";
    if (keys > 0 || mouse > 0) return "IDLE/THINKING 🤔";
    return "AWAY 💤";
}

// Data Transfer Object (Input)
record PulseData(int Keys, int Mouse, string Source, DateTime Timestamp);