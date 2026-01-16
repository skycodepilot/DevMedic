using SharpHook;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

string apiUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
Console.WriteLine($"--- DEV MEDIC AGENT ---");
Console.WriteLine($"Target API: {apiUrl}");
Console.WriteLine("Press CTRL+C to quit");

// PRIVACY FEATURE: Anonymize the Machine Name
// We hash the name so the database never stores a machine name like "Jasons-Laptop"
// (i.e. it won't store easily-identifiable data)
string anonymousSourceId = GetAnonymousId();
Console.WriteLine($"Agent ID: {anonymousSourceId} (Anonymized)");

using var httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
var hook = new TaskPoolGlobalHook();

int keyCount = 0;
int mouseCount = 0;

hook.KeyPressed += (_, _) => Interlocked.Increment(ref keyCount);
hook.MouseMoved += (_, _) => Interlocked.Increment(ref mouseCount);
hook.MouseClicked += (_, _) => Interlocked.Increment(ref mouseCount);
hook.MouseWheel += (_, _) => Interlocked.Increment(ref mouseCount);

Task hookTask = hook.RunAsync();
Console.WriteLine("Hook started. Collecting data...");

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
bool wasActive = true; 

while (await timer.WaitForNextTickAsync()) {
    int keys = Interlocked.Exchange(ref keyCount, 0);
    int mouse = Interlocked.Exchange(ref mouseCount, 0);
    int totalIntensity = keys + mouse;
    bool isIdle = totalIntensity == 0;

    string graph = DrawPulse(keys, mouse);
    Console.Write($"\rK: {keys:000} | M: {mouse:000} {graph}".PadRight(60));

    if (!isIdle || wasActive) {
        try {
            var payload = new {
                Keys = keys,
                Mouse = mouse,
                Source = anonymousSourceId, // SENDING HASH INSTEAD OF NAME
                Timestamp = DateTime.UtcNow
            };

            await httpClient.PostAsJsonAsync("/api/pulse", payload);
            wasActive = !isIdle; 
        }
        catch (Exception) {
            // Silent fail
        }
    }
}

static string DrawPulse(int keys, int mouse) {
    int keyBars = Math.Min(keys / 2, 10);
    int mouseBars = Math.Min(mouse / 10, 10);
    return "[" + new string('#', keyBars) + new string('.', mouseBars) + "]";
}

// PRIVACY HELPER: One-way Hash
static string GetAnonymousId() {
    string machineName = Environment.MachineName;
    using var sha = SHA256.Create();
    byte[] textBytes = Encoding.UTF8.GetBytes(machineName);
    byte[] hashBytes = sha.ComputeHash(textBytes);
    
    // Take the first 8 characters of the hash for a clean ID
    return "Device-" + BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 8);
}