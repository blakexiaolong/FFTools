using System.Diagnostics;

namespace CraftingSolver
{
    public class Action : System.IEquatable<Action>
    {
        public string ShortName { get; set; }
        public string Name { get; set; }
        public int DurabilityCost { get; set; }
        public int CPCost { get; set; }
        public double SuccessProbability { get; set; }
        public double QualityIncreaseMultiplier { get; set; }
        public double ProgressIncreaseMultiplier { get; set; }
        public string ActionType { get; set; }
        public int ActiveTurns { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public bool OnGood { get; set; }
        public bool OnExcellent { get; set; }
        public bool OnPoor { get; set; }

        public bool Equals(Action other) => ShortName == other.ShortName;
        public override string ToString() => $"{Name}";
    }
}
