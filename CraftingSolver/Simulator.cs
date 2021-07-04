using System;
using System.Linq;
using System.Collections.Generic;

namespace CraftingSolver
{
    public class Simulator
    {
        public Crafter Crafter { get; set; }
        public Recipe Recipe { get; set; }
        public int MaxTrickUses { get; set; }
        public bool UseConditions { get; set; }
        public double ReliabilityIndex { get; set; }
        public int MaxLength { get; set; }

        public State Simulate(Action[] individual, State startState, bool assumeSuccess, bool verbose, bool debug)
        {
            State s = startState.Clone();
            List<string> log = new List<string>();

            double pGood = ProbabilityGoodForSynth();
            double pExcellent = ProbabilityExcellentForSynth();
            bool ignoreConditionReq = !UseConditions;

            double ppGood = 0, ppExcellent = 0, ppPoor = 0, ppNormal = 1 - (ppGood + ppExcellent + ppPoor);

            if (individual == default || individual.Length == 0)
            {
                return NewStateFromSynth(ignoreConditionReq, ppExcellent, ppGood);
            }

            if (debug)
            {
                log.Add($"{"#":-2} {"Action":30} {"DUR":-5} {"CP":-5} {"EQUA":-5} {"EPRG":-8} {"IQ":-8} {"CTL":-5} {"QINC":-8} {"BPRG":-5} {"BQUA":-5} {"WAC":-5}");
                log.Add($"{s.Step:2} {"":30} {s.Durability:5} {s.CP:5} {s.Quality:5} {s.Progress:8} {0:8} {Crafter.Control:5} {0:8} {0:5} {0:5} {0:5}");
            }
            else if (verbose)
            {
                log.Add($"{"#":-2} {"Action":30} {"DUR":-5} {"CP":-5} {"EQUA":-8} {"EPROG":-8} {"IQ":-5}");
                log.Add($"{s.Step:2} {"":30} {s.Durability:-5} {s.CP:-5} {s.Quality:-8} {s.Progress:-8} {0:5}");
            }

            foreach (Action action in individual)
            {
                s.Step++;
                SimulatorStep r = ApplyModifiers(s, action);

                double successProbability = assumeSuccess ? 1 : r.SuccessProbability;

                double progressGain = r.BProgressGain;
                if (progressGain > 0)
                {
                    s.Reliability *= successProbability;
                }
                progressGain = successProbability * Math.Floor(progressGain);

                double condQualityIncreaseMultiplier = 1;
                if (!ignoreConditionReq)
                {
                    condQualityIncreaseMultiplier *= ppNormal + 1.5 * ppGood * Math.Pow((1 - ppGood * 2) / 2, MaxTrickUses) + 4 * ppExcellent + 0.5 * ppPoor;
                }
                double qualityGain = condQualityIncreaseMultiplier * r.BQualityGain;
                qualityGain = successProbability * Math.Floor(qualityGain);

                if (s.Action == Atlas.Actions.DummyAction && action != Atlas.Actions.DummyAction)
                {
                    s.WastedActions++;
                }

                if ((s.Progress >= Recipe.Difficulty || s.Durability <= 0 || s.CP < 0) && action != Atlas.Actions.DummyAction)
                {
                    s.WastedActions++;
                }
                else
                {
                    s.UpdateState(action, progressGain, qualityGain, r.DurabilityCost, r.CPCost, successProbability);
                    if (ignoreConditionReq)
                    {
                        ppPoor = ppExcellent;
                        ppGood = pGood * ppNormal;
                        ppExcellent = pExcellent * ppNormal;
                        ppNormal = 1 - (ppGood + ppExcellent + ppPoor);
                    }
                }

                double iq = 0;
                if (s.CountUps.Any(x => x.Action == Atlas.Actions.InnerQuiet))
                {
                    iq = s.CountUps.First(x => x.Action == Atlas.Actions.InnerQuiet).Turns;
                }

                if (debug)
                {
                    log.Add($"{s.Step:2} {action.Name:30} {s.Durability:5} {s.CP:5} {s.Quality:5} {s.Progress:8} {iq:8} {r.Control:5} {qualityGain:8} {Math.Floor(r.BProgressGain):5} {Math.Floor(r.BQualityGain):5} {s.WastedActions:5}");
                }
                else if (verbose)
                {
                    log.Add($"{s.Step:2} {action.Name:30} {s.Durability:-5} {s.CP:-5} {s.Quality:-8} {s.Progress:-8} {iq:5}");
                }

                s.Action = action;
            }

            StateViolations check = s.CheckViolations();
            if (debug || verbose)
            {
                log.Add($"Progress Check: {check.ProgressOk}, Durability Check: {check.DurabilityOk}, CP Check: {check.CpOk}, Tricks Check: {check.TrickOk}, Reliability Check: {check.ReliabilityOk}, Wasted Actions: {s.WastedActions}");
            }

            s.Action = individual[individual.Length - 1];
            return s;
        }

        public State NewStateFromSynth(bool ignoreConditions, double ppExcellent, double ppGood)
        {
            return new State
            {
                Simulator = this,
                Step = 0,
                LastStep = 0,
                Action = null,
                Durability = Recipe.Durability,
                CP = Crafter.CP,
                BonusMaxCP = 0,
                Quality = Recipe.StartQuality,
                Progress = 0,
                WastedActions = 0,
                TrickUses = 0,
                NameOfElementUses = 0,
                Reliability = 1,
                Indefinites = new List<Effect>(),
                CountDowns = new List<Effect>(),
                CountUps = new List<Effect>(),
                Condition = new SimulatorCondition
                {
                    ConditionQuality = ConditionQuality.Normal,
                    IgnoreConditions = ignoreConditions,
                    PPExcellent = ppExcellent,
                    PPGood = ppGood
                }
            };
        }
        public SimulatorStep ApplyModifiers(State state, Action action)
        {
            int control = Crafter.Control;
            int cpCost = action.CPCost;
            int effectiveCrafterLevel = GetEffectiveCrafterLevel();
            int levelDifference = effectiveCrafterLevel - Recipe.Level;
            int originalLevelDifference = effectiveCrafterLevel - Recipe.Level;
            int pureLevelDifference = Crafter.Level - Recipe.Level;

            if (state.CountUps.Any(x => x.Action == Atlas.Actions.InnerQuiet))
            {
                control += Convert.ToInt32(0.2 * state.CountUps.First(x => x.Action == Atlas.Actions.InnerQuiet).Turns * Crafter.Control);
            }

            double progressIncreaseMultiplier = CalcProgressMultiplier(state, action);
            if (action == Atlas.Actions.MuscleMemory && state.Step != 1)
            {
                state.WastedActions++;
                progressIncreaseMultiplier = 0;
                cpCost = 0;
            }
            double bProgressGain = CalculateBaseProgressIncrease(levelDifference, Crafter.Craftsmanship) * action.ProgressIncreaseMultiplier * progressIncreaseMultiplier;
            if (action == Atlas.Actions.FlawlessSynthesis)
            {
                bProgressGain = 40;
            }
            double qualityIncreaseMultiplier = CalcQualityMultiplier(state, action);
            double bQualityGain = CalculateBaseQualityIncrease(levelDifference, control) * action.QualityIncreaseMultiplier * qualityIncreaseMultiplier;

            // effects modifying quality gain directly
            if (action == Atlas.Actions.TrainedEye)
            {
                if (state.Step == 1 && pureLevelDifference >= 10)
                {
                    bQualityGain = Recipe.MaxQuality;
                }
                else
                {
                    state.WastedActions++;
                    bQualityGain = 0;
                    cpCost = 0;
                }
            }

            // can only use Precise Touch if Good or Excellent
            if (action == Atlas.Actions.PreciseTouch)
            {
                if (state.Condition.CheckGoodOrExcellent())
                {
                    bQualityGain *= state.Condition.PGoodOrExcellent();
                }
                else
                {
                    state.WastedActions += 1;
                    bQualityGain = 0;
                    cpCost = 0;
                }
            }

            if (action == Atlas.Actions.Reflect && state.Step != 1)
            {
                state.WastedActions += 1;
                control = 0;
                bQualityGain = 0;
                cpCost = 0;
            }

            // Effects modifying durability cost
            double durabilityCost = action.DurabilityCost;
            if (state.CountDowns.Any(x => x.Action == Atlas.Actions.WasteNot) || state.CountDowns.Any(x => x.Action == Atlas.Actions.WasteNot2))
            {
                if (action == Atlas.Actions.PrudentTouch)
                {
                    bQualityGain = 0;
                }
                else
                {
                    durabilityCost *= 0.5;
                }
            }

            return new SimulatorStep
            {
                Craftsmanship = Crafter.Craftsmanship,
                Control = control,
                EffectiveCrafterLevel = effectiveCrafterLevel,
                EffectiveRecipeLevel = Recipe.Level,
                LevelDifference = levelDifference,
                SuccessProbability = CalcSuccessProbability(state, action),
                QualityIncreaseMultiplier = qualityIncreaseMultiplier,
                BProgressGain = bProgressGain,
                BQualityGain = bQualityGain,
                DurabilityCost = durabilityCost,
                CPCost = cpCost
            };
        }
        
        private double CalcSuccessProbability(State state, Action action)
        {
            double successProbability = action.SuccessProbability;
            if ((action == Atlas.Actions.FocusedSynthesis || action == Atlas.Actions.FocusedTouch) && state.Action == Atlas.Actions.Observe)
            {
                successProbability = 1;
            }
            return Math.Min(successProbability, 1);
        }
        private double CalcProgressMultiplier(State state, Action action)
        {
            double progressIncreaseMultiplier = 1;
            if(action.ProgressIncreaseMultiplier > 0 && state.CountDowns.Any(x => x.Action == Atlas.Actions.MuscleMemory))
            {
                progressIncreaseMultiplier++;
                state.CountDowns.RemoveAll(x => x.Action == Atlas.Actions.MuscleMemory);
            }
            if (action == Atlas.Actions.BrandOfTheElements && state.CountDowns.Any(x => x.Action == Atlas.Actions.NameOfTheElements))
            {
                progressIncreaseMultiplier += state.CalcNameOfElementsBonus();
            }
            if (state.CountDowns.Any(x => x.Action == Atlas.Actions.Veneration))
            {
                progressIncreaseMultiplier += 0.5;
            }
            if (action == Atlas.Actions.Groundwork && state.Durability < Atlas.Actions.Groundwork.DurabilityCost)
            {
                progressIncreaseMultiplier *= 0.5;
            }
            return progressIncreaseMultiplier;
        }
        private double CalcQualityMultiplier(State state, Action action)
        {
            double qualityIncreaseMultiplier = 1;
            if (state.CountDowns.Any(x => x.Action == Atlas.Actions.GreatStrides) && qualityIncreaseMultiplier > 0)
            {
                qualityIncreaseMultiplier += 1;
            }
            if (state.CountDowns.Any(x => x.Action == Atlas.Actions.Innovation))
            {
                qualityIncreaseMultiplier += 0.5;
            }
            if (action == Atlas.Actions.ByregotsBlessing)
            {
                if (state.CountUps.Any(x => x.Action == Atlas.Actions.InnerQuiet) && state.CountUps.First(x => x.Action == Atlas.Actions.InnerQuiet).Turns >= 1)
                {
                    qualityIncreaseMultiplier *= 1 + (0.2 * state.CountUps.First(x => x.Action == Atlas.Actions.InnerQuiet).Turns);
                }
                else
                {
                    qualityIncreaseMultiplier = 0;
                }
            }
            return qualityIncreaseMultiplier;
        }

        public double CalculateBaseProgressIncrease(int levelDifference, int craftsmanship)
        {
            double levelDifferenceFactor = GetLevelDifferenceFactor("craftsmanship", levelDifference);
            return Math.Floor((levelDifferenceFactor * (0.21 * craftsmanship + 2) * (10000 + craftsmanship)) / (10000 + Recipe.SuggestedCraftsmanship));
        }
        public double CalculateBaseQualityIncrease(int levelDifference, int control)
        {
            double levelDifferenceFactor = GetLevelDifferenceFactor("control", levelDifference);
            return Math.Floor((levelDifferenceFactor * (0.35 * control + 35) * (10000 + control)) / (10000 + Recipe.SuggestedControl));
        }

        public double ProbabilityGoodForSynth()
        {
            int recipeLevel = Recipe.Level;
            bool qualityAssurance = Crafter.Level >= 63;
            if (recipeLevel >= 300)
            {
                return qualityAssurance ? 0.11 : 0.10;
            }
            else if (recipeLevel >= 276)
            {
                return qualityAssurance ? 0.17 : 0.15;
            }
            else if (recipeLevel >= 255)
            {
                return qualityAssurance ? 0.22 : 0.20;
            }
            else if (recipeLevel >= 150)
            {
                return qualityAssurance ? 0.11 : 0.10;
            }
            else if (recipeLevel >= 136)
            {
                return qualityAssurance ? 0.17 : 0.15;
            }
            else
            {
                return qualityAssurance ? 0.27 : 0.25;
            }
        }
        public double ProbabilityExcellentForSynth()
        {
            var recipeLevel = Recipe.Level;
            if (recipeLevel >= 300)
            {
                return 0.01;
            }
            else if (recipeLevel >= 255)
            {
                return 0.02;
            }
            else if (recipeLevel >= 150)
            {
                return 0.01;
            }
            else
            {
                return 0.02;
            }
        }

        public int GetEffectiveCrafterLevel()
        {
            int effectiveCrafterLevel = Crafter.Level;
            if (Atlas.LevelTable.ContainsKey(Crafter.Level))
            {
                effectiveCrafterLevel = Atlas.LevelTable[Crafter.Level];
            }
            return effectiveCrafterLevel;
        }
        public double GetLevelDifferenceFactor(string kind, int levelDifference)
        {
            if (levelDifference < -30)
            {
                levelDifference = -30;
            }
            else if (levelDifference > 20)
            {
                levelDifference = 20;
            }

            Dictionary<int, double> factors = Atlas.LevelDifferenceFactors[kind];
            if (factors == default)
            {
                throw new Exception("Unrecognized Level Difference Factor Type");
            }

            return factors[levelDifference];
        }
        
        public double QualityFromHqPercent(double hqPercent)
        {
            return -5.6604E-6 * Math.Pow(hqPercent, 4) + 0.0015369705 * Math.Pow(hqPercent, 3) - 0.1426469573 * Math.Pow(hqPercent, 2) + 5.6122722959 * hqPercent - 5.5950384565;
        }
        public double HqPercentFromQuality(double qualityPercent)
        {
            var hqPercent = 1;
            if (qualityPercent == 0)
            {
                hqPercent = 1;
            }
            else if (qualityPercent >= 100)
            {
                hqPercent = 100;
            }
            else
            {
                while (QualityFromHqPercent(hqPercent) < qualityPercent && hqPercent < 100)
                {
                    hqPercent += 1;
                }
            }
            return hqPercent;
        }
    }
}
