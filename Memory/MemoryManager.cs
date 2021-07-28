using System;
using System.Diagnostics;
namespace LiveSplit.CrankedUp {
    public partial class MemoryManager {
        private static ProgramPointer GameManager = new ProgramPointer("GameAssembly.dll",
            new FindPointerSignature(PointerVersion.All, AutoDeref.Single, "488B05????????488B80B8000000488B18488B0D????????F6812F01000002740E83B9E0000000007505E8????????4533C033D2488BCBE8????????84C07577488B05", 0x3, 0x0));
        private static ProgramPointer MetricsUpdater = new ProgramPointer("GameAssembly.dll",
            new FindPointerSignature(PointerVersion.All, AutoDeref.Single, "33C9E8????????488B05????????488B88B8000000488B49304885C9740733D2E8????????488B0D????????F6812F01000002740E83B9E0000000007505", 0xa, 0x0));
        public static PointerVersion Version { get; set; } = PointerVersion.All;
        public Process Program { get; set; }
        public bool IsHooked { get; set; }
        public DateTime LastHooked { get; set; }

        public MemoryManager() {
            LastHooked = DateTime.MinValue;
        }
        public string GamePointers() {
            return string.Concat(
                $"GM: {(ulong)GameManager.GetPointer(Program):X} = {(ulong)GameManager.Read<IntPtr>(Program, 0xb8, 0x0):X} ",
                $"MU: {(ulong)MetricsUpdater.GetPointer(Program):X} = {(ulong)MetricsUpdater.Read<IntPtr>(Program, 0xb8, 0x0):X} "
            );
        }
        public bool IsLoading() {
            return false;
        }
        public bool IsInGame() {
            return GameManager.Read<bool>(Program, 0xb8, 0x0, 0x17c);
        }
        public bool FinishedLevel() {
            return GameManager.Read<bool>(Program, 0xb8, 0x0, 0x90, 0x83);
        }
        public bool IsCutscene() {
            return GameManager.Read<bool>(Program, 0xb8, 0x0, 0x17d);
        }
        public int Level() {
            //MetricsUpdater.LevelIdx
            return MetricsUpdater.Read<int>(Program, 0xb8, 0x40);
        }
        public string LevelName() {
            //MetricsUpdater.LevelName
            return MetricsUpdater.Read(Program, 0xb8, 0x38, 0x0);
        }
        public bool HookProcess() {
            IsHooked = Program != null && !Program.HasExited;
            if (!IsHooked && DateTime.Now > LastHooked.AddSeconds(1)) {
                LastHooked = DateTime.Now;

                Process[] processes = Process.GetProcessesByName("CrankedUp");
                Program = processes != null && processes.Length > 0 ? processes[0] : null;

                if (Program != null && !Program.HasExited) {
                    MemoryReader.Update64Bit(Program);
                    IsHooked = true;
                }
            }

            return IsHooked;
        }
        public void Dispose() {
            if (Program != null) {
                Program.Dispose();
            }
        }
    }
}