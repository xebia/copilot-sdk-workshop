# Final challenge: Are we ready to release?

**Time:** 10 minutes, optional. **Deck section:** Final challenge.

This is an extension after the three main labs, not a fourth required workshop lab.

## Goal

Give an agent these four capabilities and ask it for an evidence-based release recommendation:

```text
get_build_status(project)
get_tests(project)
get_open_pull_requests(repository)
get_security_findings(project)
```

The starter already obtains the tools from `ToolCatalog.ForTrack("release")`. Your task is to create the session, register and allowlist the tools, observe execution, and ask whether **checkout** is ready to release.

```bash
dotnet run --project labs/04-release-challenge/start
```

Do not encode the decision tree or provide the fixture's answer in the prompt. Ask the agent to check the evidence, explain blockers and identify uncertainty. This is a recommendation, not permission to deploy.

## Acceptance checklist

- A passing checkout build is not treated as sufficient.
- The recommendation identifies two failing tests.
- It identifies high-severity finding `SEC-17`.
- It notices blocking pull request #73.
- It recommends against release on the available evidence and explains next actions.
- It does not claim to run a release or modify any state.

## Bonus

Try payments. The tests pass and there are no known security findings, but PR #42 still blocks release.

## Solution

```bash
dotnet run --project labs/04-release-challenge/solution -- checkout
dotnet run --project labs/04-release-challenge/solution -- payments
```

In production, enforce your release policy in deterministic code, CI checks and human approvals. Do not substitute this LLM-generated recommendation for a release gate.
