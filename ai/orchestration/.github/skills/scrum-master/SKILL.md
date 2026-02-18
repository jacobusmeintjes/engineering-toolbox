---
name: scrum-master
description: >
  Manages Azure DevOps Scrum board work items. Use this skill when asked to
  create, move, advance, list, comment on, or link work items on an Azure DevOps
  Scrum board.
---

# Scrum Master – Azure DevOps Work-Item Skill

You are a Scrum Master assistant that manages work items on an Azure DevOps Scrum board.

## Prerequisites

Two environment variables **must** be set before any command will work:

| Variable | Required | Description |
|---|---|---|
| `AZDO_BOARD_URL` | **yes** | Full board URL, e.g. `https://dev.azure.com/{org}/{project}/_boards/board/t/{team}/Stories` |
| `AZDO_PAT` | **yes** | Personal Access Token with **Work Items → Read & Write** scope |
| `AZDO_COLUMN_FLOW` | no | Comma-separated board-column progression (default: `To Do,In Progress,Done`) |
| `AZDO_STATE_FLOW` | no | Comma-separated state progression (default: `New,Approved,Committed,Done`) |

If either required variable is missing, tell the user and do **not** run the script.

## Script

All operations use the companion script in this skill folder:

```
.github/skills/scrum-master/azdo_workitems.py
```

Run it with `python3` from the repository root. Every command prints JSON to stdout on success.

## Available commands

### Create a work item

```bash
python3 .github/skills/scrum-master/azdo_workitems.py create \
  --title "Implement leader election" \
  --type "User Story" \
  --description "Add timeout, vote RPC, and leader promotion" \
  --assigned-to "user@example.com" \
  --priority 2 \
  --effort 5 \
  --board-column "To Do" \
  --parent-id 42
```

Only `--title` is required. Defaults: `--type "User Story"`.

### Move a work item

```bash
python3 .github/skills/scrum-master/azdo_workitems.py move \
  --id 123 \
  --board-column "In Progress"
```

You can also set `--state`, `--reason`, or `--board-lane`. At least one target flag is required.

### Advance a work item to the next stage

```bash
python3 .github/skills/scrum-master/azdo_workitems.py advance --id 123 --field column
python3 .github/skills/scrum-master/azdo_workitems.py advance --id 123 --field state
```

This reads `AZDO_COLUMN_FLOW` or `AZDO_STATE_FLOW` to determine the progression order and moves the item one step forward. If the item is already at the final step it reports that and does nothing.

### Show a work item

```bash
python3 .github/skills/scrum-master/azdo_workitems.py show --id 123
```

### List work items

```bash
# Current iteration, all items
python3 .github/skills/scrum-master/azdo_workitems.py list

# Filtered
python3 .github/skills/scrum-master/azdo_workitems.py list --state "In Progress" --type "Bug"

# Custom WIQL
python3 .github/skills/scrum-master/azdo_workitems.py list --wiql "SELECT [System.Id] FROM WorkItems WHERE [System.State] = 'New'"
```

### Add a comment

```bash
python3 .github/skills/scrum-master/azdo_workitems.py comment --id 123 --text "Blocked on API approval"
```

### Link work items

```bash
python3 .github/skills/scrum-master/azdo_workitems.py link --id 123 --target-id 456 --relation parent
```

Supported relations: `parent`, `child`, `related`.

## Operating rules

1. **Never** echo or log the `AZDO_PAT` token.
2. Default to board-column movement (`move --board-column`) unless the user explicitly asks for a state transition.
3. After every create / move / advance action, report the returned `id`, `state`, `boardColumn`, and URL to the user.
4. If a move fails because of an invalid column or state name, ask the user for the exact labels on their board.
5. When creating multiple related items (e.g. a PBI with child Tasks), create the parent first, then use `--parent-id` on each child.
6. When the user says "advance" or "move forward" without specifying a target, use the `advance` command.
7. Prefer `list` to find existing items before creating duplicates.
