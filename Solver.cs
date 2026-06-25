using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hackathon.Utils;

namespace Hackathon
{
    // JSON Data Structures for Input
    public class CarConfig
    {
        [JsonPropertyName("max_speed_m/s")]
        public double max_speed_m_s { get; set; }

        [JsonPropertyName("accel_m/se2")]
        public double accel_m_se2 { get; set; }

        [JsonPropertyName("brake_m/se2")]
        public double brake_m_se2 { get; set; }

        [JsonPropertyName("limp_constant_m/s")]
        public double limp_constant_m_s { get; set; }

        [JsonPropertyName("crawl_constant_m/s")]
        public double crawl_constant_m_s { get; set; }

        public double fuel_tank_capacity_l { get; set; }
        public double initial_fuel_l { get; set; }

        [JsonPropertyName("fuel_consumption_l/m")]
        public double fuel_consumption_l_m { get; set; }
    }

    public class RaceConfig
    {
        public string name { get; set; } = "";
        public int laps { get; set; }
        public double base_pit_stop_time_s { get; set; }
        public double pit_tyre_swap_time_s { get; set; }

        [JsonPropertyName("pit_refuel_rate_l/s")]
        public double pit_refuel_rate_l_s { get; set; }

        public double corner_crash_penalty_s { get; set; }

        [JsonPropertyName("pit_exit_speed_m/s")]
        public double pit_exit_speed_m_s { get; set; }

        public double fuel_soft_cap_limit_l { get; set; }
        public int starting_weather_condition_id { get; set; }
        public double time_reference_s { get; set; }
    }

    public class TrackSegment
    {
        public int id { get; set; }
        public string type { get; set; } = ""; // "straight" or "corner"
        public double length_m { get; set; }
        public double radius_m { get; set; } // Only for corners
    }

    public class TrackConfig
    {
        public string name { get; set; } = "";
        public List<TrackSegment> segments { get; set; } = new();
    }

    public class TyreProperties
    {
        public double life_span { get; set; }
        public double base_friction { get; set; }
        public double dry_friction_multiplier { get; set; }
        public double cold_friction_multiplier { get; set; }
        public double light_rain_friction_multiplier { get; set; }
        public double heavy_rain_friction_multiplier { get; set; }
        public double dry_degradation { get; set; }
        public double cold_degradation { get; set; }
        public double light_rain_degradation { get; set; }
        public double heavy_rain_degradation { get; set; }
    }

    public class TyresConfig
    {
        public Dictionary<string, TyreProperties> properties { get; set; } = new();
    }

    public class AvailableSet
    {
        public List<int> ids { get; set; } = new();
        public string compound { get; set; } = "";
    }

    public class WeatherCondition
    {
        public int id { get; set; }
        public string condition { get; set; } = "";
        public double duration_s { get; set; }
        public double acceleration_multiplier { get; set; }
        public double deceleration_multiplier { get; set; }
    }

    public class WeatherConfig
    {
        public List<WeatherCondition> conditions { get; set; } = new();
    }

    public class ProblemInput
    {
        public CarConfig car { get; set; } = new();
        public RaceConfig race { get; set; } = new();
        public TrackConfig track { get; set; } = new();
        public TyresConfig tyres { get; set; } = new();
        public List<AvailableSet> available_sets { get; set; } = new();
        public WeatherConfig weather { get; set; } = new();
    }

    // JSON Data Structures for Output Strategy
    public class OutputSegment
    {
        public int id { get; set; }
        public string type { get; set; } = "";

        [JsonPropertyName("target_m/s")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? target_m_s { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? brake_start_m_before_next { get; set; }
    }

    public class OutputPit
    {
        public bool enter { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? tyre_change_set_id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? fuel_refuel_amount_l { get; set; }
    }

    public class OutputLap
    {
        public int lap { get; set; }
        public List<OutputSegment> segments { get; set; } = new();
        public OutputPit pit { get; set; } = new();
    }

    public class OutputStrategy
    {
        public int initial_tyre_id { get; set; }
        public List<OutputLap> laps { get; set; } = new();
    }

    public class SimulationResult
    {
        public double total_time { get; set; }
        public double fuel_used { get; set; }
        public double score { get; set; }
        public int blowouts { get; set; }
        public bool completed { get; set; }
        public OutputStrategy strategy { get; set; } = new();
    }

    public static class Solver
    {
        public static (string Part1, string Part2) Solve(string[] lines)
        {
            // Fallback for default template runs if input is not JSON
            string content = string.Join("\n", lines).Trim();
            if (!content.StartsWith("{"))
            {
                return (content.Length.ToString(), "0");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            ProblemInput input;
            try
            {
                input = JsonSerializer.Deserialize<ProblemInput>(content, options)
                        ?? throw new Exception("Deserialization returned null.");
            }
            catch (Exception ex)
            {
                return ($"Error parsing input: {ex.Message}", "");
            }

            // Find optimal strategy by parameter sweep
            double bestScore = double.MinValue;
            SimulationResult bestResult = null;

            // Get available compounds in the track config
            var dryCompounds = input.available_sets
                .Select(s => s.compound)
                .Where(c => c.Equals("Soft", System.StringComparison.OrdinalIgnoreCase) ||
                            c.Equals("Medium", System.StringComparison.OrdinalIgnoreCase) ||
                            c.Equals("Hard", System.StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToArray();
            if (dryCompounds.Length == 0)
            {
                dryCompounds = new[] { "Soft" };
            }

            // Target speed scaling alphas
            var alphas = new[] { 1.0, 0.98, 0.96, 0.94, 0.92, 0.90, 0.88, 0.86, 0.84, 0.82, 0.80, 0.78, 0.76, 0.75, 0.70 };
            
            // Tyre degradation wear limit threshold beta
            var betas = new[] { 0.80, 0.85, 0.90 };

            foreach (var dryCompound in dryCompounds)
            {
                foreach (var alpha in alphas)
                {
                    foreach (var beta in betas)
                    {
                        var result = SimulatePolicy(input, alpha, beta, dryCompound);
                        if (result.completed && result.score > bestScore)
                        {
                            bestScore = result.score;
                            bestResult = result;
                        }
                    }
                }
            }

            if (bestResult == null)
            {
                return ("Error: No valid strategy found.", "");
            }

            var writeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string outputJson = JsonSerializer.Serialize(bestResult.strategy, writeOptions);
            var firstStraight = bestResult.strategy.laps[0].segments.FirstOrDefault(s => s.type == "straight");
            double? targetSpeed = firstStraight?.target_m_s;
            return (outputJson, $"Best Score: {bestResult.score:F2} (Time: {bestResult.total_time:F2}s, Fuel: {bestResult.fuel_used:F2}L, Al: {targetSpeed})");
        }

        private static WeatherCondition GetWeatherAtTime(ProblemInput input, double t)
        {
            var conditions = input.weather.conditions;
            if (conditions == null || conditions.Count == 0)
            {
                return new WeatherCondition
                {
                    id = 1,
                    condition = "dry",
                    duration_s = 100000.0,
                    acceleration_multiplier = 1,
                    deceleration_multiplier = 1
                };
            }

            double totalDuration = conditions.Sum(c => c.duration_s);
            double offset = t % totalDuration;

            double accum = 0;
            foreach (var c in conditions)
            {
                accum += c.duration_s;
                if (offset < accum)
                {
                    return c;
                }
            }
            return conditions.Last();
        }

        private static TyreProperties GetTyreProperties(ProblemInput input, string compound)
        {
            if (input.tyres.properties.TryGetValue(compound, out var props))
            {
                return props;
            }
            return new TyreProperties
            {
                life_span = 1,
                base_friction = 1.8,
                dry_friction_multiplier = 1.18,
                dry_degradation = 0.14
            };
        }

        private static (double frictionMultiplier, double degradationRate) GetWeatherTyreFactors(TyreProperties props, string condition)
        {
            switch (condition.ToLower())
            {
                case "dry":
                    return (props.dry_friction_multiplier, props.dry_degradation);
                case "cold":
                    return (props.cold_friction_multiplier, props.cold_degradation);
                case "light_rain":
                case "light rain":
                    return (props.light_rain_friction_multiplier, props.light_rain_degradation);
                case "heavy_rain":
                case "heavy rain":
                    return (props.heavy_rain_friction_multiplier, props.heavy_rain_degradation);
                default:
                    return (props.dry_friction_multiplier, props.dry_degradation);
            }
        }

        private static string GetBestCompoundForWeather(string condition, string dryCompoundPreference)
        {
            switch (condition.ToLower())
            {
                case "light_rain":
                case "light rain":
                    return "Intermediate";
                case "heavy_rain":
                case "heavy rain":
                    return "Wet";
                default:
                    return dryCompoundPreference; // Soft, Medium, or Hard
            }
        }

        private static int GetUnusedTyreId(ProblemInput input, string compound, HashSet<int> usedIds)
        {
            var set = input.available_sets.FirstOrDefault(s => s.compound.Equals(compound, System.StringComparison.OrdinalIgnoreCase));
            if (set != null)
            {
                foreach (int id in set.ids)
                {
                    if (!usedIds.Contains(id))
                    {
                        return id;
                    }
                }
            }
            if (set != null && set.ids.Count > 0) return set.ids[0];
            return 1;
        }

        private static SimulationResult SimulatePolicy(ProblemInput input, double alpha, double beta, string dryCompound)
        {
            double K_base = 0.0005;
            double K_drag = 0.0000000015;

            bool enableDegradation = input.race.name.Contains("Level 4");
            bool enableFuel = !input.race.name.Contains("Level 1");

            int n = input.track.segments.Count;
            double totalTrackLength = input.track.segments.Sum(s => s.length_m);

            var usedTyreIds = new HashSet<int>();

            // Initial weather lookup
            var initWeather = GetWeatherAtTime(input, 0);
            string initCompound = GetBestCompoundForWeather(initWeather.condition, dryCompound);
            int activeTyreId = GetUnusedTyreId(input, initCompound, usedTyreIds);
            usedTyreIds.Add(activeTyreId);

            double time = 0;
            double fuel = input.car.initial_fuel_l;
            double tyreDegradation = 0;
            string tyreCompound = initCompound;

            var accumulatedDegradations = new List<double>();
            int blowouts = 0;
            bool isLimp = false;
            double totalFuelUsed = 0;

            var strategy = new OutputStrategy
            {
                initial_tyre_id = activeTyreId
            };

            double currentSpeed = 0; // Starts from 0 m/s

            for (int lapNum = 1; lapNum <= input.race.laps; lapNum++)
            {
                var lap = new OutputLap
                {
                    lap = lapNum,
                    pit = new OutputPit { enter = false }
                };

                // Track and weather state at start of lap
                double lapStartTime = time;
                var startWeather = GetWeatherAtTime(input, lapStartTime);
                var tyreProps = GetTyreProperties(input, tyreCompound);
                var (fricMult, degRate) = GetWeatherTyreFactors(tyreProps, startWeather.condition);
                
                double activeFriction = (tyreProps.base_friction - tyreDegradation) * fricMult;
                if (activeFriction < 0) activeFriction = 0;

                // 1. Calculate corner limits
                double[] cornerLimits = new double[n];
                for (int i = 0; i < n; i++)
                {
                    if (input.track.segments[i].type == "corner")
                    {
                        double r = input.track.segments[i].radius_m;
                        cornerLimits[i] = Math.Sqrt(activeFriction * 9.8 * r);
                    }
                    else
                    {
                        cornerLimits[i] = input.car.max_speed_m_s;
                    }
                }

                // Contiguous corner limit propagation
                bool[] visited = new bool[n];
                for (int i = 0; i < n; i++)
                {
                    if (input.track.segments[i].type == "corner" && !visited[i])
                    {
                        var group = new List<int>();
                        int curr = i;
                        while (input.track.segments[curr].type == "corner" && !visited[curr])
                        {
                            visited[curr] = true;
                            group.Add(curr);
                            curr = (curr + 1) % n;
                        }
                        int prev = (i - 1 + n) % n;
                        while (input.track.segments[prev].type == "corner" && !visited[prev])
                        {
                            visited[prev] = true;
                            group.Add(prev);
                            prev = (prev - 1 + n) % n;
                        }

                        double minLimit = double.MaxValue;
                        foreach (var idx in group)
                        {
                            if (cornerLimits[idx] < minLimit) minLimit = cornerLimits[idx];
                        }
                        foreach (var idx in group)
                        {
                            cornerLimits[idx] = minLimit;
                        }
                    }
                }

                // 2. Backward propagation of safe entry speed limits
                double activeDecel = input.car.brake_m_se2 * startWeather.deceleration_multiplier;
                for (int iter = 0; iter < n * 2; iter++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (input.track.segments[i].type == "straight")
                        {
                            int prevIdx = (i - 1 + n) % n;
                            int nextIdx = (i + 1) % n;
                            double vEndLimit = cornerLimits[nextIdx];
                            double maxEntrySpeed = Math.Sqrt(vEndLimit * vEndLimit + 2 * activeDecel * input.track.segments[i].length_m);
                            if (cornerLimits[prevIdx] > maxEntrySpeed)
                            {
                                cornerLimits[prevIdx] = maxEntrySpeed;
                            }
                        }
                    }
                }

                // 3. Simulate segments
                for (int i = 0; i < n; i++)
                {
                    var seg = input.track.segments[i];
                    var outSeg = new OutputSegment
                    {
                        id = seg.id,
                        type = seg.type
                    };

                    var segWeather = GetWeatherAtTime(input, time);
                    double segAccel = input.car.accel_m_se2 * segWeather.acceleration_multiplier;
                    double segDecel = input.car.brake_m_se2 * segWeather.deceleration_multiplier;
                    var (_, segDegRate) = GetWeatherTyreFactors(tyreProps, segWeather.condition);

                    if (seg.type == "straight")
                    {
                        double vStart = (lapNum == 1 && i == 0) ? 0 : currentSpeed;
                        int nextIdx = (i + 1) % n;
                        double vEndLimit = cornerLimits[nextIdx];

                        double L = seg.length_m;
                        double vPeakSq = (2 * segAccel * segDecel / (segAccel + segDecel)) * (L + (vStart * vStart) / (2 * segAccel) + (vEndLimit * vEndLimit) / (2 * segDecel));
                        double vPeak = Math.Sqrt(vPeakSq);

                        double vTargetMax = Math.Min(vPeak, input.car.max_speed_m_s);
                        double vTarget = vTargetMax * alpha;
                        if (vTarget < vStart)
                        {
                            vTarget = vStart;
                        }

                        double dBrake = (vTarget * vTarget - vEndLimit * vEndLimit) / (2 * segDecel);
                        if (dBrake < 0) dBrake = 0;

                        double timeUsed = 0;
                        double fuelUsed = 0;
                        double degUsed = 0;

                        if (isLimp)
                        {
                            double limpSpeed = input.car.limp_constant_m_s;
                            timeUsed = L / limpSpeed;
                            fuelUsed = enableFuel ? (K_base + K_drag * limpSpeed * limpSpeed) * L : 0;
                            degUsed = enableDegradation ? segDegRate * L * 0.0000166 : 0;
                            currentSpeed = limpSpeed;
                            
                            outSeg.target_m_s = Math.Round(limpSpeed, 4);
                            outSeg.brake_start_m_before_next = 0;
                        }
                        else
                        {
                            double dAccel = (vTarget * vTarget - vStart * vStart) / (2 * segAccel);
                            if (dAccel < 0) dAccel = 0;
                            double dDecel = dBrake;
                            double dFlat = L - dAccel - dDecel;

                            if (dAccel > 0)
                            {
                                double tAccel = (vTarget - vStart) / segAccel;
                                timeUsed += tAccel;
                                fuelUsed += enableFuel ? (K_base + K_drag * Math.Pow((vStart + vTarget)/2, 2)) * dAccel : 0;
                                degUsed += enableDegradation ? segDegRate * dAccel * 0.0000166 : 0;
                            }
                            if (dFlat > 0)
                            {
                                double tFlat = dFlat / vTarget;
                                timeUsed += tFlat;
                                fuelUsed += enableFuel ? (K_base + K_drag * vTarget * vTarget) * dFlat : 0;
                                degUsed += enableDegradation ? segDegRate * dFlat * 0.0000166 : 0;
                            }
                            if (dDecel > 0)
                            {
                                double tDecel = (vTarget - vEndLimit) / segDecel;
                                timeUsed += tDecel;
                                fuelUsed += enableFuel ? (K_base + K_drag * Math.Pow((vTarget + vEndLimit)/2, 2)) * dDecel : 0;
                                degUsed += enableDegradation ? ((Math.Pow(vTarget/100, 2) - Math.Pow(vEndLimit/100, 2)) * 0.0398 * segDegRate) : 0;
                            }

                            currentSpeed = vEndLimit;
                            outSeg.target_m_s = Math.Round(vTarget, 4);
                            outSeg.brake_start_m_before_next = Math.Round(dBrake, 4);
                        }

                        time += timeUsed;
                        if (enableFuel)
                        {
                            fuel -= fuelUsed;
                            totalFuelUsed += fuelUsed;
                            if (fuel <= 0)
                            {
                                fuel = 0;
                                isLimp = true;
                            }
                        }
                        if (enableDegradation)
                        {
                            tyreDegradation += degUsed;
                            if (tyreDegradation >= 1.0)
                            {
                                tyreDegradation = 1.0;
                                if (!isLimp)
                                {
                                    isLimp = true;
                                    blowouts++;
                                }
                            }
                        }
                    }
                    else // Corner segment
                    {
                        double timeUsed = 0;
                        double fuelUsed = 0;
                        double degUsed = 0;

                        double vMax = Math.Sqrt(activeFriction * 9.8 * seg.radius_m);
                        double vIn = currentSpeed;

                        if (isLimp)
                        {
                            double limpSpeed = input.car.limp_constant_m_s;
                            timeUsed = seg.length_m / limpSpeed;
                            fuelUsed = enableFuel ? (K_base + K_drag * limpSpeed * limpSpeed) * seg.length_m : 0;
                            degUsed = enableDegradation ? (0.000265 * (limpSpeed * limpSpeed / seg.radius_m) * segDegRate) : 0;
                            currentSpeed = limpSpeed;
                        }
                        else if (vIn > vMax)
                        {
                            // Corner crash!
                            time += input.race.corner_crash_penalty_s;
                            if (enableDegradation)
                            {
                                tyreDegradation += 0.1;
                                if (tyreDegradation >= 1.0)
                                {
                                    tyreDegradation = 1.0;
                                    isLimp = true;
                                    blowouts++;
                                }
                            }
                            double crawlSpeed = input.car.crawl_constant_m_s;
                            timeUsed = seg.length_m / crawlSpeed;
                            fuelUsed = enableFuel ? (K_base + K_drag * crawlSpeed * crawlSpeed) * seg.length_m : 0;
                            degUsed = enableDegradation ? (0.000265 * (crawlSpeed * crawlSpeed / seg.radius_m) * segDegRate) : 0;
                            currentSpeed = crawlSpeed;
                        }
                        else
                        {
                            timeUsed = seg.length_m / vIn;
                            fuelUsed = enableFuel ? (K_base + K_drag * vIn * vIn) * seg.length_m : 0;
                            degUsed = enableDegradation ? (0.000265 * (vIn * vIn / seg.radius_m) * segDegRate) : 0;
                            currentSpeed = vIn;
                        }

                        time += timeUsed;
                        if (enableFuel)
                        {
                            fuel -= fuelUsed;
                            totalFuelUsed += fuelUsed;
                            if (fuel <= 0)
                            {
                                fuel = 0;
                                isLimp = true;
                            }
                        }
                        if (enableDegradation)
                        {
                            tyreDegradation += degUsed;
                            if (tyreDegradation >= 1.0)
                            {
                                tyreDegradation = 1.0;
                                if (!isLimp)
                                {
                                    isLimp = true;
                                    blowouts++;
                                }
                            }
                        }
                    }

                    lap.segments.Add(outSeg);
                }

                // End of lap: decision to pit stop
                if (lapNum < input.race.laps)
                {
                    double nextLapStartTime = time;
                    var nextWeather = GetWeatherAtTime(input, nextLapStartTime);
                    string nextBestCompound = GetBestCompoundForWeather(nextWeather.condition, dryCompound);

                    // Estimate fuel for next lap
                    double estLapFuel = totalTrackLength * (K_base + K_drag * Math.Pow(input.car.max_speed_m_s * alpha, 2));

                    bool needRefuel = enableFuel && (fuel < estLapFuel * 1.2);
                    bool needTyreChange = enableDegradation && (tyreDegradation > beta || tyreCompound != nextBestCompound);
                    bool weatherTyreMismatch = tyreCompound != nextBestCompound;

                    if (needRefuel || needTyreChange || weatherTyreMismatch || isLimp)
                    {
                        lap.pit.enter = true;
                        double refuelAmount = 0;

                        if (enableFuel)
                        {
                            int remainingLaps = input.race.laps - lapNum;
                            double fuelNeeded = remainingLaps * estLapFuel - fuel + 5.0; // 5L safety buffer
                            refuelAmount = Math.Max(0.0, Math.Min(input.car.fuel_tank_capacity_l - fuel, fuelNeeded));
                            lap.pit.fuel_refuel_amount_l = Math.Round(refuelAmount, 4);
                        }

                        if (enableDegradation || weatherTyreMismatch || isLimp)
                        {
                            string newCompound = nextBestCompound;
                            int newTyreId = GetUnusedTyreId(input, newCompound, usedTyreIds);
                            usedTyreIds.Add(newTyreId);
                            lap.pit.tyre_change_set_id = newTyreId;

                            accumulatedDegradations.Add(tyreDegradation);
                            tyreDegradation = 0;
                            tyreCompound = newCompound;
                            tyreProps = GetTyreProperties(input, tyreCompound);
                        }

                        double refuelTime = refuelAmount / input.race.pit_refuel_rate_l_s;
                        double tyreSwapTime = lap.pit.tyre_change_set_id != null ? input.race.pit_tyre_swap_time_s : 0;
                        double pitTime = refuelTime + tyreSwapTime + input.race.base_pit_stop_time_s;

                        time += pitTime;
                        fuel += refuelAmount;
                        isLimp = false;
                        currentSpeed = input.race.pit_exit_speed_m_s;
                    }
                }
                else
                {
                    accumulatedDegradations.Add(tyreDegradation);
                }

                strategy.laps.Add(lap);
            }

            double baseScore = 1000000000.0 / time;
            double fuelBonus = 0;
            double tyreBonus = 0;

            if (enableFuel)
            {
                double ratio = totalFuelUsed / input.race.fuel_soft_cap_limit_l;
                fuelBonus = -1000000.0 * Math.Pow(1.0 - ratio, 2) + 1000000.0;
            }
            if (enableDegradation)
            {
                double sumDeg = accumulatedDegradations.Sum();
                tyreBonus = 100000.0 * sumDeg - 50000.0 * blowouts;
            }

            double finalScore = baseScore + fuelBonus + tyreBonus;

            return new SimulationResult
            {
                total_time = time,
                fuel_used = totalFuelUsed,
                score = finalScore,
                blowouts = blowouts,
                completed = !isLimp,
                strategy = strategy
            };
        }
    }
}
