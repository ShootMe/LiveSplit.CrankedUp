using System.ComponentModel;
namespace LiveSplit.CrankedUp {
    public enum SplitType {
        [Description("Manual Split")]
        ManualSplit,
        [Description("Game Start")]
        GameStart,
        [Description("Level")]
        Level
    }
    public class Split {
        public string Name { get; set; }
        public SplitType Type { get; set; }
        public string Value { get; set; }

        public override string ToString() {
            return $"{Type}|{Value}";
        }
    }
}