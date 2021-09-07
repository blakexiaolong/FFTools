using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using C5;

namespace CraftingSolver
{
    public class Solver
    {
        private readonly int maxTasks = 60;
        private readonly Simulator sim = new Simulator
        {
            Crafter = new Crafter
            {
                Class = "Armorer",
                Craftsmanship = 2737,
                Control = 2810,
                CP = 548,
                Level = 80,
                Specialist = false,
                Actions = Atlas.Actions.DependableActions
            },
            Recipe = new Recipe
            {
                Level = 480,
                Difficulty = 6178,
                Durability = 70,
                StartQuality = 0,
                MaxQuality = 36208,
                SuggestedCraftsmanship = 2480,
                SuggestedControl = 2195,
                Stars = 3
            },
            MaxTrickUses = 0,
            UseConditions = false,
            ReliabilityIndex = 1,
            MaxLength = 30,
        };

        public void Solve()
        {
            var solution = new BruteForceSolver3().Run(sim, maxTasks);
        }

        public interface ISolver
        {
            Action[] Run(Simulator sim, int maxTasks);
        }

        public class BruteForceSolver : ISolver
        {
            public Action[] Run(Simulator sim, int maxTasks)
            {
                State startState = sim.Simulate(null, new State(), false, false, true);
                List<Action> crafterActions = sim.Crafter.Actions.ToList();

                int solutionsToKeep = 500;
                SortedDictionary<double, List<Action[]>> bestSolutions = new SortedDictionary<double, List<Action[]>>();
                int totalItems = 0;

                List<Action[]> lastRound = new List<Action[]>();
                for (int i = 1; i <= sim.MaxLength; i++)
                {
                    if (i == 2)
                    {
                        crafterActions.Remove(Atlas.Actions.MastersMend);
                        crafterActions.Remove(Atlas.Actions.TrainedEye);
                        crafterActions.Remove(Atlas.Actions.Reflect);
                    }

                    object lockObj = new object();
                    Task[] tasks = new Task[maxTasks];

                    int j = 0;
                    lastRound = GeneratePossibleSolutions(bestSolutions, crafterActions);
                    for (int t = 0; t < maxTasks; t++)
                    {
                        Task task = Task.Factory.StartNew(() =>
                        {
                            while (true)
                            {
                                Action[] actions;
                                lock (lockObj)
                                {
                                    if (j < lastRound.Count)
                                    {
                                        actions = lastRound[j];
                                        j++;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                State finalState = sim.Simulate(actions, startState, false, false, true);
                                double score = ScoreState(finalState);
                                lock (lockObj)
                                {
                                    if (score == -1)
                                    {
                                        continue;
                                    }
                                    else if (bestSolutions.ContainsKey(score))
                                    {
                                        bestSolutions[score].Add(actions);
                                    }
                                    else if (bestSolutions.Count < solutionsToKeep)
                                    {
                                        bestSolutions.Add(score, new List<Action[]> { actions });
                                    }
                                    else if (score > bestSolutions.Keys.Min())
                                    {
                                        bestSolutions.Add(score, new List<Action[]> { actions });
                                        if (bestSolutions.Count > solutionsToKeep)
                                        {
                                            bestSolutions.Remove(bestSolutions.Keys.Min());
                                        }
                                    }
                                }
                            }
                        });
                        tasks[t] = task;
                    }
                    Task.WaitAll(tasks);
                    foreach (List<Action[]> set in bestSolutions.Values)
                    {
                        for (int s = 0; s < set.Count; s++)
                        {
                            if (set[s].Length < i)
                            {
                                set.Remove(set[s]);
                                s--;
                            }
                        }
                    }
                    List<double> emptyKeys = bestSolutions.Where(x => !x.Value.Any()).Select(x => x.Key).ToList();
                    foreach (double emptyKey in emptyKeys)
                    {
                        bestSolutions.Remove(emptyKey);
                    }
                    totalItems = bestSolutions.Sum(x => x.Value.Count);
                    if (bestSolutions.Any() && bestSolutions.Keys.Max() >= sim.Recipe.Difficulty + sim.Recipe.MaxQuality) break;
                }

                Action[] bestSolution = default;
                while (bestSolutions.Any())
                {
                    double maxKey = bestSolutions.Keys.Max();
                    List<Action[]> solutions = bestSolutions[maxKey].Where(x => !sim.Simulate(x, startState, false, false, false).CheckViolations().AnyIssues()).ToList();
                    if (solutions.Any())
                    {
                        bestSolution = solutions.First();
                        break;
                    }
                    else
                    {
                        bestSolutions.Remove(maxKey);
                    }
                }

                return bestSolution;
            }
            private List<Action[]> GeneratePossibleSolutions(SortedDictionary<double, List<Action[]>> bestSolutions, List<Action> actions)
            {
                List<Action[]> newSolutions = new List<Action[]>();
                foreach (List<Action[]> set in bestSolutions.Values)
                {
                    foreach (Action[] possibleSolution in set)
                    {
                        if (possibleSolution.Last() == Atlas.Actions.DummyAction)
                        {
                            List<Action> newSolution = possibleSolution.ToList();
                            newSolution.Add(Atlas.Actions.DummyAction);
                            newSolutions.Add(newSolution.ToArray());
                        }
                        foreach (Action action in actions)
                        {
                            List<Action> newSolution = possibleSolution.ToList();
                            newSolution.Add(action);
                            newSolutions.Add(newSolution.ToArray());
                        }
                    }
                }
                if (newSolutions.Count == 0) // get things started
                {
                    newSolutions = actions.Select(x => new Action[] { x }).ToList();
                }
                return newSolutions;
            }
            private double ScoreState(State state)
            {
                StateViolations violations = state.CheckViolations();
                if (violations.FailedCraft() || state.WastedActions > 0) return -1;
                if (state.Action == Atlas.Actions.DummyAction && !violations.ProgressOk) return -1;

                return state.Progress + state.Quality + 2 * (state.Simulator.MaxLength - state.Step);
            }
        }

        public class BruteForceSolver2 : ISolver
        {
            public Action[] Run(Simulator sim, int maxTasks)
            {
                State startState = sim.Simulate(null, new State(), false, false, true);

                List<Action> firstRoundActions = sim.Crafter.Actions.ToList();
                Action[] temp = new Action[firstRoundActions.Count];
                firstRoundActions.CopyTo(temp);
                List<Action> crafterActions = temp.ToList();
                crafterActions.Remove(Atlas.Actions.MastersMend);
                crafterActions.Remove(Atlas.Actions.TrainedEye);
                crafterActions.Remove(Atlas.Actions.Reflect);

                ulong count = 0;

                List<int> builder = new List<int>();
                for (int i = 0; i < sim.MaxLength; i++) builder.Add(0);

                double bestScore = 0;
                Action[] bestSolution = default;

                object lockObj = new object();
                Task[] tasks = new Task[maxTasks];
                for (int i = 0; i < maxTasks; i++)
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            List<Action> actions = new List<Action>();
                            lock (lockObj)
                            {
                                if (builder[0] >= firstRoundActions.Count)
                                {
                                    return; // no other actions to compute
                                }
                                for (int a = 0; a < builder.Count; a++)
                                {
                                    if (a == 0) actions.Add(firstRoundActions[builder[a]]);
                                    else actions.Add(crafterActions[builder[a]]);
                                    if (actions[a] == Atlas.Actions.DummyAction) break;
                                }
                                IterateBuilder(builder, crafterActions.Count);
                                count++;
                            }

                            Action[] actionsArray = actions.ToArray();
                            State finalState = sim.Simulate(actionsArray, startState, false, false, false);
                            double score = ScoreState(sim, finalState);
                            if (score > 0)
                            {
                                lock (lockObj)
                                {
                                    if (score > bestScore)
                                    {
                                        bestScore = score;
                                        bestSolution = actionsArray;
                                    }
                                }
                            }
                        }
                    });
                    tasks[i] = task;
                }
                Task.WaitAll(tasks);

                return bestSolution;
            }
            public void IterateBuilder(List<int> builder, int actCount, int? dummyIndex = null)
            {
                for (int i = dummyIndex ?? builder.Count - 1; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        builder[i]++;
                    }
                    else
                    {
                        if (builder[i] == actCount - 1)
                        {
                            builder[i] = 0;
                        }
                        else
                        {
                            builder[i]++;
                            return;
                        }
                    }
                }
            }
            public double ScoreState(Simulator sim, State state)
            {
                if (!state.CheckViolations().ProgressOk) return -1;
                else return state.Quality + 2 * (sim.MaxLength - state.Step);
            }
        }

        public class BruteForceSolver3
        {
            public Action[] Run(Simulator sim, int maxTasks)
            {
                ulong attempts = 0;
                int solutionsToKeep = 80000; // was 10000, running it overnight
                State startState = sim.Simulate(null, new State(), false, false, false);

                List<Tuple<double, Action[]>> progress = new List<Tuple<double, Action[]>>();
                List<Tuple<double, Action[]>> solutions = new List<Tuple<double, Action[]>>();

                List<Action> firstRoundActions = sim.Crafter.Actions.ToList();
                Action[] temp = new Action[firstRoundActions.Count];
                firstRoundActions.CopyTo(temp);
                List<Action> otherActions = temp.ToList();
                foreach (Action action in Atlas.Actions.FirstRoundActions) otherActions.Remove(action);
                if (firstRoundActions.Count - otherActions.Count == Atlas.Actions.FirstRoundActions.Count())
                    firstRoundActions = Atlas.Actions.FirstRoundActions.ToList();

                foreach (Action action in firstRoundActions)
                {
                    if (action == Atlas.Actions.DummyAction) continue;
                    Action[] actions = new Action[] { action };
                    State finishState = sim.Simulate(actions, startState, false, false, false);
                    attempts++;
                    Tuple<double, bool> score = ScoreState(sim, finishState);
                    if (score.Item1 > 0)
                    {
                        if (finishState.CheckViolations().ProgressOk)
                        {
                            solutions.Add(new Tuple<double, Action[]>(score.Item1, actions));
                            if (score.Item2) goto foundPerfect;
                        }
                        else
                        {
                            progress.Add(new Tuple<double, Action[]>(score.Item1, actions));
                        }
                    }
                }

                for (int i = 0; i < sim.MaxLength; i++)
                {
                    double worstScore = double.MaxValue;
                    List<Tuple<double, Action[]>> nextProgress = new List<Tuple<double, Action[]>>();
                    foreach (var solution in progress)
                    {
                        foreach (var action in otherActions)
                        {
                            List<Action> steps = solution.Item2.ToList();
                            steps.Add(action);
                            if (steps.Contains(Atlas.Actions.DummyAction)) continue;
                            var stepsArray = steps.ToArray();

                            State finishState = sim.Simulate(stepsArray, startState, false, false, false);
                            Tuple<double, bool> score = ScoreState(sim, finishState);
                            attempts++;

                            if (score.Item1 > 0)
                            {
                                if (finishState.CheckViolations().ProgressOk)
                                {
                                    solutions.Add(new Tuple<double, Action[]>(score.Item1, stepsArray));
                                    if (score.Item2)
                                        goto foundPerfect;
                                }
                                else if (nextProgress.Count() < solutionsToKeep)
                                {
                                    nextProgress.Add(new Tuple<double, Action[]>(score.Item1, stepsArray));
                                    if (score.Item1 < worstScore) worstScore = score.Item1;
                                }
                                else if (score.Item1 > worstScore)
                                {
                                    nextProgress.RemoveAll(x => x.Item1 == worstScore);
                                    nextProgress.Add(new Tuple<double, Action[]>(score.Item1, stepsArray));
                                    worstScore = nextProgress.Min(x => x.Item1);
                                }
                            }
                        }
                    }
                    progress = nextProgress;                
                }

                foundPerfect:
                var bestScore = solutions.Max(y => y.Item1);
                var found = solutions.Where(x => x.Item1 == bestScore);
                var bestSolution = found.First().Item2;

                State final = sim.Simulate(bestSolution, startState, false, true, true);
                return bestSolution;
            }
            public Tuple<double, bool> ScoreState(Simulator sim, State state)
            {
                if (!state.CheckViolations().DurabilityOk || !state.CheckViolations().CpOk || !state.CheckViolations().TrickOk || state.WastedActions > 0) return new Tuple<double, bool>(-1, false);

                double maxQuality = sim.Recipe.MaxQuality * 1.1;
                double progress = state.Progress > sim.Recipe.Difficulty ? sim.Recipe.Difficulty : state.Progress;
                double quality = state.Quality > maxQuality ? maxQuality : state.Quality;
                bool perfectSolution = state.Quality >= sim.Recipe.MaxQuality && state.Progress > sim.Recipe.Difficulty;
                return new Tuple<double, bool>(progress + quality + 2 * (sim.MaxLength - state.Step), perfectSolution);
            }
        }
    }
}
