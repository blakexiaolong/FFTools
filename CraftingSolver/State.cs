﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CraftingSolver
{
    public class State
    {
        public Simulator Simulator { get; set; }
        public int Step { get; set; }
        public int LastStep { get; set; }
        public Action Action { get; set; } // the action leading to this state
        public double Durability { get; set; }
        public double CP { get; set; }
        public int BonusMaxCP { get; set; }
        public double Quality { get; set; }
        public double Progress { get; set; }
        public int WastedActions { get; set; }
        public int TrickUses { get; set; }
        public int NameOfElementUses { get; set; }
        public double Reliability { get; set; }
        public ICondition Condition { get; set; }
        public List<Effect> CountUps { get; set; }
        public List<Effect> CountDowns { get; set; }
        public List<Effect> Indefinites { get; set; }

        public int IQ { get; set; }
        public int Control { get; set; }
        public int QualityGain { get; set; }
        public int BProgressGain { get; set; }
        public int BQualityGain { get; set; }
        public bool Success => Progress >= Simulator.Recipe.Difficulty;

        public State()
        {
            CountDowns = new List<Effect>();
            CountUps = new List<Effect>();
            Indefinites = new List<Effect>();
        }

        public State Clone()
        {
            return new State
            {
                Simulator = Simulator,
                Step = Step,
                LastStep = LastStep,
                Action = Action,
                Durability = Durability,
                CP = CP,
                BonusMaxCP = BonusMaxCP,
                Quality = Quality,
                Progress = Progress,
                WastedActions = WastedActions,
                TrickUses = TrickUses,
                NameOfElementUses = NameOfElementUses,
                Reliability = Reliability,
                CountUps = CountUps.Select(x => x).ToList(),
                CountDowns = CountDowns.Select(x => x).ToList(),
                Indefinites = Indefinites.Select(x => x).ToList(),
                Condition = Condition,

                IQ = IQ,
                Control = Control,
                QualityGain = QualityGain,
                BProgressGain = BProgressGain,
                BQualityGain = BQualityGain
            };
        }

        public double CalcNameOfElementsBonus()
        {
            double percentComplete = Math.Floor((double)Progress / Simulator.Recipe.Difficulty * 100);
            double bonus = 2 * (100 - percentComplete) / 100;
            return Math.Min(2, Math.Max(0, bonus));
        }

        public bool UseConditionalAction()
        {
            if (CP > 0 && Condition.CheckGoodOrExcellent())
            {
                TrickUses += 1;
                return true;
            }
            else
            {
                WastedActions += 1;
                return false;
            }
        }
        public void ApplySpecialActionEffects(Action action)
        {
            if (action == Atlas.Actions.MastersMend)
            {
                Durability += 30;
            }

            if (CountDowns.Any(x => x.Action == Atlas.Actions.Manipulation && Durability > 0 && action != Atlas.Actions.Manipulation))
            {
                Durability += 5;
            }

            if (action == Atlas.Actions.ByregotsBlessing)
            {
                if (CountUps.Any(x => x.Action == Atlas.Actions.InnerQuiet))
                {
                    CountUps.RemoveAll(x => x.Action == Atlas.Actions.InnerQuiet);
                }
                else
                {
                    WastedActions += 1;
                }
            }

            if (action == Atlas.Actions.Reflect)
            {
                if (Step == 1)
                {
                    Effect iq = CountUps.FirstOrDefault(x => x.Action == Atlas.Actions.InnerQuiet);
                    if (iq == default)
                    {
                        CountUps.Add(new Effect { Action = Atlas.Actions.InnerQuiet, Turns = 2 });
                    }
                    else
                    {
                        iq.Turns = 2;
                    }
                }
                else
                {
                    WastedActions += 1;
                }
            }

            if (action.QualityIncreaseMultiplier > 0 && CountDowns.Any(x => x.Action == Atlas.Actions.GreatStrides))
            {
                CountDowns.RemoveAll(x => x.Action == Atlas.Actions.GreatStrides);
            }

            if (action.OnExcellent || action.OnGood)
            {
                if (UseConditionalAction())
                {
                    if (action == Atlas.Actions.TricksOfTheTrade)
                    {
                        CP += 20 * Condition.PGoodOrExcellent();
                    }
                }
            }

            if (action == Atlas.Actions.Veneration && CountDowns.Any(x => x.Action == Atlas.Actions.Veneration))
            {
                WastedActions += 1;
            }
            if (action == Atlas.Actions.Innovation && CountDowns.Any(x => x.Action == Atlas.Actions.Innovation))
            {
                WastedActions += 1;
            }
        }

        public void UpdateEffectCounters(Action action, double successProbability)
        {
            foreach(Effect effect in CountDowns)
            {
                effect.Turns -= 1;
            }
            CountDowns.RemoveAll(x => x.Turns == 0);

            if (CountUps.Any(x => x.Action == Atlas.Actions.InnerQuiet))
            {
                // conditional IQ countups
                Effect iq = CountUps.First(x => x.Action == Atlas.Actions.InnerQuiet);
                if (action == Atlas.Actions.PatientTouch)
                {
                    iq.Turns = Convert.ToInt32((iq.Turns * 2 * successProbability) + (iq.Turns / 2 * (1 - successProbability)));

                }
                else if (action == Atlas.Actions.PreparatoryTouch)
                {
                    iq.Turns += 2;
                }
                else if (action == Atlas.Actions.PreciseTouch && Condition.CheckGoodOrExcellent())
                {
                    iq.Turns += 2 * successProbability * Condition.PGoodOrExcellent();
                }
                else if (action.QualityIncreaseMultiplier > 0 && action != Atlas.Actions.Reflect)
                {
                    iq.Turns += Convert.ToInt32(1 * successProbability);
                }

                iq.Turns = Math.Min(iq.Turns, 10);
            }

            switch (action.ActionType)
            {
                case "countup":
                    Effect countup = CountUps.FirstOrDefault(x => x.Action == action);
                    if (countup == default)
                    {
                        CountUps.Add(new Effect { Action = action, Turns = 0 });
                    }
                    else
                    {
                        countup.Turns = 0;
                    }
                    break;
                case "indefinite":
                    Indefinites.Add(new Effect { Action = action, Turns = 1 });
                    break;
                case "countdown":
                    if (action == Atlas.Actions.NameOfTheElements)
                    {
                        if (NameOfElementUses == 0)
                        {
                            Effect nameOfBuff = CountDowns.FirstOrDefault(x => x.Action == Atlas.Actions.NameOfTheElements);
                            if (nameOfBuff == default)
                            {
                                CountDowns.Add(new Effect { Action = Atlas.Actions.NameOfTheElements, Turns = action.ActiveTurns });
                            }
                            else
                            {
                                nameOfBuff.Turns = action.ActiveTurns;
                            }
                            
                            NameOfElementUses += 1;
                        }
                        else
                        {
                            WastedActions += 1;
                        }
                    }
                    else
                    {
                        Effect countdown = CountDowns.FirstOrDefault(x => x.Action == action);
                        if (countdown == default)
                        {
                            CountDowns.Add(new Effect { Action = action, Turns = action.ActiveTurns });
                        }
                        else
                        {
                            countdown.Turns = action.ActiveTurns;
                        }
                    }
                    break;
                case "immediate":
                    break;
                default:
                    throw new InvalidOperationException($"Action Type {action.ActionType} was unrecognized");
            }

        }        
        public void UpdateState(Action action, double progressGain, double qualityGain, double durabilityCost, int cpCost, double successProbability)
        {
            Progress += progressGain;
            Quality += qualityGain;
            Durability -= durabilityCost;
            CP -= cpCost;
            LastStep += 1;

            ApplySpecialActionEffects(action);
            UpdateEffectCounters(action, successProbability);

            // Sanity Checking
            if (Durability >= -5 && Progress >= Simulator.Recipe.Difficulty)
            {
                Durability = 0;
            }
            Durability = Math.Min(Durability, Simulator.Recipe.Durability);
            CP = Math.Min(CP, Simulator.Crafter.CP + BonusMaxCP);
        }
    
        public StateViolations CheckViolations()
        {
            return new StateViolations
            {
                ProgressOk = Progress >= Simulator.Recipe.Difficulty,
                CpOk = CP >= 0,
                DurabilityOk = Durability >= 0,
                TrickOk = TrickUses <= Simulator.MaxTrickUses,
                ReliabilityOk = Reliability >= Simulator.ReliabilityIndex
            };
        }
    }
}