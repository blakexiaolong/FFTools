﻿using System.Collections.Generic;

namespace CraftingSolver
{
    public static class Atlas
    {
        public static class Actions
        {
            public static Action Observe = new Action
            {
                ShortName = "observe",
                Name = "Observe",
                DurabilityCost = 0,
                CPCost = 7,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 13,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action BasicSynth = new Action
            {
                ShortName = "basicSynth",
                Name = "Basic Synthesis",
                DurabilityCost = 10,
                CPCost = 0,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 1.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 1,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action BasicSynth2 = new Action
            {
                ShortName = "basicSynth2",
                Name = "Basic Synthesis 2",
                DurabilityCost = 10,
                CPCost = 0,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 1.2,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 31,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action CarefulSynthesis = new Action
            {
                ShortName = "carefulSynthesis",
                Name = "Careful Synthesis",
                DurabilityCost = 10,
                CPCost = 7,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 1.5,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 62,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action RapidSynthesis = new Action
            {
                ShortName = "rapidSynthesis",
                Name = "Rapid Synthesis",
                DurabilityCost = 10,
                CPCost = 0,
                SuccessProbability = 0.5,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 2.5,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 9,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action FlawlessSynthesis = new Action
            {
                ShortName = "flawlessSynthesis",
                Name = "Flawless Synthesis",
                DurabilityCost = 10,
                CPCost = 15,
                SuccessProbability = 0.9,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 1.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 37,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action BasicTouch = new Action
            {
                ShortName = "basicTouch",
                Name = "Basic Touch",
                DurabilityCost = 10,
                CPCost = 18,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 1.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 5,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action StandardTouch = new Action
            {
                ShortName = "standardTouch",
                Name = "Standard Touch",
                DurabilityCost = 10,
                CPCost = 32,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 1.25,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 18,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action HastyTouch = new Action
            {
                ShortName = "hastyTouch",
                Name = "Hasty Touch",
                DurabilityCost = 10,
                CPCost = 0,
                SuccessProbability = 0.6,
                QualityIncreaseMultiplier = 1.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 9,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action ByregotsBlessing = new Action
            {
                ShortName = "byregotsBlessing",
                Name = "Byregot's Blessing",
                DurabilityCost = 10,
                CPCost = 24,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 1.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 50,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action MastersMend = new Action
            {
                ShortName = "mastersMend",
                Name = "Master's Mend",
                DurabilityCost = 0,
                CPCost = 88,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 7,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action TricksOfTheTrade = new Action
            {
                ShortName = "tricksOfTheTrade",
                Name = "Tricks of the Trade",
                DurabilityCost = 0,
                CPCost = 0,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 13,
                OnGood = true,
                OnExcellent = true,
                OnPoor = false
            };
            public static Action InnerQuiet = new Action
            {
                ShortName = "innerQuiet",
                Name = "Inner Quiet",
                DurabilityCost = 0,
                CPCost = 18,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "countup",
                ActiveTurns = 1,
                Class = "All",
                Level = 11,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action Manipulation = new Action
            {
                ShortName = "manipulation",
                Name = "Manipulation",
                DurabilityCost = 0,
                CPCost = 96,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "countdown",
                ActiveTurns = 8,
                Class = "All",
                Level = 65,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action WasteNot = new Action
            {
                ShortName = "wasteNot",
                Name = "Waste Not",
                DurabilityCost = 0,
                CPCost = 56,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "countdown",
                ActiveTurns = 4,
                Class = "All",
                Level = 15,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action WasteNot2 = new Action
            {
                ShortName = "wasteNot2",
                Name = "Waste Not II",
                DurabilityCost = 0,
                CPCost = 98,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "countdown",
                ActiveTurns = 8,
                Class = "All",
                Level = 47,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action Veneration = new Action
            {
                ShortName = "veneration",
                Name = "Veneration",
                DurabilityCost = 0,
                CPCost = 18,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "countdown",
                ActiveTurns = 4,
                Class = "All",
                Level = 15,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action Innovation = new Action
            {
                ShortName = "innovation",
                Name = "Innovation",
                DurabilityCost = 0,
                CPCost = 18,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "countdown",
                ActiveTurns = 4,
                Class = "All",
                Level = 26,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action GreatStrides = new Action
            {
                ShortName = "greatStrides",
                Name = "Great Strides",
                DurabilityCost = 0,
                CPCost = 32,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "countdown",
                ActiveTurns = 3,
                Class = "All",
                Level = 21,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action PreciseTouch = new Action
            {
                ShortName = "preciseTouch",
                Name = "Precise Touch",
                DurabilityCost = 10,
                CPCost = 18,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 1.5,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 53,
                OnGood = true,
                OnExcellent = true,
                OnPoor = false
            };
            public static Action MuscleMemory = new Action
            {
                ShortName = "muscleMemory",
                Name = "Muscle Memory",
                DurabilityCost = 10,
                CPCost = 6,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 3.0,
                ActionType = "countdown",
                ActiveTurns = 5,
                Class = "All",
                Level = 54,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action BrandOfTheElements = new Action
            {
                ShortName = "brandOfTheElements",
                Name = "Brand of the Elements",
                DurabilityCost = 10,
                CPCost = 6,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 1.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 37,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action NameOfTheElements = new Action
            {
                ShortName = "nameOfTheElements",
                Name = "Name of the Elements",
                DurabilityCost = 0,
                CPCost = 30,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "countdown",
                ActiveTurns = 3,
                Class = "All",
                Level = 37,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action RapidSynthesis2 = new Action
            {
                ShortName = "rapidSynthesis2",
                Name = "Rapid Synthesis",
                DurabilityCost = 10,
                CPCost = 0,
                SuccessProbability = 0.5,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 5.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 63,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action PatientTouch = new Action
            {
                ShortName = "patientTouch",
                Name = "Patient Touch",
                DurabilityCost = 10,
                CPCost = 6,
                SuccessProbability = 0.5,
                QualityIncreaseMultiplier = 1.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 64,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action PrudentTouch = new Action
            {
                ShortName = "prudentTouch",
                Name = "Prudent Touch",
                DurabilityCost = 5,
                CPCost = 25,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 1.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 66,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action FocusedSynthesis = new Action
            {
                ShortName = "focusedSynthesis",
                Name = "Focused Synthesis",
                DurabilityCost = 10,
                CPCost = 5,
                SuccessProbability = 0.5,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 2.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 67,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action FocusedTouch = new Action
            {
                ShortName = "focusedTouch",
                Name = "Focused Touch",
                DurabilityCost = 10,
                CPCost = 18,
                SuccessProbability = 0.5,
                QualityIncreaseMultiplier = 1.5,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 68,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action Reflect = new Action
            {
                ShortName = "reflect",
                Name = "Reflect",
                DurabilityCost = 10,
                CPCost = 24,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 1.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 69,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action PreparatoryTouch = new Action
            {
                ShortName = "preparatoryTouch",
                Name = "Preparatory Touch",
                DurabilityCost = 20,
                CPCost = 40,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 2.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 71,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action Groundwork = new Action
            {
                ShortName = "groundwork",
                Name = "Groundwork",
                DurabilityCost = 20,
                CPCost = 18,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 3.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 72,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action DelicateSynthesis = new Action
            {
                ShortName = "delicateSynthesis",
                Name = "Delicate Synthesis",
                DurabilityCost = 10,
                CPCost = 32,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 1.0,
                ProgressIncreaseMultiplier = 1.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 76,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action IntensiveSynthesis = new Action
            {
                ShortName = "intensiveSynthesis",
                Name = "Intensive Synthesis",
                DurabilityCost = 10,
                CPCost = 6,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 3.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 78,
                OnGood = true,
                OnExcellent = true,
                OnPoor = false
            };
            public static Action TrainedEye = new Action
            {
                ShortName = "trainedEye",
                Name = "Trained Eye",
                DurabilityCost = 10,
                CPCost = 250,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 80,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };
            public static Action DummyAction = new Action
            {
                ShortName = "dummyAction",
                Name = "______________",
                DurabilityCost = 0,
                CPCost = 0,
                SuccessProbability = 1.0,
                QualityIncreaseMultiplier = 0.0,
                ProgressIncreaseMultiplier = 0.0,
                ActionType = "immediate",
                ActiveTurns = 1,
                Class = "All",
                Level = 1,
                OnGood = false,
                OnExcellent = false,
                OnPoor = false
            };

            public static Action[] AllActions = new Action[]
            {
                Observe,
                BasicSynth,
                CarefulSynthesis,
                RapidSynthesis,
                FlawlessSynthesis,
                BasicTouch,
                StandardTouch,
                HastyTouch,
                ByregotsBlessing,
                MastersMend,
                TricksOfTheTrade,
                InnerQuiet,
                Manipulation,
                WasteNot,
                WasteNot2,
                Veneration,
                Innovation,
                GreatStrides,
                PreciseTouch,
                MuscleMemory,
                BrandOfTheElements,
                NameOfTheElements,
                RapidSynthesis2,
                PatientTouch,
                PrudentTouch,
                FocusedSynthesis,
                FocusedTouch,
                Reflect,
                PreparatoryTouch,
                Groundwork,
                DelicateSynthesis,
                IntensiveSynthesis,
                TrainedEye,
                DummyAction
            };
            public static Action[] DependableActions = new Action[]
            {
                Observe,
                BasicSynth,
                CarefulSynthesis,
                BasicTouch,
                StandardTouch,
                ByregotsBlessing,
                MastersMend,
                InnerQuiet,
                Manipulation,
                WasteNot,
                WasteNot2,
                Veneration,
                Innovation,
                GreatStrides,
                MuscleMemory,
                BrandOfTheElements,
                NameOfTheElements,
                PrudentTouch,
                FocusedSynthesis,
                FocusedTouch,
                Reflect,
                PreparatoryTouch,
                Groundwork,
                DelicateSynthesis,
                TrainedEye,
                DummyAction
            };
            public static Action[] FirstRoundActions = new Action[]
            {
                MuscleMemory,
                TrainedEye,
                Reflect
            };
        };
        public static Dictionary<int, int> LevelTable = new Dictionary<int, int>
        {
            { 51, 120 },
            { 52, 125 },
            { 53, 130 },
            { 54, 133 },
            { 55, 136 },
            { 56, 139 },
            { 57, 142 },
            { 58, 145 },
            { 59, 148 },
            { 60, 150 },
            { 61, 260 },
            { 62, 265 },
            { 63, 270 },
            { 64, 273 },
            { 65, 276 },
            { 66, 279 },
            { 67, 282 },
            { 68, 285 },
            { 69, 288 },
            { 70, 290 },
            { 71, 390 },
            { 72, 395 },
            { 73, 400 },
            { 74, 403 },
            { 75, 406 },
            { 76, 409 },
            { 77, 412 },
            { 78, 415 },
            { 79, 418 },
            { 80, 420 }
        };
        public static Dictionary<string, Dictionary<int, double>> LevelDifferenceFactors = new Dictionary<string, Dictionary<int, double>>
        {
            {
                "control", new Dictionary<int, double>
                {
                    { -30, 0.6 },
                    { -29, 0.64 },
                    { -28, 0.68 },
                    { -27, 0.72 },
                    { -26, 0.76 },
                    { -25, 0.80 },
                    { -24, 0.84 },
                    { -23, 0.88 },
                    { -22, 0.92 },
                    { -21, 0.96 },
                    { -20, 1 },
                    { -19, 1 },
                    { -18, 1 },
                    { -17, 1 },
                    { -16, 1 },
                    { -15, 1 },
                    { -14, 1 },
                    { -13, 1 },
                    { -12, 1 },
                    { -11, 1 },
                    { -10, 1 },
                    { -9, 1 },
                    { -8, 1 },
                    { -7, 1 },
                    { -6, 1 },
                    { -5, 1 },
                    { -4, 1 },
                    { -3, 1 },
                    { -2, 1 },
                    { -1, 1 },
                    { 0, 1 },
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 11, 1 },
                    { 12, 1 },
                    { 13, 1 },
                    { 14, 1 },
                    { 15, 1 },
                    { 16, 1 },
                    { 17, 1 },
                    { 18, 1 },
                    { 19, 1 },
                    { 20, 1 }
                }
            },
            {
                "craftsmanship", new Dictionary<int, double>
                {
                    { -30, 0.8 },
                    { -29, 0.82 },
                    { -28, 0.84 },
                    { -27, 0.86 },
                    { -26, 0.88 },
                    { -25, 0.90 },
                    { -24, 0.92 },
                    { -23, 0.94 },
                    { -22, 0.96 },
                    { -21, 0.98 },
                    { -20, 1 },
                    { -19, 1 },
                    { -18, 1 },
                    { -17, 1 },
                    { -16, 1 },
                    { -15, 1 },
                    { -14, 1 },
                    { -13, 1 },
                    { -12, 1 },
                    { -11, 1 },
                    { -10, 1 },
                    { -9, 1 },
                    { -8, 1 },
                    { -7, 1 },
                    { -6, 1 },
                    { -5, 1 },
                    { -4, 1 },
                    { -3, 1 },
                    { -2, 1 },
                    { -1, 1 },
                    { 0, 1 },
                    { 1, 1.05 },
                    { 2, 1.1 },
                    { 3, 1.15 },
                    { 4, 1.2 },
                    { 5, 1.25 },
                    { 6, 1.27 },
                    { 7, 1.29 },
                    { 8, 1.31 },
                    { 9, 1.33 },
                    { 10, 1.35 },
                    { 11, 1.37 },
                    { 12, 1.39 },
                    { 13, 1.41 },
                    { 14, 1.43 },
                    { 15, 1.45 },
                    { 16, 1.46 },
                    { 17, 1.47 },
                    { 18, 1.48 },
                    { 19, 1.49 },
                    { 20, 1.5 }
                }
            }
        };
    }
}
