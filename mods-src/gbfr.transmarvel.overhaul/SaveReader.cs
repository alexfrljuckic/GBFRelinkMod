namespace gbfr.transmarvel.overhaul;

/// <summary>
/// Read-only parser for the GBFR save's sigil inventory + per-copy trait
/// assignments (format: docs/21-save-sigil-inventory.md). Self-contained (no
/// Reloaded dependencies) so it can be differentially tested against the
/// battle-tested JS implementation in scripts/.
///
/// Every step re-derives offsets from the file and self-validates via the
/// innate-trait redundancy (a sigil's innate SKILL number equals its GEEN
/// number); on any mismatch it throws rather than guessing.
/// </summary>
public static class SaveReader
{
    public sealed class Result
    {
        public string SavePath = "";
        public int SigilCount;
        public int AlignmentMatched;
        /// <summary>sigil GEEN hash -> set of secondary-trait SKILL hashes owned (empty-secondary copies excluded)</summary>
        public Dictionary<uint, HashSet<uint>> CombosBySigil = new();
        /// <summary>sigil GEEN hash -> owned copy count</summary>
        public Dictionary<uint, int> CopiesBySigil = new();
    }

    private const uint EmptyHash = 0x887AE0B0;

    /// <summary>Parses the newest SaveData&lt;N&gt;.dat in <paramref name="saveDir"/>. Throws on any inconsistency.</summary>
    public static Result ReadNewest(string saveDir)
    {
        var newest = Directory.GetFiles(saveDir, "SaveData*.dat")
            .Where(p => System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(p), @"^SaveData\d+\.dat$"))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault() ?? throw new FileNotFoundException($"no SaveData<N>.dat in {saveDir}");
        return Read(newest);
    }

    public static Result Read(string savePath)
    {
        byte[] buf;
        using (var fs = new FileStream(savePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            buf = new byte[fs.Length];
            int off = 0;
            while (off < buf.Length)
            {
                int n = fs.Read(buf, off, buf.Length - off);
                if (n <= 0) throw new IOException("short read");
                off += n;
            }
        }

        // reverse hash maps: GEEN_000_00..999_99 and SKILL_000_00..999_10 -> id number
        var geenRev = new Dictionary<uint, int>();
        var skillRev = new Dictionary<uint, int>();
        for (int a = 0; a <= 999; a++)
        {
            for (int b = 0; b <= 99; b++)
                geenRev[Hash($"GEEN_{a:D3}_{b:D2}")] = a;
            for (int b = 0; b <= 10; b++)
                skillRev[Hash($"SKILL_{a:D3}_{b:D2}")] = a;
        }

        var sig = Locate(buf, geenRev, "sigil inventory");
        var skl = Locate(buf, skillRev, "trait pool");

        // trait pool: seq = instanceN*100 + slot (0 innate, 1 secondary)
        var slot0 = new Dictionary<long, uint>();
        var slot1 = new Dictionary<long, uint>();
        for (int i = 0; i < skl.Count; i++)
        {
            uint seq = ReadU32(buf, skl.Start + 24 * i + 4);
            uint hash = ReadU32(buf, skl.Start + 24 * i + 16);
            long n = seq / 100;
            int s = (int)(seq % 100);
            if (s == 0) slot0[n] = hash;
            else if (s == 1) slot1[n] = hash;
        }

        // filled sigil slots
        var filled = new List<(int j, int geenNum, uint hash)>();
        for (int j = 0; j < sig.Count; j++)
        {
            uint hash = ReadU32(buf, sig.Start + 24 * j + 16);
            if (geenRev.TryGetValue(hash, out int n))
                filled.Add((j, n, hash));
        }
        if (filled.Count == 0) throw new InvalidDataException("sigil inventory is empty");

        // consensus alignment C: sigil slot j <-> trait instance (C - j)
        var votes = new Dictionary<long, int>();
        foreach (var (j, n, _) in filled)
            foreach (var (inst, h) in slot0)
                if (skillRev.TryGetValue(h, out int sn) && sn == n)
                {
                    votes.TryGetValue(inst + j, out int v);
                    votes[inst + j] = v + 1;
                }
        long C = votes.OrderByDescending(kv => kv.Value).First().Key;
        int match = 0;
        foreach (var (j, n, _) in filled)
            if (slot0.TryGetValue(C - j, out uint h) && skillRev.TryGetValue(h, out int sn) && sn == n)
                match++;
        if (match < filled.Count * 0.99)
            throw new InvalidDataException($"alignment cross-check failed ({match}/{filled.Count}) — save layout changed?");

        var result = new Result { SavePath = savePath, SigilCount = filled.Count, AlignmentMatched = match };
        foreach (var (j, _, hash) in filled)
        {
            result.CopiesBySigil.TryGetValue(hash, out int c);
            result.CopiesBySigil[hash] = c + 1;
            if (!result.CombosBySigil.TryGetValue(hash, out var set))
                result.CombosBySigil[hash] = set = new HashSet<uint>();
            if (slot1.TryGetValue(C - j, out uint sec) && sec != EmptyHash && sec != 0)
                set.Add(sec);
        }
        return result;
    }

    private readonly record struct Run(int Start, int Count);

    // find a contiguous array of 24-byte records [type, seq, 4, 1, hash, tag]
    // whose hashes match `rev`, requiring duplicate hashes (rejects the
    // all-unique catalog list) and >= 1000 records
    private static Run Locate(byte[] buf, Dictionary<uint, int> rev, string what)
    {
        for (int off = 0; off + 24 <= buf.Length; off += 4)
        {
            if (ReadU32(buf, off + 8) != 4 || ReadU32(buf, off + 12) != 1) continue;
            if (!rev.ContainsKey(ReadU32(buf, off + 16))) continue;
            uint type = ReadU32(buf, off);
            if (type == 0 || type > 0xffff) continue;
            int s = off, e = off;
            while (s - 24 >= 0 && ReadU32(buf, s - 24) == type) s -= 24;
            while (e + 48 <= buf.Length && ReadU32(buf, e + 24) == type) e += 24;
            int count = (e - s) / 24 + 1;
            if (count >= 1000)
            {
                var seen = new HashSet<uint>();
                int dup = 0;
                for (int i = 0; i < count; i++)
                {
                    uint h = ReadU32(buf, s + 24 * i + 16);
                    if (h == EmptyHash) continue;
                    if (!seen.Add(h)) dup++;
                }
                if (dup > 10) return new Run(s, count);
            }
            off = e;
        }
        throw new InvalidDataException($"could not find the {what} array — save layout changed?");
    }

    private static uint ReadU32(byte[] b, int off) =>
        (uint)(b[off] | (b[off + 1] << 8) | (b[off + 2] << 16) | (b[off + 3] << 24));

    // GBFR's custom XXHash32 (docs/15) — duplicated here to keep this file
    // dependency-free for the differential test harness.
    public static uint Hash(string input)
    {
        const uint P1 = 0x9E3779B1, P2 = 0x85EBCA77, P3 = 0xC2B2AE3D, P4 = 0x27D4EB2F, P5 = 0x165667B1;
        static uint Rotl(uint x, int r) => (x << r) | (x >>> (32 - r));
        static uint Round(uint acc, uint val) => Rotl(acc + val * P2, 13) * P1;

        byte[] data = System.Text.Encoding.ASCII.GetBytes(input);
        int len = data.Length, i = 0;
        uint h = 0x178A54A4;
        if (len >= 16)
        {
            uint v1 = 0x2557311B, v2 = 0x871FB76A, v3 = 0x0133ECF3, v4 = 0x62FC7342;
            do
            {
                v1 = Round(v1, ReadU32(data, i));
                v2 = Round(v2, ReadU32(data, i + 4));
                v3 = Round(v3, ReadU32(data, i + 8));
                v4 = Round(v4, ReadU32(data, i + 12));
                i += 16;
            } while (len - i > 16);
            h = Rotl(v1, 1) + Rotl(v2, 7) + Rotl(v3, 12) + Rotl(v4, 18);
        }
        h += (uint)len;
        for (; len - i >= 4; i += 4)
            h = Rotl(h + ReadU32(data, i) * P3, 17) * P4;
        for (; i < len; i++)
            h = Rotl(h + data[i] * P5, 11) * P1;
        h = (h ^ (h >> 15)) * P2;
        h = (h ^ (h >> 13)) * P3;
        return h ^ (h >> 16);
    }
}
