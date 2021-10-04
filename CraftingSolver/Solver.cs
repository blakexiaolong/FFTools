using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CraftingSolver
{
    public class Solver
    {
        private readonly int maxTasks = 60;

        static Recipe neoIshgardian = new Recipe
        {
            Level = 480,
            Difficulty = 6178,
            Durability = 70,
            StartQuality = 0,
            MaxQuality = 36208,
            SuggestedCraftsmanship = 2480,
            SuggestedControl = 2195,
            Stars = 3
        };
        static Recipe exarchic = new Recipe
        {
            Level = 510,
            Difficulty = 8591,
            Durability = 70,
            StartQuality = 0,
            MaxQuality = 56662,
            SuggestedCraftsmanship = 2620,
            SuggestedControl = 2540,
            Stars = 4
        };

        static Crafter unbuffed = new Crafter
        {
            Class = "Armorer",
            Craftsmanship = 2737,
            Control = 2810,
            CP = 548,
            Level = 80,
            Specialist = false,
            Actions = Atlas.Actions.DependableActions
        };
        static Crafter chiliCrabCunning = new Crafter
        {
            Class = "Armorer",
            Craftsmanship = 2737,
            Control = 2880,
            CP = 636,
            Level = 80,
            Specialist = false,
            Actions = Atlas.Actions.DependableActions
        };

    private readonly Simulator sim = new Simulator
        {            
            Crafter = chiliCrabCunning,
            Recipe = exarchic,
            MaxTrickUses = 0,
            UseConditions = false,
            ReliabilityIndex = 1,
            MaxLength = 30,
        };

        public void Solve()
        {
            List<Action> solution;
            try
            {
                solution = new BruteForceSolver3().Run(sim, maxTasks);
            }
            catch (Exception e)
            {

            }
            finally
            {

            }
        }

        public interface ISolver
        {
            List<Action> Run(Simulator sim, int maxTasks);
        }

        public static Tuple<double, bool> ScoreState(Simulator sim, State state)
        {
            var violations = state.CheckViolations();
            if (state.Reliability != 1 || state.WastedActions > 0 || !violations.DurabilityOk || !violations.CpOk || !violations.TrickOk)
            {
                foreach (var kvp in state.WastedCounter)
                {
                    if (!wastedDict.ContainsKey(kvp.Key))
                    {
                        wastedDict.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        wastedDict[kvp.Key] += kvp.Value;
                    }
                }
                return new Tuple<double, bool>(-1, false);
            }
            bool perfectSolution = state.Quality >= sim.Recipe.MaxQuality && state.Progress >= sim.Recipe.Difficulty;
            double maxQuality = sim.Recipe.MaxQuality * 1.1;
            double progress = (state.Progress > sim.Recipe.Difficulty ? sim.Recipe.Difficulty : state.Progress) / sim.Recipe.Difficulty;
            double quality = (state.Quality > maxQuality ? maxQuality : state.Quality) / sim.Recipe.MaxQuality;
            return new Tuple<double, bool>((progress + quality) * 100, perfectSolution);

            progress = (state.Progress > sim.Recipe.Difficulty ? sim.Recipe.Difficulty : state.Progress);
            quality = (state.Quality > maxQuality ? maxQuality : state.Quality);
            return new Tuple<double, bool>(progress + quality + 2 * (sim.MaxLength - state.Step), perfectSolution);
        }

        static Dictionary<string, int> wastedDict = new Dictionary<string, int>();
        static Dictionary<string, int> auditDict = new Dictionary<string, int>
        {
            {  "AuditRepeatBuffs", 0 },
            {  "AuditCP", 0 },
            {  "AuditDurability", 0 },
            {  "AuditUnfocused", 0 },
            {  "AuditBBWithoutIQ", 0 },
            {  "AuditInnerQuiet", 0 },
        };

        public class BruteForceSolver3 : ISolver
        {
            #region Audits
            public delegate bool Audit(Simulator sim, List<Action> actions, List<Action> crafterActions);
            public static Audit[] audits = new Audit[]
            {
                AuditDurability,
                AuditUnfocused,
                AuditCP,
                AuditRepeatBuffs,
                AuditBBWithoutIQ,
                AuditInnerQuiet,
            };

            public static bool AuditBrand(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                bool brands = actions.Any(x => x.Equals(Atlas.Actions.BrandOfTheElements));
                switch (actions.Count(x => x.Equals(Atlas.Actions.NameOfTheElements)))
                {
                    case 0:
                        if (brands)
                        {
                            auditDict["AuditBrand"]++;
                            return false;
                        }
                        break;
                    case 1:
                        int nameIx = actions.IndexOf(Atlas.Actions.NameOfTheElements);
                        List<int> brandIxs = GetIndices(actions, Atlas.Actions.BrandOfTheElements);
                        if (brandIxs.Any(x => x - nameIx > 3 || x - nameIx < 1))
                        {
                            auditDict["AuditBrand"]++;
                            return false;
                        }
                        break;
                    default:
                        auditDict["AuditBrand"]++;
                        return false;
                }
                return true;
            }
            public static bool AuditQualityAfterByregots(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                int bbIx = actions.IndexOf(Atlas.Actions.ByregotsBlessing);
                if (bbIx >= 0)
                {
                    for (int i = bbIx + 1; i < actions.Count; i++)
                    {
                        if (Atlas.Actions.QualityActions.Contains(actions[i]))
                        {
                            auditDict["AuditQualityAfterByregots"]++;
                            return false;
                        }
                    }
                }
                return true;
            }

            public static bool AuditUnfocused(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                if (actions[actions.Count - 1].Equals(Atlas.Actions.FocusedSynthesis) || actions[actions.Count - 1].Equals(Atlas.Actions.FocusedTouch))
                {
                    if (actions.Count < 2)
                    {
                        auditDict["AuditUnfocused"]++;
                        return false;
                    }
                    if (actions[actions.Count - 2].Equals(Atlas.Actions.Observe))
                    {
                        return true;
                    }
                    else
                    {
                        auditDict["AuditUnfocused"]++;
                        return false;
                    }
                }
                return true;
            }
            public static bool AuditRepeatBuffs(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                if (actions.Count < 2 || !(actions[actions.Count - 1].Equals(actions[actions.Count - 2]) && Atlas.Actions.Buffs.Contains(actions[actions.Count - 1])))
                {
                    return true;
                }
                else
                {
                    auditDict["AuditRepeatBuffs"]++;
                    return false;
                }
            }
            public static bool AuditInnerQuiet(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                if (actions.Count(x => x.Equals(Atlas.Actions.InnerQuiet) || x.Equals(Atlas.Actions.Reflect)) < 2)
                {
                    return true;
                }
                auditDict["AuditInnerQuiet"]++;
                return false;
            }
            public static bool AuditCP(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                if (actions.Sum(x => x.CPCost) > sim.Crafter.CP)
                {
                    auditDict["AuditCP"]++;
                    return false;
                }
                return true;
            }
            public static bool AuditDurability(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                int dur = sim.Recipe.Durability;
                int manipTurns = 0;
                foreach (Action action in actions)
                {
                    if (dur <= 0)
                    {
                        auditDict["AuditDurability"]++;
                        return false;
                    }

                    if (action.Equals(Atlas.Actions.Groundwork) && dur < action.DurabilityCost) dur -= 10;
                    else if (action.DurabilityCost > 0) dur -= action.DurabilityCost;
                    else if (action.Equals(Atlas.Actions.MastersMend)) dur = Math.Min(sim.Recipe.Durability, dur + 30);

                    if (action.Equals(Atlas.Actions.Manipulation)) manipTurns = 8;
                    else if (manipTurns > 0 && dur > 0)
                    {
                        dur = Math.Min(sim.Recipe.Durability, dur + 5);
                        manipTurns--;
                    }
                }
                return true;
            }
            public static bool AuditBBWithoutIQ(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                if (actions.Last().Equals(Atlas.Actions.ByregotsBlessing))
                {
                    if (actions.Count(x => x.Equals(Atlas.Actions.ByregotsBlessing)) > 1)
                    {
                        auditDict["AuditBBWithoutIQ"]++;
                        return false;
                    }
                    int lastIqIx = Math.Max(actions.LastIndexOf(Atlas.Actions.InnerQuiet), actions.LastIndexOf(Atlas.Actions.Reflect));

                    if (lastIqIx < 0)
                    {
                        auditDict["AuditBBWithoutIQ"]++;
                        return false;
                    }
                    else if (lastIqIx == actions.Count - 2)
                    {
                        auditDict["AuditBBWithoutIQ"]++;
                        return false;
                    }
                    else
                    {
                        if (!actions.Skip(lastIqIx + 1).Take(actions.Count - 2).Any(x => Atlas.Actions.QualityActions.Contains(x)))
                        {
                            auditDict["AuditBBWithoutIQ"]++;
                            return false;
                        }
                    };
                }
                return true;
            }

            public static List<int> GetIndices(List<Action> actions, Action action)
            {
                List<int> res = new List<int>();
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i].Equals(action)) res.Add(i);
                }
                return res;
            }
            #endregion

            public static bool SolutionAudit(Simulator sim, List<Action> actions, List<Action> crafterActions) => audits.All(audit => audit(sim, actions, crafterActions));

            public List<Action> Run(Simulator sim, int maxTasks)
            {
                ulong attempts = 0, auditFail = 0, simFail = 0;
                int solutionsToKeep = 100000;
                double bestScore = 0;
                State startState = sim.Simulate(null, new State(), true, false, false);

                List<Tuple<double, List<Action>>> progress = new List<Tuple<double, List<Action>>>();
                List<Tuple<double, List<Action>>> solutions = new List<Tuple<double, List<Action>>>();

                List<Action> firstRoundActions = sim.Crafter.Actions.ToList();
                if (sim.Crafter.Level - sim.Recipe.Level < 10) firstRoundActions.Remove(Atlas.Actions.TrainedEye);
                Action[] temp = new Action[firstRoundActions.Count];
                firstRoundActions.CopyTo(temp);
                List<Action> otherActions = temp.ToList();
                otherActions.Remove(Atlas.Actions.MuscleMemory);
                otherActions.Remove(Atlas.Actions.TrainedEye);
                otherActions.Remove(Atlas.Actions.Reflect);

                Stopwatch sw = new Stopwatch();
                List<Tuple<int, long>> times = new List<Tuple<int, long>>();
                sw.Start();

                foreach (Action action in firstRoundActions)
                {
                    if (action == Atlas.Actions.DummyAction) continue;
                    List<Action> actions = new List<Action> { action };
                    if (!SolutionAudit(sim, actions, firstRoundActions))
                    {
                        auditFail++;
                        continue;
                    }
                    State finishState = sim.Simulate(actions, startState, false, false, false);
                    attempts++;
                    Tuple<double, bool> score = ScoreState(sim, finishState);
                    if (score.Item1 > 0)
                    {
                        if (finishState.CheckViolations().ProgressOk)
                        {
                            solutions.Add(new Tuple<double, List<Action>>(score.Item1, actions));
                            if (score.Item2) goto foundPerfect;
                        }
                        else
                        {
                            progress.Add(new Tuple<double, List<Action>>(score.Item1, actions));
                        }
                    }
                    else { simFail++; }
                }

                for (int i = 1; i < sim.MaxLength; i++)
                {
                    double worstScore = double.MaxValue;
                    List<Tuple<double, List<Action>>> nextProgress = new List<Tuple<double, List<Action>>>();
                    foreach (var solution in progress)
                    {
                        foreach (var action in otherActions)
                        {
                            List<Action> steps = solution.Item2.ToList();
                            steps.Add(action);
                            if (steps.Contains(Atlas.Actions.DummyAction)) continue;

                            if (!SolutionAudit(sim, steps, otherActions))
                            {
                                auditFail++;
                                continue;
                            }
                            State finishState = sim.Simulate(steps, startState, true, false, false);
                            Tuple<double, bool> score = ScoreState(sim, finishState);
                            attempts++;

                            if (score.Item1 > 0)
                            {
                                if (finishState.CheckViolations().ProgressOk)
                                {
                                    if (score.Item1 > bestScore)
                                    {
                                        bestScore = score.Item1;
                                        solutions.Add(new Tuple<double, List<Action>>(score.Item1, steps));
                                        if (score.Item2)
                                            goto foundPerfect;
                                    }
                                }
                                else if (nextProgress.Count() < solutionsToKeep)
                                {
                                    nextProgress.Add(new Tuple<double, List<Action>>(score.Item1, steps));
                                    if (score.Item1 < worstScore) worstScore = score.Item1;
                                }
                                else if (score.Item1 > worstScore)
                                {
                                    nextProgress.RemoveAll(x => x.Item1 == worstScore);
                                    nextProgress.Add(new Tuple<double, List<Action>>(score.Item1, steps));
                                    worstScore = nextProgress.Min(x => x.Item1);
                                }
                            }
                            else { simFail++; }
                        }
                    }
                    progress = nextProgress;
                    times.Add(new Tuple<int, long>(i, sw.ElapsedMilliseconds));
                }

            foundPerfect:
                bestScore = solutions.Max(y => y.Item1);
                var found = solutions.Where(x => x.Item1 == bestScore);
                var bestSolution = found.First().Item2;

                State final = sim.Simulate(bestSolution, startState, false, true, true);
                return bestSolution;
            }
        }
        public class BackFirstSolver
        {
            #region Audits
            public delegate bool Audit(List<Action> actions, List<Action> crafterActions);
            public static Audit[] audits = new Audit[]
            {
                AuditLastAction,
                AuditInnerQuiet,
                AuditRepeatBuffs,
                AuditFirstRound,
                AuditBrand
            };

            public static bool AuditLastAction(List<Action> actions, List<Action> crafterActions)
            {
                return Atlas.Actions.ProgressActions.Contains(actions.Last());
            }
            public static bool AuditInnerQuiet(List<Action> actions, List<Action> crafterActions)
            {
                if (actions.Any(x => x == Atlas.Actions.InnerQuiet || x == Atlas.Actions.Reflect))
                {
                    if (crafterActions.Contains(Atlas.Actions.ByregotsBlessing))
                    {
                        if (!actions.Any(x => x == Atlas.Actions.ByregotsBlessing)) return false;
                    }
                    else if (actions.Count(x => x == Atlas.Actions.InnerQuiet || x == Atlas.Actions.Reflect) > 1) return false;
                }
                return true;
            }
            public static bool AuditRepeatBuffs(List<Action> actions, List<Action> crafterActions)
            {
                return actions.Count < 2 || !(actions[0] == actions[1] && Atlas.Actions.Buffs.Contains(actions[0]));
            }
            public static bool AuditFirstRound(List<Action> actions, List<Action> crafterActions)
            {
                return !Atlas.Actions.FirstRoundActions.Any(x => actions.LastIndexOf(x) > 0);
            }
            public static bool AuditBrand(List<Action> actions, List<Action> crafterActions)
            {
                bool brands = actions.Any(x => x == Atlas.Actions.BrandOfTheElements);
                switch (actions.Count(x => x == Atlas.Actions.NameOfTheElements))
                {
                    case 0:
                        if (brands) return false;
                        break;
                    case 1:
                        int nameIx = actions.IndexOf(Atlas.Actions.NameOfTheElements);
                        List<int> brandIxs = GetIndices(actions, Atlas.Actions.BrandOfTheElements);
                        if (brandIxs.Any(x => x - nameIx > 3 || x - nameIx < 1)) return false;
                        break;
                    default:
                        return false;
                }
                return true;
            }
            #endregion

            public static bool SolutionAudit(List<Action> actions, List<Action> crafterActions) => audits.All(audit => audit(actions, crafterActions));

            public static List<int> GetIndices(List<Action> actions, Action action)
            {
                List<int> res = new List<int>();
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i] == action) res.Add(i);
                }
                return res;
            }

            public void Iterate(List<Action> actions, Dictionary<Action, int> actionsDict, List<Action> actionsList, int ix)
            {
                if (ix == -1)
                {
                    actions.Insert(0, actionsList[0]);
                    return;
                }

                int actionIx = actionsDict[actions[ix]] + 1;
                if (actionIx == actionsList.Count)
                    Iterate(actions, actionsDict, actionsList, ix - 1);
                actions[ix] = actionsList[actionIx % actionsList.Count];
            }
            public List<Action> Run(Simulator sim, int maxTasks)
            {
                ulong attempts = 0;
                double bestScore = 0;

                Stopwatch sw = new Stopwatch();
                List<Tuple<int, long>> times = new List<Tuple<int, long>>();

                List<Action> crafterActions = sim.Crafter.Actions.ToList();
                crafterActions.Remove(Atlas.Actions.DummyAction);
                if (sim.Crafter.Level - sim.Recipe.Level > 10) crafterActions.Remove(Atlas.Actions.TrainedEye);

                Dictionary<Action, int> actionsDict = new Dictionary<Action, int>();
                for (int i = 0; i < crafterActions.Count; i++) actionsDict[crafterActions[i]] = i;

                State startState = sim.Simulate(null, new State(), true, false, false);
                List<Tuple<double, List<Action>>> solutions = new List<Tuple<double, List<Action>>>();

                sw.Start();

                List<Action> actions = new List<Action>();
                while (!(actions.Count == sim.MaxLength && actions.All(x => x == crafterActions[crafterActions.Count - 1])))
                {
                    Iterate(actions, actionsDict, crafterActions, actions.Count - 1);
                    if (!SolutionAudit(actions, crafterActions)) continue;

                    attempts++;
                    State finalState = sim.Simulate(actions, startState, false, false, false);
                    if (!finalState.CheckViolations().ProgressOk) continue;

                    Tuple<double, bool> score = ScoreState(sim, finalState);
                    if (score.Item1 <= 0) continue;

                    if (score.Item1 > bestScore)
                    {
                        bestScore = score.Item1;
                        solutions.Add(new Tuple<double, List<Action>>(score.Item1, actions));
                        if (score.Item2) goto foundPerfect;
                    }
                }

            foundPerfect:
                bestScore = solutions.Max(y => y.Item1);
                var found = solutions.Where(x => x.Item1 == bestScore);
                var bestSolution = found.First().Item2;

                State final = sim.Simulate(bestSolution, startState, false, true, true);
                return bestSolution;
            }
        }

        public class GeneticSolver
        {
            const int MAX_GENERATION = 30;
            const int ELITE_PERCENTAGE = 15;
            const int MATE_PERCENTAGE = 30;
            const int POPULATION = 50000000;
            const int PROB_MUTATION = 10;

            Random rand = new Random();
            Action[] genes;
            int probParentX = 100 - (PROB_MUTATION / 2);

            public class ListComparer : IComparer<KeyValuePair<double, List<Action>>>
            {
                int IComparer<KeyValuePair<double, List<Action>>>.Compare(KeyValuePair<double, List<Action>> x, KeyValuePair<double, List<Action>> y) => (int)(x.Key - y.Key);
            }

            public static Tuple<double, bool> ScoreState(Simulator sim, State state)
            {
                if (!state.CheckViolations().DurabilityOk || !state.CheckViolations().CpOk || !state.CheckViolations().TrickOk || state.Reliability != 1)
                    return new Tuple<double, bool>(-1, false);
                bool perfectSolution = state.Quality >= sim.Recipe.MaxQuality && state.Progress >= sim.Recipe.Difficulty;
                double progress = (state.Progress > sim.Recipe.Difficulty ? sim.Recipe.Difficulty : state.Progress) / sim.Recipe.Difficulty;
                if (progress >= 1)
                {
                    double maxQuality = sim.Recipe.MaxQuality * 1.1;
                    double quality = (state.Quality > maxQuality ? maxQuality : state.Quality) / sim.Recipe.MaxQuality;
                    return new Tuple<double, bool>((progress + quality) * 100, perfectSolution);
                }
                else
                {
                    return new Tuple<double, bool>(progress * 100, perfectSolution);
                }
            }

            #region Audits
            public delegate bool Audit(List<Action> actions, List<Action> crafterActions);
            public static Audit[] audits = new Audit[]
            {
                //AuditLastAction,
                AuditInnerQuiet,
                AuditRepeatBuffs,
                AuditFirstRound,
                AuditFocused,
                AuditBrand
            };

            public static bool AuditLastAction(List<Action> actions, List<Action> crafterActions)
            {
                return Atlas.Actions.ProgressActions.Contains(actions.Last());
            }
            public static bool AuditInnerQuiet(List<Action> actions, List<Action> crafterActions)
            {
                if (actions.Any(x => x.Equals(Atlas.Actions.InnerQuiet) || x.Equals(Atlas.Actions.Reflect)))
                {
                    if (crafterActions.Contains(Atlas.Actions.ByregotsBlessing))
                    {
                        if (!actions.Any(x => x.Equals(Atlas.Actions.ByregotsBlessing))) return false;
                    }
                    else if (actions.Count(x => x.Equals(Atlas.Actions.InnerQuiet) || x.Equals(Atlas.Actions.Reflect)) > 1) return false;
                }
                return true;
            }
            public static bool AuditRepeatBuffs(List<Action> actions, List<Action> crafterActions)
            {
                return actions.Count < 2 || !(actions[0].Equals(actions[1]) && Atlas.Actions.Buffs.Contains(actions[0]));
            }
            public static bool AuditFirstRound(List<Action> actions, List<Action> crafterActions)
            {
                return !Atlas.Actions.FirstRoundActions.Any(x => actions.LastIndexOf(x) > 0);
            }
            public static bool AuditBrand(List<Action> actions, List<Action> crafterActions)
            {
                bool brands = actions.Any(x => x.Equals(Atlas.Actions.BrandOfTheElements));
                switch (actions.Count(x => x.Equals(Atlas.Actions.NameOfTheElements)))
                {
                    case 0:
                        if (brands) return false;
                        break;
                    case 1:
                        int nameIx = actions.IndexOf(Atlas.Actions.NameOfTheElements);
                        List<int> brandIxs = GetIndices(actions, Atlas.Actions.BrandOfTheElements);
                        if (brandIxs.Any(x => x - nameIx > 3 || x - nameIx < 1)) return false;
                        break;
                    default:
                        return false;
                }
                return true;
            }
            public static bool AuditFocused(List<Action> actions, List<Action> crafterActions)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    Action action = actions[i];
                    if (action.Equals(Atlas.Actions.FocusedSynthesis) || action.Equals(Atlas.Actions.FocusedTouch))
                    {
                        if (i - 1 < 0 || !actions[i - 1].Equals(Atlas.Actions.Observe)) return false;
                    }
                }
                return true;
            }
            #endregion

            public static bool SolutionAudit(List<Action> actions, List<Action> crafterActions) => audits.All(audit => audit(actions, crafterActions));

            public static List<int> GetIndices(List<Action> actions, Action action)
            {
                List<int> res = new List<int>();
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i] == action) res.Add(i);
                }
                return res;
            }

            public Action MutateGene() => genes[rand.Next(genes.Length - 1)];
            public List<Action> CreateChromosome(int length)
            {
                List<Action> actions = new List<Action>();
                for (int i = 0; i < length; i++)
                {
                    Action action = MutateGene();
                    actions.Add(action);
                    if (action.Equals(Atlas.Actions.DummyAction)) break;
                }
                return actions;
            }
            public List<Action> Mate(List<Action> parent1, List<Action> parent2)
            {
                List<Action> actions = new List<Action>();

                int length = rand.Next(1) == 0 ? parent1.Count : parent2.Count;
                for (int i = 0; i < length; i++)
                {
                    Action action;
                    int r = rand.Next(100);

                    if (r <= probParentX) action = parent1.Count-1 > i ? MutateGene() : parent1[i];
                    else if (r <= probParentX * 2) action = parent2.Count - 1 > i ? MutateGene() : parent2[i];
                    else action = MutateGene();

                    actions.Add(action);
                    if (action.Equals(Atlas.Actions.DummyAction)) break;
                }

                return actions;
            }

            public void BuildPopulation(List<List<Action>> population, int maxLength)
            {
                var genesList = genes.ToList();
                for (int i = 0; i < POPULATION; i++)
                {
                    var chromosome = CreateChromosome(maxLength);
                    if (!SolutionAudit(chromosome, genesList)) continue;
                    population.Add(chromosome);
                }
            }

            public List<Action> Run(Simulator sim, int maxTasks)
            {
                int generation = 1;

                genes = sim.Crafter.Actions;
                List<Action> genesList = genes.ToList();
                ListComparer comparer = new ListComparer();

                int maxLength = sim.MaxLength;
                State startState = sim.Simulate(null, new State(), true, false, false);
                List<List<Action>> population = new List<List<Action>>();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                BuildPopulation(population, maxLength);

                while (true)
                {
                    if (population.Count == 0)
                        BuildPopulation(population, maxLength);
                    List<KeyValuePair<double, List<Action>>> scoredPopulation = new List<KeyValuePair<double, List<Action>>>();
                    foreach (List<Action> chromosome in population)
                    {
                        State state = sim.Simulate(chromosome, startState, false, false, false);
                        Tuple<double, bool> score = ScoreState(sim, state);
                        if (score.Item2)
                            return chromosome;
                        scoredPopulation.Add(new KeyValuePair<double, List<Action>>(score.Item1, chromosome));
                    }

                    scoredPopulation.Sort(comparer);
                    if (generation == MAX_GENERATION)
                        return scoredPopulation.Last().Value;
                    scoredPopulation.RemoveAll(x => x.Key == -1);
                    if (scoredPopulation.Count == 0)
                    {
                        generation++;
                        continue;
                    }
                    else
                    {

                    }

                    var best = scoredPopulation.Last();
                    List<List<Action>> newGeneration = new List<List<Action>>();

                    // take top percent of population
                    int populationSize = scoredPopulation.Count;
                    int eliteCount = (int)Math.Ceiling(populationSize * ((double)ELITE_PERCENTAGE / 100));
                    newGeneration.AddRange(scoredPopulation.Skip(populationSize - eliteCount).Select(x => x.Value));

                    // mate next percent of population
                    int mateCount = (int)Math.Ceiling(populationSize * ((double)MATE_PERCENTAGE / 100));
                    List<List<Action>> matingPool = scoredPopulation.Skip(populationSize - mateCount).Select(x => x.Value).ToList();
                    for (int i = 0; i < POPULATION - eliteCount; i++)
                    {
                        List<Action> parent1 = matingPool[rand.Next(mateCount - 1)];
                        List<Action> parent2 = matingPool[rand.Next(mateCount - 1)];
                        var chromosome = Mate(parent1, parent2);
                        if (!SolutionAudit(chromosome, genesList)) continue;
                        newGeneration.Add(chromosome);
                    }

                    population = newGeneration;
                    generation++;
                }
            }
        }
    }
}
