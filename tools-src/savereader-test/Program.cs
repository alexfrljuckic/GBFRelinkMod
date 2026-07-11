using gbfr.transmarvel.overhaul;

// Dump a save's per-sigil combo sets in a canonical text format for
// differential comparison against the JS reference (scripts/).
// Usage: savereader-test <SaveData.dat path>
if (args.Length < 1) { Console.Error.WriteLine("usage: savereader-test <save.dat>"); return 1; }

try
{
    var r = SaveReader.Read(args[0]);
    Console.WriteLine($"sigils={r.SigilCount} aligned={r.AlignmentMatched}");
    foreach (var kv in r.CombosBySigil.OrderBy(kv => kv.Key))
    {
        var secs = kv.Value.OrderBy(x => x).Select(x => x.ToString("X8"));
        Console.WriteLine($"{kv.Key:X8} copies={r.CopiesBySigil[kv.Key]} secs={string.Join(",", secs)}");
    }
    return 0;
}
catch (Exception e)
{
    Console.Error.WriteLine($"FAIL: {e.Message}");
    return 2;
}
