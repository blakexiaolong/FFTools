using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingSolver
{
    internal static class LightSimulator
    {
        public static State Simulate(Simulator simulator, List<Action> actions, State startState, bool useDurability = true)
        {
            State s = startState.Clone();
            //Dictionary<Effect, int> countdowns = new Dictionary<Effect, int>();
            //Dictionary<Effect, int> indefinites = new Dictionary<Effect, int>();
            //foreach (Action action in actions)
            //{
            //    s.Step++;
            //    double baseProgressGain, baseQualityGain;
            //    //apply modifiers
            //    int cpCost = useDurability ? action.CPCost : 0;
            //    double progressIncreaseMultiplier = 1;
            //    Effect muMe=countdowns()

            //    double progressGain = Math.Floor(baseProgressGain);
            //    double qualityGain = Math.Floor(baseQualityGain);

            //    // update state
            //    s.Action = action;
            //}

            return s;
        }
    }
}
