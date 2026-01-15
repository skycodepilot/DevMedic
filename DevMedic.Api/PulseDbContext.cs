using Microsoft.EntityFrameworkCore;

// This class represents the Database itself
public class PulseDbContext : DbContext
{
    public PulseDbContext(DbContextOptions<PulseDbContext> options) : base(options) { }

    // This represents the "Table" of pulses
    public DbSet<PulseLog> Pulses => Set<PulseLog>();
}

// We are upgrading our simple "record" to a full Database Entity with an ID
public class PulseLog
{
    public int Id { get; set; } // Primary Key
    public int Keys { get; set; }
    public int Mouse { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty; // Store "CODING", "BROWSING", etc.
    public DateTime Timestamp { get; set; }
}