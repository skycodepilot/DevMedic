using SharpHook;
using System.Net.Http.Json;

string apiUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
Console.WriteLine($"--- DEV MEDIC AGENT ---");
Console.WriteLine($"Target API: {apiUrl}");
Console.WriteLine("Press CTRL+C to quit");

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

// STATE TRACKER: Assumed true so we don't miss the very first second
bool wasActive = true; 

while (await timer.WaitForNextTickAsync()) {
    int keys = Interlocked.Exchange(ref keyCount, 0);
    int mouse = Interlocked.Exchange(ref mouseCount, 0);
    int totalIntensity = keys + mouse;
    bool isIdle = totalIntensity == 0;

    // 1. VISUALIZATION (Always update local screen)
    // Added .PadRight(60) to wipe out "ghost text" from previous long lines
    string graph = DrawPulse(keys, mouse);
    Console.Write($"\rK: {keys:000} | M: {mouse:000} {graph}".PadRight(60));

    // 2. SMART LOGIC
    // We send data if:
    // A) We are currently doing something (!isIdle)
    // B) We were doing something last second, but stopped now (wasActive) -> This sends the "AWAY" packet once.
    if (!isIdle || wasActive) {
        try {
            var payload = new {
                Keys = keys,
                Mouse = mouse,
                Source = Environment.MachineName,
                Timestamp = DateTime.UtcNow
            };

            await httpClient.PostAsJsonAsync("/api/pulse", payload);
            
            // Update state: 
            // If we just sent real data, wasActive = true.
            // If we just sent the "Away" packet, wasActive = false (so we silence next time).
            wasActive = !isIdle; 
        }
        catch (Exception) {
            // If API fails, keep trying to send (don't change state to avoid losing data? 
            // For now, let's just keep silent to avoid console spam).
        }
    }
}

static string DrawPulse(int keys, int mouse) {
    int keyBars = Math.Min(keys / 2, 10);
    int mouseBars = Math.Min(mouse / 10, 10);
    return "[" + new string('#', keyBars) + new string('.', mouseBars) + "]";
}