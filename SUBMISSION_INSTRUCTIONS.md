# Entelect Grand Prix Submission Guidelines

This project implements the solution for the **Entelect Grand Prix** (F1-inspired race simulation). Submissions must follow these strict guidelines to be valid.

---

## 1. Submission Components

Each submission consists of exactly two parts:
1. **ZIP Archive:** A compressed zip file containing the complete source code of the solver.
2. **Output Text File:** A `.txt` file containing the serialized JSON output representing the generated race strategy.

---

## 2. Determinism Requirement

The solution **must be completely deterministic**. 
* During validation, the submitted source code will be executed on the validation track inputs.
* The output generated during validation must **exactly match** the submitted `.txt` file.
* Any non-deterministic behavior (e.g., relying on random seeds, system times, uninitialized variables, or parallel execution order differences) that causes discrepancies will invalidate the submission.

---

## 3. Strategy Output Format (JSON)

The output `.txt` file must contain a valid JSON object with the following schema:

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
          "target_m/s": 90.0,
          "brake_start_m_before_next": 174.9199
        },
        {
          "id": 2,
          "type": "corner"
        }
      ],
      "pit": {
        "enter": false
      }
    }
  ]
}
```

### Properties Reference
* **`initial_tyre_id`** (integer): The unique identifier of the tyre set that the car will start the race with.
* **`laps`** (array): Array of lap objects representing the strategy for each lap.
  * **`lap`** (integer): The 1-based lap index.
  * **`segments`** (array): Array of segment strategy actions.
    * **`id`** (integer): Segment ID on the track.
    * **`type`** (string): Type of segment (`"straight"` or `"corner"`).
    * **`target_m/s`** (double, straights only): Target speed the car will attempt to reach and maintain on this straight.
    * **`brake_start_m_before_next`** (double, straights only): Distance (in meters) *before the end* of the straight at which the car must start decelerating.
  * **`pit`** (object): Pit stop action at the end of the lap.
    * **`enter`** (boolean): Whether to enter the pit lane at the end of this lap.
    * **`tyre_change_set_id`** (integer, optional): The unique ID of the new tyre set to switch to.
    * **`fuel_refuel_amount_l`** (double, optional): The volume of fuel to refuel in liters.

---

## 4. Run & Build Instructions

Compile the C# solution:
```bash
dotnet build
```

Run the solver on a specific problem input:
```bash
dotnet bin/Debug/net10.0/2026.dll -i problems/1.txt
```

The output strategy JSON will be automatically saved to `problems/1_output.txt`.
