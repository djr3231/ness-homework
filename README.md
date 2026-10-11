# Project Gaia

Project Gaia is a small full-stack operations application built as a software
development interview exercise. Part A exposes a .NET Web API and a React
client for addition, subtraction, string concatenation, and string containment.

The project deliberately favors a small, explainable architecture over
production-scale infrastructure. Its main learning goals are API design,
dependency boundaries, validation and error handling, OpenAPI-based client
generation, and observable client/server behavior.

## Technology Stack

- .NET 10, C# 14, and ASP.NET Core controllers
- Built-in ASP.NET Core OpenAPI generation and Scalar API reference
- React 19, TypeScript 6, and Vite 8
- Tailwind CSS 4 for the minimal client presentation
- Hey API `@hey-api/openapi-ts` for generated TypeScript DTOs, endpoint
  functions, and Fetch transport

## Repository Structure

```text
server/
  Gaia.Operations/  Operation contracts, dispatch, results, and implementations
  Gaia.Api/         HTTP controllers, DTOs, dependency registration, and errors
client/             React client and generated OpenAPI client
docs/superpowers/   Approved design and step-by-step implementation plan
AGENTS.md            Pair-programming and Git working agreement
```

The dependency direction is intentionally one-way:

```text
React client --HTTP/OpenAPI--> Gaia.Api --> Gaia.Operations
```

`Gaia.Operations` has no dependency on ASP.NET Core. `Gaia.Api` translates HTTP
requests and responses but delegates operation behavior to the operations
library. The React client has no compile-time reference to either .NET project;
OpenAPI is the language-neutral contract between them.

## Architecture

Each operation implements `IBinaryOperation`, which supplies a stable key, a
display name, and an `Execute(left, right)` method. The built-in operations are:

| Key | Display name | Behavior |
|---|---|---|
| `add` | Addition | Adds two invariant-culture decimal values |
| `subtract` | Subtraction | Subtracts the second decimal from the first |
| `concatenate` | Concatenate | Joins two strings without changing them |
| `contains` | Contains | Performs an ordinal, case-sensitive containment check |

`OperationExecutor` is the central dispatch boundary. It receives all
registered operations, indexes them by key, exposes their metadata, and routes
each calculation to the selected implementation. Controllers therefore do not
contain operation-specific branches. Unknown keys and invalid operands become
stable operation failures that the API returns as `400 ProblemDetails`.
Unexpected exceptions are logged on the server and returned to clients as a
generic `500 ProblemDetails` response.

The API exposes:

- `GET /api/operations`
- `POST /api/calculations`

The React client loads the operation catalog instead of hard-coding it. It
sends both operands as strings and lets the selected server operation decide
how to interpret them.

### Adding an Operation

1. Add a class under `server/Gaia.Operations/BuiltIn/` that implements
   `IBinaryOperation`.
2. Give it a unique key, display name, and execution behavior.
3. Register it as an `IBinaryOperation` in `server/Gaia.Api/Program.cs`.
4. Build and repeat the verification matrix.

No change is required in `OperationExecutor`, either controller, or the React
operation selector. Regenerate the TypeScript client only when the HTTP
contract itself changes.

## Prerequisites

- .NET 10 SDK
- Node.js 22.18 or newer
- npm
- Git, if the development history will be reviewed

## Build and Run

### API

From the repository root:

```powershell
Set-Location server
dotnet restore Gaia.slnx
dotnet build Gaia.slnx
dotnet run --project Gaia.Api
```

### Client

In a second terminal, create the local environment file once and start Vite:

```powershell
Set-Location client
npm install
Copy-Item .env.example .env.local
npm run dev
```

`.env.local` is intentionally ignored by Git. Its development value is:

```dotenv
VITE_API_BASE_URL=http://localhost:5050
```

### Local URLs

| Service | URL |
|---|---|
| React client | `http://localhost:5173` |
| API | `http://localhost:5050` |
| Scalar API reference | `http://localhost:5050/scalar` |
| OpenAPI document | `http://localhost:5050/openapi/v1.json` |

The development API allows the exact Vite origin
`http://localhost:5173`. CORS is not opened globally.

## Regenerate the TypeScript API Client

Building `Gaia.Api` creates
`server/Gaia.Api/openapi/gaia-api.json`. Hey API reads that document and writes
the generated TypeScript client under `client/src/api/generated/`.

```powershell
Set-Location server
dotnet build Gaia.slnx
Set-Location ..\client
npm run generate:api
npm run build
```

Generated files are committed so the API contract is visible in Git. They
should not be edited manually. The handwritten `client/src/api/client.ts`
module configures the generated Fetch client and provides the boundary used by
React components.

## Development Process and AI Collaboration

This assignment explicitly permits AI assistance. The project was developed
as a deliberate pair-programming and learning process with OpenAI Codex, with
understanding and reviewability treated as part of the result.

I remained the primary implementer: I made the final design decisions, wrote
the application code, ran the project commands, exercised the API and browser
flows, and retained responsibility for every submitted change. Codex supported
requirements analysis, alternative design discussions, small-step planning,
conceptual explanations, code review, documentation, and routine local Git
work under the agreed repository rules, with explicit approval before every
commit.

The process is inspectable through:

- the [Part A design](docs/superpowers/specs/2026-07-29-project-gaia-part-a-design.md);
- the [Part A implementation plan](docs/superpowers/plans/2026-07-29-project-gaia-part-a-implementation.md);
- the pair-programming and Git rules in [AGENTS.md](AGENTS.md);
- the incremental Conventional Commit history.

Review the development sequence with:

```powershell
git log --graph --oneline --decorate --all
```

The README is not intended to hide AI involvement or present generated output
as independent work. The goal is to make both the technical result and the
candidate's active participation easy to evaluate.

## Verification

Part A uses builds, linting, OpenAPI regeneration, and real browser/API checks.
No automated test suite is claimed.

### Static and Contract Checks

- [ ] `dotnet build server/Gaia.slnx`
- [ ] From `client/`, `npm run generate:api` produces no unexpected generated
      diff
- [ ] From `client/`, `npm run lint`
- [ ] From `client/`, `npm run build`
- [ ] The required `A34D` marker exists in `OperationExecutor.cs`

### Manual End-to-End Matrix

| Operation | Field A | Field B | Expected result |
|---|---|---|---|
| Addition | `12.5` | `7.5` | `20` |
| Subtraction | `10` | `3` | `7` |
| Concatenate | `Project ` | `Gaia` | `Project Gaia` |
| Contains | `Project Gaia` | `Gaia` | `true` |
| Addition | `abc` | `2` | Readable validation error |

Also verify:

- [ ] Calculate cannot start overlapping requests.
- [ ] Refresh restores a valid initial state.
- [ ] The browser console contains no errors.

These boxes are completed only after the final verification run against the
actual API and Vite client.

## Part A Boundary

Part A is independently runnable and contains no database. SQL Server, Entity
Framework Core, operation history, the three most recent operations of the same
type, and the current-month operation count are deliberately deferred to the
optional Part B. Part B requires a separate design and implementation cycle.
