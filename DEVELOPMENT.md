# DEVELOPMENT — fork notes (Rhino 7 line)

This is a personal fork of **jingcheng-chen/rhinomcp**, retargeted to **Rhino 7 /
.NET Framework 4.8**. It is the **Rhino 7** line of a two-repo setup:

| Line | Repo | Base | Target |
|------|------|------|--------|
| **Rhino 7** (this repo) | `ktkt40208/rhinomcp-r7` | jingcheng-chen/rhinomcp | net48 / Rhino 7 |
| **Rhino 8** | `ktkt40208/RhinoMCP` | mcneel/RhinoMCP | net8.0 / Rhino 8+ |

Why this fork exists: the official mcneel/RhinoMCP (the Rhino 8 line) is net8.0 and cannot run
on Rhino 7. jingcheng-chen/rhinomcp has a lighter architecture — a separate Python MCP server
talking over a TCP socket to a thin C# plugin — so only the **C# plugin** needs retargeting to
net48; the Python server is unchanged and version-agnostic.

## Where the work is

The retarget is on **`main`** (this fork's working line; `main` tracks our work, not a pristine
mirror of upstream — see "Syncing upstream" below). Changes vs the upstream fork point:

- `plugin/rhinomcp.csproj` — net8.0 → **net48**; RhinoCommon & Grasshopper 8.17 → **7.33**;
  dropped the net8-only `System.Drawing.Common` + `Microsoft.WindowsDesktop.App.Ref` NuGets in
  favour of framework references; added `Microsoft.NETFramework.ReferenceAssemblies` (lets net48
  build on a machine without a full .NET Framework dev pack, e.g. macOS); `LangVersion` 11.
- `plugin/Compat/IsExternalInit.cs` — polyfill so `record`/`init` compile on net48.
- `plugin/Compat/StringCompat.cs` — polyfill `string.Contains(string|char, StringComparison)`,
  absent on net48 (global-namespace extension; fixes 15 call sites in the Grasshopper code).
- `plugin/Serializers/Serializer.cs` — replaced the Rhino-8-only `Curve.ControlPolygon()` and
  `PolylineCurve.ToArray()` with R7-compatible NURBS/sampling and `Point(i)` loops.

Builds clean to `plugin/bin/Debug/net48/rhinomcp.rhp`. **Not yet runtime-tested in Rhino 7.**

## Build (macOS or Windows)

Any recent .NET SDK works (the `Microsoft.NETFramework.ReferenceAssemblies` package supplies the
net48 reference assemblies, so no Visual Studio / .NET Framework dev pack is required):

```bash
dotnet build plugin/rhinomcp.csproj
# output: plugin/bin/Debug/net48/rhinomcp.rhp

# install into the Rhino 7 plug-ins dir (macOS) in one step:
dotnet build plugin/rhinomcp.csproj -p:CopyToRhinoPluginDir=true -p:RhinoVersion=7.0
```

(If your only SDK is an old Homebrew `dotnet@8`, that's fine here — unlike the Rhino 8 repo, this
build has no Roslyn source generator, so the SDK version doesn't matter.)

## Run

1. In Rhino 7, register the built `.rhp` (drag-drop the first time), then run `mcpstart` — the
   plugin opens a TCP server on `127.0.0.1:1999`.
2. Start the Python MCP server (unchanged from upstream) and point your MCP client at it, e.g.
   `uvx rhinomcp`, or run the local `server/` per the repo README.

## Syncing upstream

**Pull-only** fork: `main` carries the R7 retarget; we don't PR back to jingcheng. The `upstream`
remote points at jingcheng-chen/rhinomcp; the tag `upstream-fork-point` marks the fork commit.

```bash
git fetch upstream
git merge upstream/main             # pull upstream's new tools/fixes into our R7 main
git diff upstream-fork-point..main  # the R7 retarget delta
```

Expect recurring conflicts in `plugin/rhinomcp.csproj` when merging (upstream is net8.0, we are
net48) — keep our net48/RhinoCommon-7 target and take upstream's other changes. New upstream tools
generally just work once the project still targets net48.

## Known issues to address on the R7 line

- **TCP message framing (upstream issue #31):** the bridge has no length-prefix framing, so two
  commands in one socket read can wedge the connection. The bundled single-socket Python client
  serialises requests so it rarely bites, but a robust fix exists in the `blazingphoenix7:fix/tcp-framing`
  branch (4-byte length prefix + backward-compat sniffing, with tests) — recommended to cherry-pick.
- **Stalled upstream fixes worth pulling:** `blazingphoenix7` PRs #28 (capture_viewport view-mutation),
  #29 (jsonschema RefResolver deprecation), #30 (wire validate_response).
- **`execute_rhinocommon_csharp_code`** uses Roslyn scripting (`Microsoft.CodeAnalysis.CSharp.Scripting`)
  — verify it behaves on .NET Framework 4.8 at runtime.

## Note on the tool surface

Unlike the Rhino 8 repo (where geometry tools were hand-ported onto mcneel), jingcheng's plugin
**already includes** the full typed tool surface (create/modify/analyze/undo/redo, layer &
attribute CRUD, and ~27 Grasshopper tools incl. `gh_build_graph`/`gh_mutate_graph`). After this
retarget they are available on Rhino 7 as-is — no re-porting needed.
