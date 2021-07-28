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
        private bool lastInGame, lastFinished;
        private DateTime splitLate;

        public LogicManager(SplitterSettings settings) {
            Memory = new MemoryManager();
            Settings = settings;
            splitLate = DateTime.MaxValue;
        }

        public void Reset() {
            lastInGame = true;
            lastFinished = true;
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
            ShouldSplit = inGame && !lastInGame && Memory.Level() == (int)SplitLevel.City1;
            lastInGame = inGame;
        }
        private void CheckLevel(Split split) {
            bool inGame = Memory.IsInGame();
            int levelID = Memory.Level();
            bool finished = Memory.FinishedLevel();

            SplitLevel level = Utility.GetEnumValue<SplitLevel>(split.Value);
            if (CurrentSplit == 0) {
                switch (level) {
                    case SplitLevel.Any: ShouldSplit = inGame && !lastInGame; break;
                    default: ShouldSplit = inGame && !lastInGame && levelID == (int)level; break;
                }
            } else {
                switch (level) {
                    case SplitLevel.Any: ShouldSplit = inGame && finished && !lastFinished; break;
                    default: ShouldSplit = inGame && levelID == (int)level && finished && !lastFinished; break;
                }
            }

            lastInGame = inGame;
            lastFinished = finished;
        }
    }
}