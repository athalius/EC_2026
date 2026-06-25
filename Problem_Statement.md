# Entelect Grand Prix: F1-Inspired Race Simulation Problem

Welcome to the **Entelect Grand Prix**! This document provides a comprehensive summary of the race simulation rules, assumptions, formulas, and data structures outlined in the official problem statement.

---

## 1. Goal & Overview

As the race strategist, your objective is to develop a deterministic program that generates a race strategy for a car. This strategy must optimize the car's speed around a racetrack layout while managing fuel, tyre wear, and weather changes.

Your program must output a JSON configuration file specifying:
* **Initial tyre ID** to start the race.
* **Target speed** on each straight segment.
* **Braking point** along each straight segment (distance in meters before the next segment).
* **Pit stop decisions** (when to enter, refuel volume, and tyre changes) at the end of each lap.

---

## 2. Assumptions & Core Rules

1. **Constant Acceleration/Deceleration:**
   * The car accelerates on straights at a constant rate defined by `accel_m/se2` in the JSON level file.
   * The car decelerates (brakes) on straights at a constant rate defined by `brake_m/se2` in the JSON level file.
   * Both acceleration and deceleration rates are affected by weather multipliers.
2. **Speed Limits & Constraints:**
   * The car's speed cannot exceed `max_speed_m/s`.
   * Entering a corner too quickly causes a crash (see *Crawl Mode*).
   * If a target speed for a straight is slower than the car's entry speed, the car will just maintain the entry speed.
3. **Corner Speed:**
   * The car's speed is constant throughout the entire corner segment. No acceleration or deceleration occurs during a corner.
4. **Limp Mode (Out of Fuel / Tyre Blowout):**
   * Triggered if the car runs out of fuel (fuel = 0) OR a tyre blowout occurs (tyre lifespan/life hits 0).
   * In Limp Mode, the car travels at a slow constant speed (`limp_constant_m/s`) with no acceleration or deceleration.
   * Limp Mode persists for the current segment and all subsequent segments until a pit stop is made to resolve the issue.
5. **Crawl Mode (Crash):**
   * Triggered if the car enters a corner exceeding the **Maximum Allowed Corner Speed**.
   * The car veers off-track, crashes, suffers a time penalty (`corner_crash_penalty_s`), and takes a flat **0.1 tyre degradation penalty**.
   * The car travels at a slow constant speed (`crawl_constant_m/s`) for the remainder of the corner.
   * Since acceleration only occurs on straights, the car will continue to travel at crawl speed for any subsequent corners until a straight is encountered.
6. **Race Start:**
   * The car starts the race at **0 m/s**.
7. **Pit Stops:**
   * The pit lane is only accessible at the end of a lap (it is not a track segment).
   * After a pit stop, the car exits the pit lane at `pit_exit_speed_m/s`.
8. **SI Units:** All calculations and parameters use SI units (meters, seconds, liters, meters per second).

---

## 3. Physics & Calculations

### Acceleration / Deceleration Time
The time ($t$) required to transition between an initial speed ($v_i$) and a final speed ($v_f$) at a constant rate ($a$) is:

$$t = \frac{|v_f - v_i|}{a}$$

### Corner Safety Speed Limit
The maximum speed ($v_{\text{max\_corner}}$) at which a corner of radius ($r$) can be safely taken depends on the tyre's current friction coefficient:

$$v_{\text{max\_corner}} = \sqrt{\text{tyre\_friction} \times g \times r}$$

*Where $g$ is the gravity constant (usually $9.8\ \text{m/s}^2$).*

### Distance Formulas
* **If final speed is known:**
  $$d = \frac{v_f^2 - v_i^2}{2 \times a}$$
* **If elapsed time is known:**
  $$d = v_i \times t + 0.5 \times a \times t^2$$

---

## 4. Tyres & Degradation

There are 5 tyre compounds available: **Soft**, **Medium**, **Hard**, **Intermediate**, and **Wet**.

### Tyre Properties Table

| Property / Compound | Soft | Medium | Hard | Intermediate | Wet |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Base Friction Coefficient** | 1.8 | 1.7 | 1.6 | 1.2 | 1.1 |
| **Dry Multiplier** | 1.18 | 1.08 | 0.98 | 0.90 | 0.72 |
| **Cold Multiplier** | 1.00 | 0.97 | 0.92 | 0.96 | 0.88 |
| **Light Rain Multiplier** | 0.92 | 0.88 | 0.82 | 1.08 | 1.02 |
| **Heavy Rain Multiplier** | 0.80 | 0.74 | 0.68 | 1.02 | 1.20 |
| **Dry Degradation Rate** | 0.14 | 0.10 | 0.07 | 0.11 | 0.16 |
| **Cold Degradation Rate** | 0.11 | 0.08 | 0.06 | 0.09 | 0.12 |
| **Light Rain Degradation Rate** | 0.12 | 0.09 | 0.07 | 0.08 | 0.09 |
| **Heavy Rain Degradation Rate** | 0.13 | 0.10 | 0.08 | 0.09 | 0.05 |

### Tyre Friction Calculation
Tyre friction at any point is determined by its degradation state and the active weather multiplier:

$$\text{tyre\_friction} = (\text{base\_friction\_coefficient} - \text{total\_degradation}) \times \text{weather\_multiplier}$$

### Tyre Degradation Formulas

Degradation is accumulated separately for straights, braking, and corners:

| Degradation Type | Constant ($K$) | Formula |
| :--- | :---: | :--- |
| **Straights** | $K_{\text{STRAIGHT}} = 0.0000166$ | $\text{Degradation} = \text{tyre\_degradation\_rate} \times \text{segment\_length} \times K_{\text{STRAIGHT}}$ |
| **Braking** | $K_{\text{BRAKING}} = 0.0398$ | $\text{Degradation} = \left[ \left(\frac{v_i}{100}\right)^2 - \left(\frac{v_f}{100}\right)^2 \right] \times K_{\text{BRAKING}} \times \text{tyre\_degradation\_rate}$ |
| **Corners** | $K_{\text{CORNER}} = 0.000265$ | $\text{Degradation} = K_{\text{CORNER}} \times \frac{v_{\text{corner}}^2}{\text{radius}} \times \text{tyre\_degradation\_rate}$ |

---

## 5. Fuel & Refueling

Fuel consumption is speed-dependent. Faster speeds burn more fuel.

### Fuel Consumption Formula

$$F_{\text{used}} = \left( K_{\text{base}} + K_{\text{drag}} \times \bar{v}^2 \right) \times \text{distance}$$

*Where:*
* $\bar{v} = \frac{v_i + v_f}{2}$ (average speed over the distance in m/s)
* $K_{\text{base}} = 0.0005\ \text{l/m}$
* $K_{\text{drag}} = 0.0000000015\ \text{l/m}$

### Refueling Time
$$\text{refuel\_time (s)} = \frac{\text{amount\_to\_refuel (L)}}{\text{refuel\_rate (L/s)}}$$

---

## 6. Pit Stops

At the end of each lap, the car can enter the pit lane to change tyres, refuel, or both.

$$\text{pit\_stop\_time (s)} = \text{refuel\_time} + \text{pit\_tyre\_swap\_time} + \text{base\_pit\_stop\_time}$$

*   **Refuel Time:** calculated as above based on refuel amount and refuel rate.
*   **Pit Tyre Swap Time:** flat time to change tyres (defined in JSON).
*   **Base Pit Stop Time:** overhead time for entering/exiting the pits (defined in JSON).

---

## 7. Weather

Weather changes dynamically during the race at specified intervals:
*   **Conditions:** Dry, Cold, Light Rain, Heavy Rain.
*   **Cycling:** If the race time exceeds the sum of all weather condition durations, the weather cycle repeats from the first condition.
*   **Impact:** Modifies acceleration (`acceleration_multiplier`), deceleration (`deceleration_multiplier`), and tyre performance (friction and degradation multipliers).

---

## 8. Progression Levels

*   **Level 1: Basic Navigation**
    *   Focus on track navigation, target speeds, and braking points.
    *   No tyre degradation.
*   **Level 2: Fuel Management**
    *   Adds fuel capacity limitations and refueling pit stops.
    *   Introduces a **soft cap** fuel allowance.
*   **Level 3: Dynamic Weather**
    *   Weather changes during the race.
    *   Requires selecting correct tyres and adapting speeds based on track wetness/temperature.
*   **Level 4: Tyre Degradation**
    *   Full tyre wear physics and compound degradation rates.
    *   Requires managing a limited set of available tyres and strategically pitting to prevent blowouts.

---

## 9. Scoring Formulas

*   **Level 1 Scoring:**
    $$\text{Score} = \frac{1,000,000,000}{\text{Total Time}}$$

*   **Level 2 & 3 Scoring:**
    $$\text{Fuel Bonus} = -1,000,000 \times \left(1 - \frac{\text{fuel\_used}}{\text{fuel\_soft\_cap\_limit}}\right)^2 + 1,000,000$$
    $$\text{Score} = \text{Base Score} + \text{Fuel Bonus}$$

*   **Level 4 Scoring:**
    $$\text{Tyre Bonus} = 100,000 \times \sum(\text{tyre\_degradation}) - 50,000 \times \text{number\_of\_blowouts}$$
    $$\text{Score} = \text{Base Score} + \text{Fuel Bonus} + \text{Tyre Bonus}$$

---

## 10. Input / Output Data Structures

### Input JSON Structure Example (Level 4)
```json
{
 "car": {
   "max_speed_m/s": 90,
   "accel_m/se2": 10,
   "brake_m/se2": 20,
   "limp_constant_m/s": 20,
   "crawl_constant_m/s": 10,
   "fuel_tank_capacity_l": 150.0,
   "initial_fuel_l": 150.0,
   "fuel_consumption_l/m": 0.0005
 },
 "race": {
   "name": "Entelect GP Level 0",
   "laps": 2,
   "base_pit_stop_time_s": 20.0,
   "pit_tyre_swap_time_s": 10.0,
   "pit_refuel_rate_l/s": 5.0,
   "corner_crash_penalty_s": 10.0,
   "pit_exit_speed_m/s": 20.0,
   "fuel_soft_cap_limit_l": 1400.0,
   "starting_weather_condition_id": 1
 },
 "track": {
   "name": "Neo Kyalami Example",
   "segments": [
     {"id": 1, "type": "straight", "length_m": 850},
     {"id": 2, "type": "corner", "radius_m": 60, "length_m": 120},
     {"id": 3, "type": "straight", "length_m": 850},
     {"id": 4, "type": "corner", "radius_m": 60, "length_m": 120},
     {"id": 5, "type": "corner", "radius_m": 45, "length_m": 90},
     {"id": 6, "type": "corner", "radius_m": 80, "length_m": 140},
     {"id": 7, "type": "straight", "length_m": 650},
     {"id": 8, "type": "corner", "radius_m": 80, "length_m": 140}
   ]
 },
 "tyres": {
   "properties": {
     "Soft": {
       "life_span": 1,
       "dry_friction_multiplier": 1.18,
       "cold_friction_multiplier": 1.00,
       "light_rain_friction_multiplier": 0.92,
       "heavy_rain_friction_multiplier": 0.80,
       "dry_degradation": 0.14,
       "cold_degradation": 0.11,
       "light_rain_degradation": 0.12,
       "heavy_rain_degradation": 0.13
     },
     "Medium": { ... },
     "Hard": { ... },
     "Intermediate": { ... },
     "Wet": { ... }
   },
   "available_sets": [
     { "ids": [1, 2, 3], "compound": "Soft" },
     { "ids": [4, 5, 6], "compound": "Medium" },
     { "ids": [7, 8, 9], "compound": "Hard" },
     { "ids": [10, 11, 12], "compound": "Intermediate" },
     { "ids": [13, 14, 15], "compound": "Wet" }
   ]
 },
 "weather": {
   "conditions": [
     {
       "id": 1,
       "condition": "cold",
       "duration_s": 1000.0,
       "acceleration_multiplier": 0.95,
       "deceleration_multiplier": 0.95
     },
     {
       "id": 2,
       "condition": "light_rain",
       "duration_s": 3000.0,
       "acceleration_multiplier": 0.80,
       "deceleration_multiplier": 0.80
     }
   ]
 }
}
```

### Submission Output JSON Structure (.txt format)
```json
{
 "initial_tyre_id": 1,
 "laps": [
   {
     "lap": 1,
     "segments": [
       {
         "id": 1,
         "type": "straight",
         "target_m/s": 70,
         "brake_start_m_before_next": 800
       },
       {
         "id": 2,
         "type": "corner"
       },
       {
         "id": 3,
         "type": "straight",
         "target_m/s": 50,
         "brake_start_m_before_next": 500
       },
       {
         "id": 4,
         "type": "corner"
       },
       {
         "id": 5,
         "type": "corner"
       },
       {
         "id": 6,
         "type": "corner"
       },
       {
         "id": 7,
         "type": "straight",
         "target_m/s": 60,
         "brake_start_m_before_next": 500
       },
       {
         "id": 8,
         "type": "corner"
       }
     ],
     "pit": {
       "enter": false
     }
   },
   {
     "lap": 2,
     "segments": [
       ...
     ],
     "pit": {
       "enter": true,
       "tyre_change_set_id": 3,
       "fuel_refuel_amount_l": 20
     }
   }
 ]
}
```
