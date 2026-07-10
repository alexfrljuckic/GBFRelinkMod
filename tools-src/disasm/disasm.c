/* Minimal PE disassembler using Zydis. Usage: disasm <exe> <rva_hex> <len_dec> */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <Zydis/Zydis.h>

static unsigned char *g_buf; static long g_size;

static uint32_t rva_to_off(uint32_t rva) {
    uint32_t pe = *(uint32_t *)(g_buf + 0x3c);
    uint16_t nsec = *(uint16_t *)(g_buf + pe + 6);
    uint16_t optsz = *(uint16_t *)(g_buf + pe + 20);
    unsigned char *sec = g_buf + pe + 24 + optsz;
    for (int i = 0; i < nsec; i++) {
        unsigned char *s = sec + i * 40;
        uint32_t va = *(uint32_t *)(s + 12), vs = *(uint32_t *)(s + 8), raw = *(uint32_t *)(s + 20);
        if (rva >= va && rva < va + vs) return rva - va + raw;
    }
    return 0;
}

int main(int argc, char **argv) {
    if (argc < 4) { printf("usage: disasm <exe> <rva_hex> <len>\n"); return 1; }
    FILE *f = fopen(argv[1], "rb");
    if (!f) { printf("open fail\n"); return 1; }
    fseek(f, 0, SEEK_END); g_size = ftell(f); fseek(f, 0, SEEK_SET);
    g_buf = malloc(g_size); fread(g_buf, 1, g_size, f); fclose(f);

    uint32_t rva = (uint32_t)strtoul(argv[2], NULL, 16);
    int len = atoi(argv[3]);
    uint32_t off = rva_to_off(rva);
    uint64_t runtime = 0x140000000ULL + rva; /* image base + rva */

    ZyanUSize offset = 0;
    ZydisDisassembledInstruction insn;
    while (offset < (ZyanUSize)len) {
        if (ZYAN_SUCCESS(ZydisDisassembleIntel(ZYDIS_MACHINE_MODE_LONG_64,
                runtime + offset, g_buf + off + offset, len - offset, &insn))) {
            printf("+0x%02llx  %-40s\n", (unsigned long long)offset, insn.text);
            offset += insn.info.length;
        } else {
            printf("+0x%02llx  (bad)\n", (unsigned long long)offset);
            offset += 1;
        }
    }
    free(g_buf);
    return 0;
}
