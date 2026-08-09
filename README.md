# Queue Management System Documentation

## Overview

This document describes the queue management system implementation.

### Features

-   Automatic ticket generation.
-   Department Prefix support.
-   Three reset modes:
    -   Daily
    -   By Shift
    -   Manual
-   Reset All counters.
-   Reset a specific Shift.
-   Reset a specific Department.

------------------------------------------------------------------------

# Department Prefix

Each department has a unique Prefix.

Example:

  Department    Prefix
  ------------- --------
  Grocery       A
  Frozen        B
  Electronics   C

Generated tickets:

``` text
A1
A2
A3

B1
B2
```

------------------------------------------------------------------------

# Ticket Number

Ticket number is generated as:

    Department Prefix + Counter

Example:

    A15

No leading zeros are used.

------------------------------------------------------------------------

# Reset Types

## Daily

The counter resets automatically every day.

## By Shift

Each shift contains:

-   Name
-   StartTime
-   Duration

Example:

  Shift     Start   Duration
  --------- ------- ----------
  Morning   08:00   08:00
  Evening   16:00   08:00
  Night     00:00   08:00

The counter resets automatically at the beginning of each shift.

## Manual

The counter continues until the administrator performs a reset.

------------------------------------------------------------------------

# New Properties

## Department

``` csharp
public string Prefix { get; set; }

public DateTimeOffset? LastResetAt { get; set; }
```

## Shift

``` csharp
public TimeSpan StartTime { get; set; }

public TimeSpan Duration { get; set; }

public DateTimeOffset? LastResetAt { get; set; }
```

## QueueSettings

``` csharp
public ResetType ResetType { get; set; }

public DateTimeOffset? LastGlobalResetAt { get; set; }
```

------------------------------------------------------------------------

# Reset Operations

## Reset All

Updates:

``` csharp
QueueSettings.LastGlobalResetAt
```

All departments and shifts start counting again.

------------------------------------------------------------------------

## Reset Shift

Updates:

``` csharp
Shift.LastResetAt
```

Only the selected shift starts again.

------------------------------------------------------------------------

## Reset Department

Updates:

``` csharp
Department.LastResetAt
```

Only the selected department starts again.

------------------------------------------------------------------------

# Ticket Generation Flow

1.  Read Queue Settings.
2.  Determine Reset Type.
3.  Calculate Reset Date.
4.  Find the latest ticket after the reset date.
5.  Extract the numeric part.
6.  Increment the counter.
7.  Generate the new ticket.

Example:

    A17 -> A18

------------------------------------------------------------------------

# Validation

The system validates:

-   Department exists.
-   Ticket Status exists.
-   Queue Settings exist.
-   Shift exists when ResetType = ByShift.
-   Prefix is unique.

------------------------------------------------------------------------

# Administrator Features

-   Create Departments.
-   Edit Departments.
-   Assign Prefix.
-   Create Shifts.
-   Edit Shifts.
-   Configure Queue Reset Type.
-   Reset All Counters.
-   Reset Shift Counter.
-   Reset Department Counter.

------------------------------------------------------------------------

# Advantages

-   Flexible queue numbering.
-   Independent counters per department.
-   Multiple reset strategies.
-   No data deletion during reset.
-   Full ticket history is preserved.
-   Easy to extend in the future.

------------------------------------------------------------------------

# ViewModels Updated

The following ViewModels were modified and therefore their
FluentValidation validators should also be updated.

## Department

### CreateDepartmentVM

New property added:

``` csharp
public string Prefix { get; set; } = string.Empty;
```

Validator should validate: - Name - Prefix - WarehouseId

### UpdateDepartmentVM

New property added:

``` csharp
public string Prefix { get; set; } = string.Empty;
```

Validator should validate: - Name - Prefix - WarehouseId

------------------------------------------------------------------------

## Shift

`StartDate` has been replaced with `StartTime`.

### CreateShiftVM

``` csharp
public TimeSpan StartTime { get; set; }
```

Validator should validate: - Name - StartTime - Duration

### UpdateShiftVM

``` csharp
public TimeSpan StartTime { get; set; }
```

Validator should validate: - Name - StartTime - Duration

------------------------------------------------------------------------

## Queue Settings

If QueueSettings ViewModels are used, validators should ensure:

-   ResetType is required.
-   ShiftId is required only when ResetType is `ByShift`.

------------------------------------------------------------------------

## Validators Updated

-   CreateDepartmentValidator
-   UpdateDepartmentValidator
-   CreateShiftValidator
-   UpdateShiftValidator
-   CreateQueueSettingsValidator (if exists)
-   UpdateQueueSettingsValidator (if exists)
