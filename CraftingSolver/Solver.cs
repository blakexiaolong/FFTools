﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace CraftingSolver
{
    public class Solver
    {
        private readonly int maxTasks = 20;

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
            Stars = 4,
        };

        static Recipe newNeoIshgardian = new Recipe
        {
            Level = 480,
            Difficulty = 2800,
            Durability = 70,
            StartQuality = 0,
            MaxQuality = 8500,
            SuggestedCraftsmanship = 2480,
            SuggestedControl = 2195,
            Stars = 3,
            ProgressDivider = 110,
            QualityDivider = 90,
            ProgressModifier = 0.8,
            QualityModifier = 0.7
        };
        static Recipe newExarchic = new Recipe
        {
            Level = 510,
            Difficulty = 3600,
            Durability = 70,
            StartQuality = 0,
            MaxQuality = 9500,
            SuggestedCraftsmanship = 2620,
            SuggestedControl = 2540,
            Stars = 4,
            ProgressDivider = 110,
            QualityDivider = 90,
            ProgressModifier = 0.8,
            QualityModifier = 0.7
        };
        static Recipe classicalMilpreves = new Recipe
        {
            Level = 580,
            Difficulty = 3900,
            StartQuality = 0,
            MaxQuality = 10920,
            Durability = 70,
            SuggestedCraftsmanship = 3180,
            SuggestedControl = 3080,
            Stars = 2,
            ProgressDivider = 130,
            QualityDivider = 115,
            ProgressModifier = 0.8,
            QualityModifier = 0.7
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
        static Crafter newUnbuffed = new Crafter
        {
            Class = "Armorer",
            Craftsmanship = 3286,
            Control = 3396,
            CP = 565,
            Level = 90,
            Specialist = false,
            Actions = Atlas.Actions.DependableActions
        };
        static Crafter newBuffed = new Crafter
        {
            Class = "Armorer",
            Craftsmanship = 3324,
            Control = 3462,
            CP = 675,
            Level = 90,
            Specialist = false,
            Actions = Atlas.Actions.DependableActions
        };

        private readonly Simulator sim = new Simulator
        {
            Crafter = newBuffed,
            Recipe = classicalMilpreves,
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
                sim.Initialize();
                Atlas.Actions.UpgradeActionsByLevel(sim.Crafter.Level);
                State finishState = sim.Simulate(new List<Action>
                {
                    Atlas.Actions.MuscleMemory,
                    Atlas.Actions.Manipulation,
                    Atlas.Actions.Veneration,
                    Atlas.Actions.WasteNot2,
                    Atlas.Actions.Groundwork,
                    Atlas.Actions.Groundwork,
                    Atlas.Actions.PreparatoryTouch,
                    Atlas.Actions.Innovation,
                    Atlas.Actions.PreparatoryTouch,
                    Atlas.Actions.PreparatoryTouch,
                    Atlas.Actions.PreparatoryTouch,
                    Atlas.Actions.AdvancedTouch,
                    Atlas.Actions.GreatStrides,
                    Atlas.Actions.Innovation,
                    Atlas.Actions.PreparatoryTouch,
                    Atlas.Actions.TrainedFinesse,
                    Atlas.Actions.GreatStrides,
                    Atlas.Actions.ByregotsBlessing,
                    Atlas.Actions.CarefulSynthesis
                }, sim.Simulate(null, new State()));
                var score = ScoreState(sim, finishState);
                solution = new JABOASolver().Run(sim, maxTasks);
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

        public static List<Action> GetFirstRoundActions(Simulator sim)
        {
            List<Action> firstRoundActions = sim.Crafter.Actions.ToList();
            if (sim.Crafter.Level - sim.Recipe.Level < 10) firstRoundActions.Remove(Atlas.Actions.TrainedEye);
            return firstRoundActions;
        }
        public static List<Action> GetOtherActions(List<Action> firstRoundActions)
        {
            Action[] temp = new Action[firstRoundActions.Count];
            firstRoundActions.CopyTo(temp);
            List<Action> otherActions = temp.ToList();
            otherActions.Remove(Atlas.Actions.MuscleMemory);
            otherActions.Remove(Atlas.Actions.TrainedEye);
            otherActions.Remove(Atlas.Actions.Reflect);
            return otherActions;
        }

        public static Tuple<double, bool> ScoreState(Simulator sim, State state, bool ignoreProgress = false)
        {
            if (!state.Success)
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
            }
            bool perfectSolution = state.Quality >= sim.Recipe.MaxQuality && state.Progress >= sim.Recipe.Difficulty;
            double progress = ignoreProgress ? 0 : (state.Progress > sim.Recipe.Difficulty ? sim.Recipe.Difficulty : state.Progress) / sim.Recipe.Difficulty;
            double maxQuality = sim.Recipe.MaxQuality * 1.1;
            double quality = (state.Quality > maxQuality ? maxQuality : state.Quality) / sim.Recipe.MaxQuality;
            double cp = state.CP / sim.Crafter.CP;
            double dur = state.Durability / sim.Recipe.Durability;
            return new Tuple<double, bool>((progress + quality) * 100 + (cp + dur) * 10, perfectSolution);
        }

        static ulong attempts = 0, chainAuditFail = 0, auditFail = 0, simFail = 0, lookaheadSkip = 0, repeated = 0;
        static Dictionary<string, int> wastedDict = new Dictionary<string, int>();
        static Dictionary<string, int> auditDict = new Dictionary<string, int>
        {
            { "AuditRepeatBuffs", 0 },
            { "AuditCP", 0 },
            { "AuditDurability", 0 },
            { "AuditUnfocused", 0 },
            { "AuditBBWithoutIQ", 0 },
            { "AuditInnerQuiet", 0 },
            { "AuditBrand", 0 },
            { "AuditQualityAfterByregots", 0 },
            { "AuditLastAction", 0 },
            { "AuditByregots", 0 },
        };

        public class BetterBruteForcer : ISolver
        {
            #region Objects
            class ActionNode
            {
                public Action Action { get; set; }
                public ActionNode Parent { get; set; }
            }
            #endregion

            #region Audits
            public delegate bool Audit(Simulator sim, List<Action> actions, List<Action> crafterActions);
            public static Audit[] chainAudits = new Audit[]
            {
                AuditDurability,
                AuditUnfocused,
                AuditCP,
                AuditRepeatBuffs,
                AuditBBWithoutIQ,
                AuditInnerQuiet,
                AuditQualityAfterByregots
            };
            public static Audit[] solutionAudits = new Audit[]
            {
                AuditLastAction,
                AuditByregots
            };

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

            public static bool AuditLastAction(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                if (Atlas.Actions.ProgressActions.Contains(actions.Last()))
                {
                    return true;
                }
                else
                {
                    auditDict["AuditLastAction"]++;
                    return false;
                }
            }
            public static bool AuditByregots(Simulator sim, List<Action> actions, List<Action> crafterActions)
            {
                if (actions.Any(x => x == Atlas.Actions.InnerQuiet || x == Atlas.Actions.Reflect))
                {
                    if (crafterActions.Contains(Atlas.Actions.ByregotsBlessing))
                    {
                        if (!actions.Any(x => x == Atlas.Actions.ByregotsBlessing))
                        {
                            auditDict["AuditByregots"]++;
                            return false;
                        }
                    }
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
            public static bool ChainAudit(Simulator sim, List<Action> actions, List<Action> crafterActions) => chainAudits.All(audit => audit(sim, actions, crafterActions));
            public static bool SolutionAudit(Simulator sim, List<Action> actions, List<Action> crafterActions) => solutionAudits.All(audit => audit(sim, actions, crafterActions));
            #endregion

            private bool CheckSteps(Simulator sim, State startState, ActionNode node, List<Action> steps, List<Action> actions, ref double bestScore, List<ActionNode> leaves, List<Tuple<double, List<Action>>> solutions)
            {
                if (ChainAudit(sim, steps, actions))
                {
                    if (SolutionAudit(sim, steps, actions))
                    {
                        attempts++;
                        State finalState = sim.Simulate(steps, startState);
                        Tuple<double, bool> score = ScoreState(sim, finalState);

                        if (score.Item1 > 0)
                        {
                            if (finalState.CheckViolations().ProgressOk)
                            {
                                if (score.Item1 > bestScore)
                                {
                                    bestScore = score.Item1;
                                    solutions.Add(new Tuple<double, List<Action>>(score.Item1, steps));
                                    if (score.Item2) return true;
                                }
                            }
                            else
                            {
                                leaves.Add(node);
                            }
                        }
                        else { simFail++; }
                    }
                    else
                    {
                        auditFail++;
                        leaves.Add(node);
                    }
                }
                else { chainAuditFail++; }

                return false;
            }

            private List<Action> GetActions(ActionNode leaf)
            {
                List<Action> actions = new List<Action>();
                ActionNode branch = leaf;
                while (branch.Parent != default)
                {
                    actions.Insert(0, branch.Action);
                    branch = branch.Parent;
                }
                return actions;
            }

            public List<Action> Run(Simulator sim, int maxTasks)
            {
                State startState = sim.Simulate(null, new State());
                List<ActionNode> leaves = new List<ActionNode>();
                List<Tuple<double, List<Action>>> solutions = new List<Tuple<double, List<Action>>>();

                List<Action> firstRoundActions = sim.Crafter.Actions.ToList();
                if (sim.Crafter.Level - sim.Recipe.Level < 10) firstRoundActions.Remove(Atlas.Actions.TrainedEye);
                List<Action> otherActions = firstRoundActions.ToList();
                otherActions.Remove(Atlas.Actions.MuscleMemory);
                otherActions.Remove(Atlas.Actions.TrainedEye);
                otherActions.Remove(Atlas.Actions.Reflect);

                double bestScore = double.MinValue;

                Stopwatch sw = new Stopwatch();
                List<Tuple<int, long>> times = new List<Tuple<int, long>>();
                List<Tuple<int, long>> leafCounter = new List<Tuple<int, long>>();
                sw.Start();

                ActionNode head = new ActionNode() { Action = Atlas.Actions.DummyAction, Parent = null };
                foreach (Action action in firstRoundActions)
                {
                    ActionNode node = new ActionNode { Action = action, Parent = head };
                    if (CheckSteps(sim, startState, node, GetActions(node), firstRoundActions, ref bestScore, leaves, solutions))
                    {
                        goto foundPerfect;
                    }
                }
                leafCounter.Add(new Tuple<int, long>(1, leaves.Count));

                int lastStep = 1;
                while (leaves.Any())
                {
                    ActionNode parent = leaves.First();
                    leaves.Remove(parent);
                    int step = GetActions(parent).Count;

                    foreach (Action action in otherActions)
                    {
                        ActionNode node = new ActionNode { Action = action, Parent = parent };
                        if (CheckSteps(sim, startState, node, GetActions(node), firstRoundActions, ref bestScore, leaves, solutions))
                            goto foundPerfect;
                    }

                    if (step > lastStep)
                    {
                        lastStep = step;
                        times.Add(new Tuple<int, long>(step, sw.ElapsedMilliseconds));
                        leafCounter.Add(new Tuple<int, long>(step, leaves.Count));
                    }
                }

            foundPerfect:
                bestScore = solutions.Max(y => y.Item1);
                var found = solutions.Where(x => x.Item1 == bestScore);
                var bestSolution = found.First().Item2;

                State final = sim.Simulate(bestSolution, startState, debug: true);
                return bestSolution;
            }
        }
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
                int solutionsToKeep = 50;
                double bestScore = 0;
                State startState = sim.Simulate(null, new State());

                List<double> worstScores = new List<double>();
                Dictionary<int, List<Tuple<double, List<Action>>>> progress = new Dictionary<int, List<Tuple<double, List<Action>>>>();
                List<Tuple<double, List<Action>>> solutions = new List<Tuple<double, List<Action>>>();

                for (int i = 0; i < sim.MaxLength; i++)
                {
                    worstScores.Add(double.MaxValue);
                    progress.Add(i, new List<Tuple<double, List<Action>>>());
                }

                List<Action> firstRoundActions = sim.Crafter.Actions.ToList();

                // lets see how much these influence speed
                firstRoundActions.Remove(Atlas.Actions.Manipulation);
                firstRoundActions.Remove(Atlas.Actions.WasteNot);
                firstRoundActions.Remove(Atlas.Actions.WasteNot2);

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
                    State finishState = sim.Simulate(actions, startState);
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
                            progress[1].Add(new Tuple<double, List<Action>>(score.Item1, actions));
                        }
                    }
                    else { simFail++; }
                }

                for (int i = 1; i < sim.MaxLength; i++)
                {
                    foreach (var solution in progress[i])
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

                            State finishState;
                            Tuple<double, bool> score;
                            if (Atlas.Actions.Buffs.Contains(action))
                            {
                                Tuple<Tuple<double, bool>, List<Action>, State> bestBuff = BuildBuff(sim, startState, steps, otherActions, action, action.ActiveTurns, new Dictionary<int, double>());
                                score = bestBuff.Item1;
                                steps = bestBuff.Item2;
                                finishState = bestBuff.Item3;
                            }
                            else
                            {
                                finishState = sim.Simulate(steps, startState);
                                score = ScoreState(sim, finishState);
                                attempts++;
                            }

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
                                else if (progress[steps.Count].Count() < solutionsToKeep)
                                {
                                    progress[steps.Count].Add(new Tuple<double, List<Action>>(score.Item1, steps));
                                    if (score.Item1 < worstScores[steps.Count]) worstScores[steps.Count] = score.Item1;
                                }
                                else if (score.Item1 > worstScores[steps.Count])
                                {
                                    progress[steps.Count].RemoveAll(x => x.Item1 == worstScores[steps.Count]);
                                    progress[steps.Count].Add(new Tuple<double, List<Action>>(score.Item1, steps));
                                    worstScores[steps.Count] = progress[steps.Count].Min(x => x.Item1);
                                }
                            }
                            else { simFail++; }
                        }
                    }
                    progress[i].Clear();
                    times.Add(new Tuple<int, long>(i, sw.ElapsedMilliseconds));
                }

            foundPerfect:
                bestScore = solutions.Max(y => y.Item1);
                var found = solutions.Where(x => x.Item1 == bestScore);
                var bestSolution = found.First().Item2;

                State final = sim.Simulate(bestSolution, startState, debug: true);
                return bestSolution;
            }

            public Tuple<Tuple<double, bool>, List<Action>, State> BuildBuff(Simulator sim, State startState, List<Action> actions, List<Action> crafterActions, Action buff, int turns, Dictionary<int, double> bestScores)
            {
                if (actions.Count > sim.MaxLength) return new Tuple<Tuple<double, bool>, List<Action>, State>(new Tuple<double, bool>(-1, false), actions, null);

                if (buff.ActionType == ActionType.CountUp) // deal with it later, its probably IQ or Reflect
                {
                    throw new NotImplementedException("We're not passing IQ, Muscle Memory, or Reflect into here yet, soooooo");
                }
                else if (buff.ActionType == ActionType.CountDown || buff.ActionType == ActionType.Immediate) // Immediate is just Observe at time of writing
                {
                    double bestScore = double.MinValue;
                    List<Action> bestActions = actions;
                    State bestState = startState;

                    foreach (Action action in crafterActions)
                    {
                        List<Action> steps = actions.ToList();
                        steps.Add(action);

                        if (SolutionAudit(sim, steps, crafterActions))
                        {
                            Tuple<Tuple<double, bool>, List<Action>, State> buffScore;
                            if (Atlas.Actions.Buffs.Contains(action)) // buff inside a buff, neat
                            {
                                buffScore = BuildBuff(sim, startState, steps, crafterActions, action, action.ActiveTurns, new Dictionary<int, double>());
                            }
                            else
                            {
                                attempts++;
                                State state = sim.Simulate(steps, startState);
                                Tuple<double, bool> score = ScoreState(sim, state);

                                if (score.Item2)
                                {
                                    return new Tuple<Tuple<double, bool>, List<Action>, State>(score, steps, state);
                                }
                                else if (!bestScores.ContainsKey(steps.Count) || score.Item1 > bestScores[steps.Count])
                                {
                                    if (!bestScores.ContainsKey(steps.Count)) bestScores.Add(steps.Count, score.Item1);
                                    else bestScores[steps.Count] = score.Item1;

                                    int buffDuration = state.BuffDuration(buff);
                                    if (buffDuration > 0) buffScore = BuildBuff(sim, startState, steps, crafterActions, buff, buffDuration, bestScores);
                                    else buffScore = new Tuple<Tuple<double, bool>, List<Action>, State>(score, steps, state);
                                }
                                else if (score.Item1 < 0)
                                {
                                    simFail++;
                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            if (buffScore.Item1.Item2)
                            {
                                return buffScore;
                            }
                            else if (buffScore.Item1.Item1 > bestScore)
                            {
                                bestScore = buffScore.Item1.Item1;
                                bestActions = buffScore.Item2;
                                bestState = buffScore.Item3;
                            }
                        }
                        else
                        {
                            auditFail++;
                        }
                    }

                    return new Tuple<Tuple<double, bool>, List<Action>, State>(new Tuple<double, bool>(bestScore, false), bestActions, bestState);
                }
                else // ????  I don't think these exist right now
                {
                    throw new NotImplementedException("Do Indefinite buffs even exist?");
                }
            }
        }
        public class BruteForceSolver4
        {
            const int LOOKAHEAD_AMOUNT = 6;
            const int MAX_PROGRESS_SIZE = 20;
            const int MAX_LOOKAHEAD_SIZE = 300;
            bool FOUND_PERFECT = false;
            Random random = new Random();
            #region Audits
            public delegate bool Audit(Simulator sim, List<Action> actions, List<Action> crafterActions);
            public static Audit[] audits = new Audit[]
            {
                AuditCP,
                AuditUnfocused,
                AuditDurability,
                AuditRepeatBuffs
            };

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
                List<Action> wastedBuffs = new List<Action>();
                foreach (Action action in actions)
                {
                    if (Atlas.Actions.Buffs.Contains(action))
                    {
                        if (wastedBuffs.Contains(action))
                        {
                            auditDict["AuditRepeatBuffs"]++;
                            return false;
                        }
                        wastedBuffs.Add(action);
                    }
                    else if (wastedBuffs.Any())
                    {
                        if (Atlas.Actions.ProgressActions.Contains(action))
                        {
                            wastedBuffs.RemoveAll(buff => Atlas.Actions.ProgressBuffs.Contains(buff));
                            wastedBuffs.RemoveAll(buff => !Atlas.Actions.QualityBuffs.Contains(buff));
                        }
                        else if (Atlas.Actions.QualityActions.Contains(action))
                        {
                            wastedBuffs.RemoveAll(buff => Atlas.Actions.QualityBuffs.Contains(buff));
                            wastedBuffs.RemoveAll(buff => !Atlas.Actions.ProgressBuffs.Contains(buff));
                        }
                    }
                }

                return true;
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
                int wasteNotTurns = 0;
                foreach (Action action in actions)
                {
                    if (dur <= 0)
                    {
                        auditDict["AuditDurability"]++;
                        return false;
                    }

                    if (action.Equals(Atlas.Actions.Groundwork) && dur < action.DurabilityCost) dur -= 10;
                    else if (action.DurabilityCost > 0) dur -= wasteNotTurns > 0 ? action.DurabilityCost / 2 : action.DurabilityCost;
                    else if (action.Equals(Atlas.Actions.MastersMend)) dur = Math.Min(sim.Recipe.Durability, dur + 30);

                    if (action.Equals(Atlas.Actions.Manipulation)) manipTurns = 8;
                    else if (action.Equals(Atlas.Actions.WasteNot)) wasteNotTurns = 4;
                    else if (action.Equals(Atlas.Actions.WasteNot2)) wasteNotTurns = 8;
                    else if (manipTurns > 0 && dur > 0)
                    {
                        dur = Math.Min(sim.Recipe.Durability, dur + 5);
                    }
                    manipTurns = Math.Max(manipTurns - 1, 0);
                    wasteNotTurns = Math.Max(wasteNotTurns - 1, 0);
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
                double bestScore = 0;
                State startState = sim.Simulate(null, new State());

                double worstScore = 0;
                object lockObj = new object();
                Tuple<double, List<Action>> solution = new Tuple<double, List<Action>>(0, new List<Action>());
                List<Tuple<double, List<Action>>> progress = new List<Tuple<double, List<Action>>>();

                List<Action> firstRoundActions = GetFirstRoundActions(sim);
                firstRoundActions.Remove(Atlas.Actions.DummyAction);
                List<Action> otherActions = GetOtherActions(firstRoundActions);

                Stopwatch sw = new Stopwatch();
                List<Tuple<int, long>> times = new List<Tuple<int, long>>();
                sw.Start();

                foreach (Action action in firstRoundActions)
                {
                    List<Action> actions = new List<Action> { action };
                    State finishState = sim.Simulate(actions, startState);
                    attempts++;
                    Tuple<double, bool> score = ScoreState(sim, finishState);
                    if (score.Item1 > 0)
                    {
                        if (finishState.CheckViolations().ProgressOk)
                        {
                            if (score.Item1 > bestScore)
                            {
                                bestScore = score.Item1;
                                solution = new Tuple<double, List<Action>>(score.Item1, actions);
                                if (score.Item2) goto foundPerfect;
                            }
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
                    List<Tuple<double, List<Action>>> nextProgress = new List<Tuple<double, List<Action>>>();
                    foreach (Tuple<double, List<Action>> possibleSolution in progress)
                    {
                        List<Task> tasks = new List<Task>();
                        foreach (Action action in otherActions)
                        {
                            if (FOUND_PERFECT) goto foundPerfect;
                            tasks.Add(Task.Factory.StartNew(() =>
                            {
                                List<Action> steps = possibleSolution.Item2.ToList();
                                steps.Add(action);

                                State finishState = sim.Simulate(steps, startState);
                                Tuple<double, bool> score = ScoreState(sim, finishState);
                                attempts++;

                                if (score.Item1 <= 0)
                                {
                                    lock (lockObj) simFail++;
                                    return;
                                }

                                if (finishState.CheckViolations().ProgressOk)
                                {
                                    lock (lockObj)
                                    {
                                        if (score.Item1 > bestScore)
                                        {
                                            bestScore = score.Item1;
                                            solution = new Tuple<double, List<Action>>(score.Item1, steps);
                                            if (score.Item2)
                                            {
                                                FOUND_PERFECT = true;
                                                return;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    List<Tuple<double, List<Action>>> lookaheadSolutions = LookAhead(sim, startState, steps, otherActions, LOOKAHEAD_AMOUNT);
                                    Tuple<double, List<Action>> bestLookahead = new Tuple<double, List<Action>>(0, new List<Action>());
                                    foreach (var lookaheadSolution in lookaheadSolutions) if (lookaheadSolution.Item1 > bestLookahead.Item1) bestLookahead = lookaheadSolution;

                                    lock (lockObj)
                                    {
                                        if (FOUND_PERFECT)
                                        {
                                            solution = bestLookahead;
                                            return;
                                        }
                                        else if (nextProgress.Count() < MAX_PROGRESS_SIZE)
                                        {
                                            nextProgress.Add(new Tuple<double, List<Action>>(bestLookahead.Item1, bestLookahead.Item2.Take(i + 1).ToList()));
                                            worstScore = nextProgress.Min(x => x.Item1);
                                        }
                                        else if (bestLookahead.Item1 > worstScore)
                                        {
                                            nextProgress.RemoveAll(x => x.Item1 == worstScore);
                                            nextProgress.Add(new Tuple<double, List<Action>>(bestLookahead.Item1, bestLookahead.Item2.Take(i + 1).ToList()));
                                            worstScore = nextProgress.Min(x => x.Item1);
                                        }
                                    }
                                }
                            }));
                            Task.WaitAll(tasks.ToArray());
                        }
                    }
                    progress = nextProgress;
                }

            foundPerfect:
                sw.Stop();
                State final = sim.Simulate(solution.Item2, startState, debug: true);
                return solution.Item2.Take(final.LastStep).ToList();
            }

            public List<Tuple<double, List<Action>>> LookAhead(Simulator sim, State startState, List<Action> progress, List<Action> actions, int amount, SortedList<double, List<Action>>[] paths = null)
            {
                // initialize arrays
                if (paths == null)
                {
                    paths = new SortedList<double, List<Action>>[amount];
                    for (int i = 0; i < amount; i++)
                    {
                        paths[i] = new SortedList<double, List<Action>>();
                    }
                }
                int index = paths.Length - amount;

                List<Tuple<double, List<Action>>> retChoices = new List<Tuple<double, List<Action>>>();
                foreach (Action action in actions)
                {
                    if (FOUND_PERFECT) continue;
                    List<Action> steps = progress.ToList();
                    steps.Add(action);

                    State finishState = sim.Simulate(steps, startState);
                    Tuple<double, bool> score = ScoreState(sim, finishState);
                    attempts++;

                    if (score.Item1 <= 0)
                    {
                        simFail++;
                        continue;
                    }
                    else if (paths[index].Count == MAX_LOOKAHEAD_SIZE && score.Item1 < paths[index].First().Key)
                    {
                        lookaheadSkip++;
                        continue;
                    }

                    double agg = 0;
                    while (paths[index].ContainsKey(score.Item1 + agg))
                    {
                        agg += random.NextDouble() / 10000;
                        repeated++;
                    }
                    paths[index].Add(score.Item1 + agg, steps);

                    if (paths[index].Count > MAX_LOOKAHEAD_SIZE) paths[index].RemoveAt(0);

                    if (finishState.CheckViolations().ProgressOk || amount == 1)
                    {
                        retChoices.Add(new Tuple<double, List<Action>>(score.Item1, steps));
                        if (score.Item2) FOUND_PERFECT = true;
                    }
                    else if (amount == 1)
                    {
                        retChoices.Add(new Tuple<double, List<Action>>(score.Item1, steps));
                    }
                    else
                    {
                        retChoices.AddRange(LookAhead(sim, startState, steps, actions, amount - 1, paths));
                    }
                }
                return retChoices;
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
                AuditFirstRound
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

                State startState = sim.Simulate(null, new State());
                List<Tuple<double, List<Action>>> solutions = new List<Tuple<double, List<Action>>>();

                sw.Start();

                List<Action> actions = new List<Action>();
                while (!(actions.Count == sim.MaxLength && actions.All(x => x == crafterActions[crafterActions.Count - 1])))
                {
                    Iterate(actions, actionsDict, crafterActions, actions.Count - 1);
                    if (!SolutionAudit(actions, crafterActions)) continue;

                    attempts++;
                    State finalState = sim.Simulate(actions, startState);
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

                State final = sim.Simulate(bestSolution, startState, debug: true);
                return bestSolution;
            }
        }
        public class GeneticSolver
        {
            const int MAX_GENERATION = 50;
            const int ELITE_PERCENTAGE = 10;
            const int MATE_PERCENTAGE = 50;
            const int INITIAL_POPULATION = 5000000; // 5 million
            const int GENERATION_SIZE = 500000; // 500 thousand
            const int PROB_MUTATION = 15;

            Random rand = new Random();
            Action[] genes;
            int probParentX = 100 - (PROB_MUTATION / 2);

            public class ListComparer : IComparer<KeyValuePair<double, List<Action>>>
            {
                int IComparer<KeyValuePair<double, List<Action>>>.Compare(KeyValuePair<double, List<Action>> x, KeyValuePair<double, List<Action>> y) => (int)(y.Key - x.Key);
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
                AuditInnerQuiet,
                AuditRepeatBuffs,
                AuditFirstRound,
                AuditFocused,
                AuditTrainedFinesse
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
            private static bool AuditTrainedFinesse(List<Action> actions, List<Action> crafterActions)
            {
                int iq = 0;
                foreach(Action action in actions)
                {
                    if (action.QualityIncreaseMultiplier > 0) iq++;
                    if (action.Equals(Atlas.Actions.PreparatoryTouch)) iq++;
                    if (action.Equals(Atlas.Actions.Reflect)) iq++;
                    if (action.Equals(Atlas.Actions.ByregotsBlessing)) iq = 0;

                    if (action.Equals(Atlas.Actions.TrainedFinesse) && iq < 10) return false;
                }
                return true;
            }

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
            #endregion

            public Action MutateGene(Random r) => genes[r.Next(genes.Length - 1)];
            public List<Action> CreateChromosome(int length, Random r)
            {
                List<Action> actions = new List<Action>();
                for (int i = 0; i < length; i++)
                {
                    Action action = MutateGene(r);
                    actions.Add(action);
                    if (action.Equals(Atlas.Actions.DummyAction)) break;
                }
                return actions;
            }
            public List<Action> Mate(List<Action> parent1, List<Action> parent2, Random r)
            {
                List<Action> actions = new List<Action>();

                int length = r.Next(2) == 0 ? parent1.Count : parent2.Count;
                for (int i = 0; i < length; i++)
                {
                    Action action;
                    int p = r.Next(100);

                    if (p <= probParentX) action = parent1.Count - 1 > i ? parent2[i] : parent1[i];
                    else if (p <= probParentX * 2) action = parent2.Count - 1 > i ? parent1[i] : parent2[i];
                    else action = MutateGene(r);

                    actions.Add(action);
                    if (action.Equals(Atlas.Actions.DummyAction)) break;
                }

                return actions;
            }

            public void BuildPopulation(ConcurrentBag<List<Action>> population, int maxLength, int maxTasks)
            {
                var genesList = genes.ToList();
                Task[] actions = new Task[maxTasks];
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i] = ThreadPool.RunAsync((workItem) =>
                    {
                        Random r = new Random(rand.Next() % (i + 1));
                        for (int x = 0; x < INITIAL_POPULATION; x += maxTasks)
                        {
                            var chromosome = CreateChromosome(maxLength, r);
                            if (!SolutionAudit(chromosome, genesList)) continue;
                            population.Add(chromosome);
                        }
                    }).AsTask();
                }
                Task.WaitAll(actions);
            }

            public List<Action> GetSucessfulSteps(Simulator sim, List<Action> actions, State startState)
            {
                State finishState = sim.Simulate(actions, startState);
                return actions.Take(finishState.LastStep).ToList();
            }

            public List<Action> Run(Simulator sim, int maxTasks)
            {
                int generation = 1;

                genes = sim.Crafter.Actions;
                List<Action> genesList = genes.ToList();
                ListComparer comparer = new ListComparer();

                int maxLength = sim.MaxLength;
                State startState = sim.Simulate(null, new State());

                ConcurrentBag<List<Action>> population = new ConcurrentBag<List<Action>>();
                object lockObj = new object();
                bool foundPerfect = false;
                List<Action> perfect = new List<Action>();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                BuildPopulation(population, maxLength, maxTasks);

                while (true)
                {
                    GC.Collect();
                    if (population.Count == 0) BuildPopulation(population, maxLength, maxTasks);

                    ConcurrentBag<KeyValuePair<double, List<Action>>> scoredPopulation = new ConcurrentBag<KeyValuePair<double, List<Action>>>();
                    Task[] actions = new Task[maxTasks];
                    for (int i = 0; i < actions.Length; i++)
                    {
                        actions[i] = ThreadPool.RunAsync((workItem) =>
                        {
                            while (!foundPerfect && population.TryTake(out List<Action> chromosome))
                            {
                                State state = sim.Simulate(chromosome, startState);
                                Tuple<double, bool> score = ScoreState(sim, state);
                                if (score.Item1 > 0)
                                {
                                    if (state.CheckViolations().ProgressOk && score.Item2)
                                    {
                                        foundPerfect = true;
                                        perfect = chromosome;
                                        continue;
                                    }
                                    scoredPopulation.Add(new KeyValuePair<double, List<Action>>(score.Item1, chromosome));
                                }
                            }
                        }).AsTask();
                    }
                    Task.WaitAll(actions);

                    if (foundPerfect)
                        return GetSucessfulSteps(sim, perfect, startState);

                    population = new ConcurrentBag<List<Action>>();
                    List<KeyValuePair<double, List<Action>>> scores = scoredPopulation.ToList();
                    scores.Sort(comparer);
                    if (generation == MAX_GENERATION)
                        return GetSucessfulSteps(sim, scores.First().Value, startState);
                    scores.RemoveAll(x => x.Key == -1);

                    if (scores.Any())
                    {
                        var best = scores.First();
                        int generationSize = generation <= 2 ? INITIAL_POPULATION : GENERATION_SIZE;

                        // take top percent of population
                        int populationSize = Math.Min(generationSize, scores.Count);
                        int eliteCount = (int)Math.Ceiling(populationSize * ((double)ELITE_PERCENTAGE / 100));
                        IEnumerable<List<Action>> elites = scores.Take(eliteCount).Select(x => x.Value);
                        foreach (List<Action> elite in elites) population.Add(elite);

                        // mate next percent of population
                        int mateCount = (int)Math.Ceiling(populationSize * ((double)MATE_PERCENTAGE / 100));
                        List<List<Action>> matingPool = scores.Take(mateCount).Select(x => x.Value).ToList();
                        while (true)
                        {
                            List<Action> parent1 = matingPool[rand.Next(mateCount - 1)];
                            List<Action> parent2 = matingPool[rand.Next(mateCount - 1)];
                            var chromosome = Mate(parent1, parent2, rand);
                            if (!SolutionAudit(chromosome, genesList)) continue;
                            if (population.Count >= generationSize) break;
                            population.Add(chromosome);
                        }
                    }
                    
                    generation++;
                }
            }
        }

        // Just a Bunch of Actions
        public class JABOASolver
        {
            private static double maxDurabilityCost = 88 / 30D;
            private static double minDurabilityCost = 96 / 40D;
            private static double wasteNotPerActionCost = 98 / 8D;

            private static long listsEvaluated = 0;

            public List<Action> Run(Simulator sim, int maxTasks)
            {
                // trying to get the best community set
                //var thing = new Dictionary<Action, int>()
                //{
                //    { Atlas.Actions.PreparatoryTouch, 5 },
                //    { Atlas.Actions.Innovation, 2 },
                //    { Atlas.Actions.AdvancedTouch, 1 },
                //    { Atlas.Actions.GreatStrides, 2 },
                //    { Atlas.Actions.TrainedFinesse,1 },
                //    { Atlas.Actions.ByregotsBlessing, 1 },
                //    { Atlas.Actions.MuscleMemory, 1 },
                //    { Atlas.Actions.Veneration, 2 },
                //    { Atlas.Actions.Groundwork, 2 },
                //    { Atlas.Actions.CarefulSynthesis, 1 },
                //    { Atlas.Actions.Manipulation,1 },
                //    { Atlas.Actions.WasteNot2, 1 }
                //};
                //var thingResults = CombineActionSets(sim, sim.Simulate(null, new State()), thing, qualityListChecks, qualityOnly: false);
                //var best = thingResults.OrderBy(x => x.Item1).LastOrDefault();
                //Debug.WriteLine($"{best.Item1}  -  {string.Join(", ", best.Item2.Select(x => x.ShortName))}");



                int cp = sim.Crafter.CP + (int)(sim.Recipe.Durability * maxDurabilityCost);
                State startState = sim.Simulate(null, new State());

                // solve just progress actions, minimize CP
                Action[] progressActions = new Action[Atlas.Actions.ProgressActions.Length + Atlas.Actions.ProgressBuffs.Length];
                Atlas.Actions.ProgressActions.CopyTo(progressActions, 0);
                Atlas.Actions.ProgressBuffs.CopyTo(progressActions, Atlas.Actions.ProgressActions.Length);
                Action[] qualityActions = new Action[Atlas.Actions.QualityActions.Length + Atlas.Actions.QualityBuffs.Length];
                Atlas.Actions.QualityActions.CopyTo(qualityActions, 0);
                Atlas.Actions.QualityBuffs.CopyTo(qualityActions, Atlas.Actions.QualityActions.Length);

                Debug.WriteLine($"\n[{DateTime.Now}] Generating Progress Combinations");
                List<Dictionary<Action, int>> progressSets = GetCombinations(progressActions, 8, cp, progressChecks, sim.BaseProgressIncrease, sim.Recipe.Difficulty);
                Debug.WriteLine($"[{DateTime.Now}] Combinations Generated: {progressSets.Count()}");
                RemoveBadProgress(progressSets, sim.BaseProgressIncrease, sim.Recipe.Difficulty);
                Debug.Write($"[{DateTime.Now}] Removed Bad Progress: Combinations Remaining: {progressSets.Count()}");
                var p = progressSets.SelectMany(x => CombineActionSets(sim, startState, x, progressListChecks));
                var q = p.Min(x => x.Item2.Count());
                List<List<Action>> progressLists = p.Where(y => y.Item2.Count == q).OrderBy(x => ListToCpCost(x.Item2)).Select(x => x.Item2).ToList();
                Debug.WriteLine($"\n[{DateTime.Now}] Lists Generated: {p.Count()}");
                Debug.WriteLine($"\n[{DateTime.Now}] Good Lists Found: {progressLists.Count()}\n\t{string.Join(",", progressLists.First().Select(x => x.ShortName))}");
                cp = sim.Crafter.CP + (int)(sim.Recipe.Durability * maxDurabilityCost) - (int)ListToCpCost(progressLists[0]);

                List<Dictionary<Action, int>> prevSets = new List<Dictionary<Action, int>>();
                Tuple<double, List<Action>> bestSolution = new Tuple<double, List<Action>>(0, null);
                for (int i = 0; i < 11; i++)
                {
                    // solve just quality actions, minimize CP
                    Debug.WriteLine($"\n[{DateTime.Now}] Generating Quality Combinations {{{i}}}");
                    prevSets = CombinationGenerator(qualityActions, prevSets, cp, out var newCombinations).Where(x => qualityChecks.All(audit => audit(x, sim.BaseQualityIncrease, sim.Recipe.MaxQuality))).ToList();
                    Debug.WriteLine($"[{DateTime.Now}] Combinations Generated: {newCombinations.Count()}");
                    RemoveBadQuality(newCombinations, sim.BaseQualityIncrease, sim.Recipe.MaxQuality);
                    Debug.Write($"[{DateTime.Now}] Removed Bad Quality; Combinations Remaining: {newCombinations.Count()}");
                    p = newCombinations.SelectMany(x => CombineActionSets(sim, startState, x, qualityListChecks, qualityOnly: true)).OrderBy(x => x.Item1).ToList();
                    Debug.WriteLine($"\n[{DateTime.Now}] Lists Generated: {p.Count()}");
                    var o = p.LastOrDefault();
                    if (o != null)
                    {
                        Debug.WriteLine($"\n[{DateTime.Now}] Good Lists Found: {p.Count()}  ||  Best Score: {o.Item1}\n\t{string.Join(",", o.Item2.Select(x => x.ShortName))}");
                    }
                }

                // zip them back together, dealing with durability actions

                return null;
            }

            #region Audits
            public delegate bool CombinationCheck(Dictionary<Action, int> set, double baseGain, int max);
            public static CombinationCheck[] progressChecks = new CombinationCheck[]
            {
                CheckMuscleMemory
            };
            public static bool CheckMuscleMemory(Dictionary<Action, int> set, double baseProgressGain, int progressMax)
            {
                return set[Atlas.Actions.MuscleMemory] <= 1;
            }
            public static CombinationCheck[] qualityChecks = new CombinationCheck[]
            {
                CheckByregots,
                CheckFirstRound
            };
            public static bool CheckByregots(Dictionary<Action, int> set, double baseQualityGain, int qualityMax)
            {
                return set[Atlas.Actions.ByregotsBlessing] <= 1;
            }
            public static bool CheckFirstRound(Dictionary<Action, int> set, double baseQualityGain, int qualityMax)
            {
                return set[Atlas.Actions.Reflect] + set[Atlas.Actions.TrainedEye] <= 1;
            }

            public delegate bool SolutionCheck(Simulator sim, List<Action> set);
            public static SolutionCheck[] solutionChecks = new SolutionCheck[]
            {
                CheckWasteNot,
                CheckDurability
            };
            public static bool CheckWasteNot(Simulator sim, List<Action> set)
            {
                int index = 0;
                while ((index = set.FindIndex(index, x => x.Equals(Atlas.Actions.WasteNot))) >= 0)
                {
                    index++;
                    var wasteNotActions = set.Skip(index).Take(Atlas.Actions.WasteNot.ActiveTurns);
                    if (wasteNotActions.Contains(Atlas.Actions.WasteNot) || wasteNotActions.Contains(Atlas.Actions.WasteNot2))
                        return false;

                    if (wasteNotActions.Count() == Atlas.Actions.WasteNot.ActiveTurns && wasteNotActions.Count(x => x.DurabilityCost > 0) > Atlas.Actions.WasteNot.ActiveTurns / 2)
                    {
                        return false;
                    }
                }

                index = 0;
                while ((index = set.FindIndex(index, x => x.Equals(Atlas.Actions.WasteNot2))) >= 0)
                {
                    index++;
                    var wasteNotActions = set.Skip(index).Take(Atlas.Actions.WasteNot.ActiveTurns);
                    if (wasteNotActions.Contains(Atlas.Actions.WasteNot) || wasteNotActions.Contains(Atlas.Actions.WasteNot2))
                        return false;

                    if (wasteNotActions.Count() == Atlas.Actions.WasteNot2.ActiveTurns && wasteNotActions.Count(x => x.DurabilityCost > 0) > Atlas.Actions.WasteNot2.ActiveTurns / 2)
                    {
                        return false;
                    }
                }
                return true;
            }
            public static bool CheckDurability(Simulator sim, List<Action> set)
            {
                int dur = sim.Recipe.Durability;
                int manipTurns = 0;
                int wasteNotTurns = 0;
                foreach (Action action in set)
                {
                    if (dur <= 0)
                    {
                        auditDict["AuditDurability"]++;
                        return false;
                    }

                    if (action.Equals(Atlas.Actions.Groundwork) && dur < action.DurabilityCost) dur -= 10;
                    else if (action.DurabilityCost > 0) dur -= wasteNotTurns > 0 ? action.DurabilityCost / 2 : action.DurabilityCost;
                    else if (action.Equals(Atlas.Actions.MastersMend))
                    {
                        if (dur == sim.Recipe.Durability)
                            return false;
                        dur = Math.Min(sim.Recipe.Durability, dur + 30);
                    }

                    if (action.Equals(Atlas.Actions.Manipulation)) manipTurns = 8;
                    else if (action.Equals(Atlas.Actions.WasteNot)) wasteNotTurns = 4;
                    else if (action.Equals(Atlas.Actions.WasteNot2)) wasteNotTurns = 8;
                    else if (manipTurns > 0 && dur > 0)
                    {
                        dur = Math.Min(sim.Recipe.Durability, dur + 5);
                    }
                    manipTurns = Math.Max(manipTurns - 1, 0);
                    wasteNotTurns = Math.Max(wasteNotTurns - 1, 0);
                }
                return true;
            }
            public static SolutionCheck[] progressListChecks = new SolutionCheck[]
            {

            };
            public static SolutionCheck[] qualityListChecks = new SolutionCheck[]
            {

            };
            #endregion

            public List<Dictionary<Action, int>> GetCombinations(Action[] actionSet, int maxLength, int cpMax, CombinationCheck[] combinationChecks, double baseGain, int max)
            {
                List<Dictionary<Action, int>> combinations = new List<Dictionary<Action, int>>();
                List<Dictionary<Action, int>> prevSets = new List<Dictionary<Action, int>>();
                for (int i = 0; i < maxLength; i++)
                {
                    prevSets = CombinationGenerator(actionSet, prevSets, cpMax, out var newCombinations).Where(x => combinationChecks.All(audit => audit(x, baseGain, max))).ToList();
                    combinations.AddRange(newCombinations);
                }
                combinations.AddRange(prevSets);
                return combinations;
            }
            public List<Dictionary<Action, int>> CombinationGenerator(Action[] actionSet, List<Dictionary<Action, int>> prevSets, int cpMax, out List<Dictionary<Action, int>> addCombinations)
            {
                List<Dictionary<Action, int>> newCombinations = new List<Dictionary<Action, int>>();
                addCombinations = new List<Dictionary<Action, int>>();
                if (prevSets.Count == 0)
                {
                    foreach (Action action in actionSet)
                    {
                        Dictionary<Action, int> dict = new Dictionary<Action, int>();
                        foreach (KeyValuePair<Action, int> kvp in actionSet.Select(x => new KeyValuePair<Action, int>(x, 0)))
                        {
                            dict.Add(kvp.Key, kvp.Value);
                        }
                        dict[action] = 1;
                        newCombinations.Add(dict);
                    }
                    return newCombinations;
                }

                foreach (Dictionary<Action, int> combinations in prevSets)
                {
                    bool thisSetAdded = false;
                    foreach (Action action in actionSet)
                    {
                        Dictionary<Action, int> newCombination = combinations.ToDictionary(x => x.Key, x => x.Value);
                        newCombination[action]++;
                        if (SetToCpCost(newCombination) > cpMax)
                        {
                            if (!thisSetAdded)
                            {
                                newCombination[action]--;
                                addCombinations.Add(newCombination);
                                thisSetAdded = true;
                            }
                        }
                        else
                        {
                            newCombinations.Add(newCombination);
                        }
                    }
                }
                return newCombinations.Distinct(new DictionaryComparer()).ToList();
            }
            public class DictionaryComparer : IEqualityComparer<Dictionary<Action, int>>
            {
                public string CompareString(Dictionary<Action, int> obj)
                {
                    return string.Join("", obj.OrderBy(x => x.Key.ID).Select(x => x.Value));
                }
                public bool Equals(Dictionary<Action, int> x, Dictionary<Action, int> y)
                {
                    if (GetHashCode(x) != GetHashCode(y))
                    {
                        return false;
                    }
                    else
                    {
                        return CompareString(x) == CompareString(y);
                    }
                }
                public int GetHashCode(Dictionary<Action, int> obj)
                {
                    return CompareString(obj).GetHashCode();
                }
            }

            public static double SetToCpCost(Dictionary<Action, int> set)
            {
                double cpTotal = set.Sum(y => (y.Key.CPCost + y.Key.DurabilityCost * minDurabilityCost) * y.Value);

                set.TryGetValue(Atlas.Actions.FocusedSynthesis, out int fSynthesis);
                set.TryGetValue(Atlas.Actions.FocusedTouch, out int fTouch);
                int observeCost = (fSynthesis + fTouch) * Atlas.Actions.Observe.CPCost;

                set.TryGetValue(Atlas.Actions.AdvancedTouch, out int aTouch);
                set.TryGetValue(Atlas.Actions.StandardTouch, out int sTouch);
                set.TryGetValue(Atlas.Actions.BasicTouch, out int bTouch);
                int standardCombo = Math.Min(bTouch, sTouch);
                int advancedCombo = Math.Min(standardCombo, aTouch);
                int comboSavings = standardCombo * (Atlas.Actions.StandardTouch.CPCost - Atlas.Actions.BasicTouch.CPCost)
                                 + advancedCombo * (Atlas.Actions.AdvancedTouch.CPCost - Atlas.Actions.BasicTouch.CPCost);

                //IEnumerable<KeyValuePair<Action, int>> wasteNotTargets = set.Where(x => x.Value > 0 && x.Key.DurabilityCost > 5 && !Atlas.Actions.FirstRoundActions.Contains(x.Key));
                //double durabilitySavings = wasteNotTargets.Sum(x => x.Key.DurabilityCost / 2 * x.Value * minDurabilityCost);
                //double wasteNotSavings = durabilitySavings + wasteNotPerActionCost * wasteNotTargets.Sum(x => x.Value);
                double wasteNotSavings = 0;

                return cpTotal + observeCost - comboSavings - wasteNotSavings;
            }
            public static double ListToCpCost(List<Action> list)
            {
                double cpTotal = list.Sum(x => x.CPCost + x.DurabilityCost * minDurabilityCost);
                int observeCost = (list.Count(x => x.Equals(Atlas.Actions.FocusedSynthesis)) + list.Count(x => x.Equals(Atlas.Actions.FocusedTouch))) * Atlas.Actions.Observe.CPCost;

                int comboSavings = 0;
                int index = 0;
                while ((index = list.FindIndex(index, x => x.Equals(Atlas.Actions.StandardTouch))) >= 0)
                {
                    if (list[index - 1].Equals(Atlas.Actions.BasicTouch))
                    {
                        comboSavings += Atlas.Actions.StandardTouch.CPCost - Atlas.Actions.BasicTouch.CPCost;
                        if (list[index + 1].Equals(Atlas.Actions.AdvancedTouch))
                        {
                            comboSavings += Atlas.Actions.AdvancedTouch.CPCost - Atlas.Actions.BasicTouch.CPCost;
                        }
                    }
                    index++;
                }

                return cpTotal + observeCost - comboSavings;
            }
            
            public static void RemoveBadProgress(List<Dictionary<Action, int>> sets, double baseProgressGain, int progressMax)
            {
                sets.RemoveAll(x =>
                {
                    double progress = 0;
                    progress = x.Sum(y => y.Key.ProgressIncreaseMultiplier * y.Value);
                    if (x[Atlas.Actions.Veneration] > 0)
                    {
                        int rounds = Math.Min(x[Atlas.Actions.Veneration] * Atlas.Actions.Veneration.ActiveTurns, x.Where(y => !y.Key.Equals(Atlas.Actions.MuscleMemory) && !y.Key.Equals(Atlas.Actions.Veneration)).Sum(y => y.Value));
                        var maxOption = x.Where(y => y.Value > 0 && !y.Key.Equals(Atlas.Actions.MuscleMemory)).Max(y => y.Key.ProgressIncreaseMultiplier);
                        progress += maxOption * 0.5 * rounds;
                    }
                    if (x[Atlas.Actions.MuscleMemory] > 1 && x.Values.Sum(y => y) > 1)
                    {
                        progress += 1;
                    }
                    return progress * baseProgressGain < progressMax;
                });
            }

            public static void RemoveBadQuality(List<Dictionary<Action, int>> sets, double baseQualityGain, int qualityMax)
            {
                sets.RemoveAll(x => x[Atlas.Actions.ByregotsBlessing] > 1);
                sets.RemoveAll(x => x[Atlas.Actions.Reflect] + x[Atlas.Actions.TrainedEye] > 1);
                sets.RemoveAll(x => x[Atlas.Actions.TrainedFinesse] > 0 && GetInnerQuiet(x) < 10);
                sets.RemoveAll(x => x[Atlas.Actions.TrainedEye] > 0 && x.Any(y => y.Key.QualityIncreaseMultiplier > 0 || Atlas.Actions.QualityBuffs.Contains(y.Key)));
                sets.RemoveAll(x =>
                {
                    if (x[Atlas.Actions.Innovation] > 0 || x[Atlas.Actions.GreatStrides] > 0 || x[Atlas.Actions.TrainedEye] > 0) return false;
                    double sum = x.Sum(y => y.Key.QualityIncreaseMultiplier * y.Value) + Math.Max(10, GetInnerQuiet(x)) * 0.2;
                    return sum < qualityMax;
                });
            }
            private static int GetInnerQuiet(Dictionary<Action, int> set)
            {
                return set.Where(x => x.Key.QualityIncreaseMultiplier > 0).Sum(x => x.Value) + set[Atlas.Actions.PreparatoryTouch];
            }
            private static void RemoveDuplicatedSets(List<Dictionary<Action, int>> qualitySets)
            {
                for (int i = 0; i < qualitySets.Count; i++)
                {
                    if (qualitySets.Any(x => qualitySets[i].All(y => x[y.Key] > y.Value)))
                    {
                        qualitySets.RemoveAt(i);
                        i--;
                    }
                }
            }
            public static List<Dictionary<Action, int>> OrderQuality(List<Dictionary<Action, int>> sets, double baseQualityGain, int qualityMax)
            {
                return sets.OrderBy(x =>
                {
                    if (x[Atlas.Actions.TrainedEye] > 0) return qualityMax;

                    double progress = 0;
                    progress = x.Sum(y => y.Key.QualityIncreaseMultiplier * y.Value);
                    IEnumerable<KeyValuePair<Action, int>> buffableActions = x.Where(y => !Atlas.Actions.QualityBuffs.ToList().Contains(y.Key) && !Atlas.Actions.FirstRoundActions.ToList().Contains(y.Key));

                    var maxOption = x.Where(y => y.Value > 0 && !y.Key.Equals(Atlas.Actions.Reflect)).Max(y => y.Key.QualityIncreaseMultiplier);
                    if (x[Atlas.Actions.Innovation] > 0)
                    {
                        int rounds = Math.Min(x[Atlas.Actions.Innovation] * Atlas.Actions.Innovation.ActiveTurns, buffableActions.Sum(y => y.Value));
                        progress += maxOption * 0.5 * rounds;
                    }
                    if (x[Atlas.Actions.GreatStrides] > 0)
                    {
                        int rounds = Math.Min(x[Atlas.Actions.GreatStrides], buffableActions.Sum(y => y.Value));
                        progress += maxOption * 1 * rounds;

                        if (x[Atlas.Actions.Innovation] > 0)
                        {
                            rounds = Math.Min(rounds, x[Atlas.Actions.Innovation] * Atlas.Actions.Innovation.ActiveTurns);
                            progress += maxOption * 1.5 * rounds;
                        }
                    }

                    if (x[Atlas.Actions.ByregotsBlessing] == 1)
                    {
                        progress += 1 + Math.Max(2, GetInnerQuiet(x) * 0.2);
                    }

                    return progress*qualityMax;
                }).ToList();
            }

            public static List<Tuple<double, List<Action>>> CombineActionSets(Simulator sim, State startState, Dictionary<Action, int> set, SolutionCheck[] checks, bool qualityOnly = false)
            {
                listsEvaluated = 0;
                int keepAmount = 100;
                IEnumerable<Action> firstRoundActions = set.Where(x => x.Value > 0 && Atlas.Actions.FirstRoundActions.Contains(x.Key)).Select(x => x.Key);
                if (firstRoundActions.Count() > 1) return null;

                Dictionary<Action, int> actionChoices = new Dictionary<Action, int>();
                foreach (var pair in set.Where(x => x.Value > 0)) actionChoices.Add(pair.Key, pair.Value);

                List<List<Action>> actionSets = new List<List<Action>>();
                GetLookupDictionary(actionChoices, out Dictionary<Action, int> lookup);

                // generate initial list
                Action firstRound = firstRoundActions.FirstOrDefault();
                if (firstRound != null)
                {
                    actionSets.Add(new List<Action>() { firstRound });
                }
                else
                {
                    foreach (var action in actionChoices.Where(x => x.Value > 0))
                    {
                        actionSets.Add(new List<Action>() { action.Key });
                    }
                }

                List<Tuple<double, List<Action>>> solutions = new List<Tuple<double, List<Action>>>();
                for (int i = 0; i < sim.MaxLength; i++)
                {
                    List<List<Action>> newActionSets = new List<List<Action>>();
                    foreach (var actionSet in actionSets)
                    {
                        List<List<Action>> responses = Iterate(sim, startState, actionSet, actionChoices, qualityOnly);
                        List<Tuple<bool, double, List<Action>>> scores = responses
                            .Where(x => checks.All(audit => true))
                            .Select(x =>
                            {
                                State state = sim.Simulate(x, startState, useDurability: true);
                                Tuple<double, bool> score = ScoreState(sim, state, ignoreProgress: qualityOnly);
                                if (score.Item1 > 0)
                                {
                                    return new Tuple<bool, double, List<Action>>(qualityOnly || state.CheckViolations().ProgressOk, score.Item1, x);
                                }
                                return null;
                            }).Where(x => x != null).ToList();

                        if (qualityOnly)
                        {
                            newActionSets.AddRange(scores.Select(x => x.Item3));
                            solutions.AddRange(scores.Select(x => new Tuple<double, List<Action>>(x.Item2, x.Item3)));
                        }
                        else
                        {
                            newActionSets.AddRange(scores.Where(x => !x.Item1).Select(x => x.Item3));
                            solutions.AddRange(scores.Where(x => x.Item1).Select(x => new Tuple<double, List<Action>>(x.Item2, x.Item3)));
                        }
                    }

                    actionSets = newActionSets;
                    if (qualityOnly)
                    {
                        solutions = solutions.OrderBy(x => x.Item1).Take(keepAmount).ToList();
                    }
                }

                listsEvaluated++;
                if (listsEvaluated % 10000 == 0) Debug.Write("|");
                else if (listsEvaluated % 1000 == 0) Debug.Write("-");
                return solutions;
            }
            private static List<List<Action>> Iterate(Simulator sim, State startState, List<Action> prevSet, Dictionary<Action, int> actionChoices, bool qualityOnly)
            {
                List<List<Action>> newSets = new List<List<Action>>();
                Dictionary<Action, int> counts = new Dictionary<Action, int>();
                foreach (var group in prevSet.GroupBy(x => x.ID))
                {
                    counts.Add(group.First(), group.Count());
                }

                var remainingActions = actionChoices.Where(x => !counts.ContainsKey(x.Key) || x.Value > counts[x.Key]).Select(x => x.Key);
                if (!qualityOnly && !remainingActions.Any(x => x.ProgressIncreaseMultiplier > 0)) return newSets;

                foreach (Action action in remainingActions)
                {
                    if (action == prevSet.Last() && Atlas.Actions.Buffs.Contains(action)) continue;

                    List<Action> newSet = prevSet.ToList();
                    newSet.Add(action);
                    newSets.Add(newSet);
                }
                return newSets;
            }
            private static void GetLookupDictionary(Dictionary<Action, int> actionChoices, out Dictionary<Action, int> lookup)
            {
                List<Action> choiceList = actionChoices.Keys.ToList();
                lookup = new Dictionary<Action, int>();

                for (int i = 0; i < choiceList.Count; i++)
                {
                    lookup.Add(choiceList[i], i);
                }
            }
        }
    }
}
