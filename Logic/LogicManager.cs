using System;
namespace LiveSplit.CrankedUp {
    public class LogicManager {
        public bool ShouldSplit { get; private set; }
        public bool ShouldReset { get; private set; }
        public int CurrentSplit { get; private set; }
        public bool Running { get; private set; }
        public bool Paused { get; private set; }
        public float GameTime { get; private set; }
        public MemoryManager Memory { get; private set; }
        public SplitterSettings Settings { get; private set; }
        private bool lastBoolValue;
        private DateTime splitLate;

        public LogicManager(SplitterSettings settings) {
            Memory = new MemoryManager();
            Settings = settings;
            splitLate = DateTime.MaxValue;
        }

        public void Reset() {
            lastBoolValue = true;
            splitLate = DateTime.MaxValue;
            Paused = false;
            Running = false;
            CurrentSplit = 0;
            InitializeSplit();
            ShouldSplit = false;
            ShouldReset = false;
        }
        public void Decrement() {
            CurrentSplit--;
            splitLate = DateTime.MaxValue;
            InitializeSplit();
        }
        public void Increment() {
            Running = true;
            splitLate = DateTime.MaxValue;
            CurrentSplit++;
            InitializeSplit();
        }
        private void InitializeSplit() {
            if (CurrentSplit < Settings.Autosplits.Count) {
                bool temp = ShouldSplit;
                CheckSplit(Settings.Autosplits[CurrentSplit], true);
                ShouldSplit = temp;
            }
        }
        public bool IsHooked() {
            bool hooked = Memory.HookProcess();
            Paused = !hooked;
            ShouldSplit = false;
            ShouldReset = false;
            GameTime = -1;
            return hooked;
        }
        public void Update(int currentSplit) {
            if (currentSplit != CurrentSplit) {
                CurrentSplit = currentSplit;
                Running = CurrentSplit > 0;
                InitializeSplit();
            }

            if (CurrentSplit < Settings.Autosplits.Count) {
                CheckSplit(Settings.Autosplits[CurrentSplit], !Running);
                if (!Running) {
                    Paused = true;
                    if (ShouldSplit) {
                        Running = true;
                    }
                }

                if (ShouldSplit) {
                    Increment();
                }
            }
        }
        private void CheckSplit(Split split, bool updateValues) {
            ShouldSplit = false;
            Paused = Memory.IsLoading();

            if (!updateValues && Paused) {
                return;
            }

            switch (split.Type) {
                case SplitType.ManualSplit:
                    break;
                case SplitType.GameStart:
                    CheckGameStart();
                    break;
                case SplitType.Level:
                    CheckLevel(split);
                    break;
            }

            if (Running && Paused) {
                ShouldSplit = false;
            } else if (DateTime.Now > splitLate) {
                ShouldSplit = true;
                splitLate = DateTime.MaxValue;
            }
        }
        private void CheckGameStart() {
            bool inGame = Memory.IsInGame();
            ShouldSplit = inGame && !lastBoolValue && Memory.Level() == 8;
            lastBoolValue = inGame;
        }
        private void CheckLevel(Split split) {
            bool inGame = Memory.IsInGame();
            int levelID = Memory.Level();
            bool finished = Memory.FinishedLevel();

            SplitLevel level = Utility.GetEnumValue<SplitLevel>(split.Value);
            switch (level) {
                case SplitLevel.Any: ShouldSplit = inGame && finished && !lastBoolValue; break;
                case SplitLevel.City1: ShouldSplit = inGame && levelID == 8 && finished && !lastBoolValue; break;
                case SplitLevel.City2: ShouldSplit = inGame && levelID == 9 && finished && !lastBoolValue; break;
                case SplitLevel.City3: ShouldSplit = inGame && levelID == 10 && finished && !lastBoolValue; break;
                case SplitLevel.City4: ShouldSplit = inGame && levelID == 11 && finished && !lastBoolValue; break;
                case SplitLevel.City5: ShouldSplit = inGame && levelID == 12 && finished && !lastBoolValue; break;
                case SplitLevel.City6: ShouldSplit = inGame && levelID == 13 && finished && !lastBoolValue; break;
                case SplitLevel.City7: ShouldSplit = inGame && levelID == 14 && finished && !lastBoolValue; break;
                case SplitLevel.City8: ShouldSplit = inGame && levelID == 15 && finished && !lastBoolValue; break;
                case SplitLevel.City9: ShouldSplit = inGame && levelID == 16 && finished && !lastBoolValue; break;
                case SplitLevel.City10: ShouldSplit = inGame && levelID == 19 && finished && !lastBoolValue; break;
                case SplitLevel.City11: ShouldSplit = inGame && levelID == 20 && finished && !lastBoolValue; break;
                case SplitLevel.City12: ShouldSplit = inGame && levelID == 21 && finished && !lastBoolValue; break;
                case SplitLevel.City13: ShouldSplit = inGame && levelID == 22 && finished && !lastBoolValue; break;
                case SplitLevel.City14: ShouldSplit = inGame && levelID == 23 && finished && !lastBoolValue; break;
                case SplitLevel.City15: ShouldSplit = inGame && levelID == 24 && finished && !lastBoolValue; break;
                case SplitLevel.City16: ShouldSplit = inGame && levelID == 25 && finished && !lastBoolValue; break;
                case SplitLevel.City17: ShouldSplit = inGame && levelID == 26 && finished && !lastBoolValue; break;
                case SplitLevel.City18: ShouldSplit = inGame && levelID == 27 && finished && !lastBoolValue; break;
                case SplitLevel.City19: ShouldSplit = inGame && levelID == 28 && finished && !lastBoolValue; break;
                case SplitLevel.City20: ShouldSplit = inGame && levelID == 29 && finished && !lastBoolValue; break;
                case SplitLevel.Western1: ShouldSplit = inGame && levelID == 33 && finished && !lastBoolValue; break;
                case SplitLevel.Western2: ShouldSplit = inGame && levelID == 34 && finished && !lastBoolValue; break;
                case SplitLevel.Western3: ShouldSplit = inGame && levelID == 35 && finished && !lastBoolValue; break;
                case SplitLevel.Western4: ShouldSplit = inGame && levelID == 36 && finished && !lastBoolValue; break;
                case SplitLevel.Western5: ShouldSplit = inGame && levelID == 37 && finished && !lastBoolValue; break;
                case SplitLevel.Western6: ShouldSplit = inGame && levelID == 38 && finished && !lastBoolValue; break;
                case SplitLevel.Western7: ShouldSplit = inGame && levelID == 39 && finished && !lastBoolValue; break;
                case SplitLevel.Western8: ShouldSplit = inGame && levelID == 40 && finished && !lastBoolValue; break;
                case SplitLevel.Western9: ShouldSplit = inGame && levelID == 41 && finished && !lastBoolValue; break;
                case SplitLevel.Western10: ShouldSplit = inGame && levelID == 42 && finished && !lastBoolValue; break;
                case SplitLevel.Western11: ShouldSplit = inGame && levelID == 43 && finished && !lastBoolValue; break;
                case SplitLevel.Western12: ShouldSplit = inGame && levelID == 44 && finished && !lastBoolValue; break;
                case SplitLevel.Western13: ShouldSplit = inGame && levelID == 45 && finished && !lastBoolValue; break;
                case SplitLevel.Western14: ShouldSplit = inGame && levelID == 46 && finished && !lastBoolValue; break;
                case SplitLevel.Western15: ShouldSplit = inGame && levelID == 47 && finished && !lastBoolValue; break;
                case SplitLevel.Western16: ShouldSplit = inGame && levelID == 48 && finished && !lastBoolValue; break;
                case SplitLevel.Western17: ShouldSplit = inGame && levelID == 49 && finished && !lastBoolValue; break;
                case SplitLevel.Western18: ShouldSplit = inGame && levelID == 50 && finished && !lastBoolValue; break;
                case SplitLevel.Western19: ShouldSplit = inGame && levelID == 51 && finished && !lastBoolValue; break;
                case SplitLevel.Western20: ShouldSplit = inGame && levelID == 52 && finished && !lastBoolValue; break;
            }

            lastBoolValue = finished;
        }
    }
}