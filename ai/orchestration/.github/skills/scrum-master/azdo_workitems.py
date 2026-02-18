#!/usr/bin/env python3
"""Azure DevOps work-item management script for the Scrum Master Copilot skill.

Environment variables
---------------------
AZDO_BOARD_URL  (required)  Full Azure DevOps board URL
                            e.g. https://dev.azure.com/{org}/{project}/_boards/board/t/{team}/Stories
AZDO_PAT        (required)  Personal Access Token with Work Items read/write scope

AZDO_COLUMN_FLOW (optional) Comma-separated board column progression (default: To Do,In Progress,Done)
AZDO_STATE_FLOW  (optional) Comma-separated state progression         (default: New,Approved,Committed,Done)
"""
from __future__ import annotations

import argparse
import base64
import json
import os
import sys
from dataclasses import dataclass
from typing import Any
from urllib import error, parse, request

API_VERSION = "7.1"

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

@dataclass
class AzdoContext:
    organization: str
    project: str
    team: str | None
    pat: str

    @property
    def base_url(self) -> str:
        return f"https://dev.azure.com/{self.organization}/{self.project}"


def _fail(message: str, code: int = 1) -> None:
    print(f"ERROR: {message}", file=sys.stderr)
    raise SystemExit(code)


def _parse_board_url(board_url: str) -> tuple[str, str, str | None]:
    """Extract org, project, and optional team from an Azure DevOps URL."""
    parsed = parse.urlparse(board_url)
    if not parsed.scheme or not parsed.netloc:
        _fail("AZDO_BOARD_URL must be a full URL (https://…).")

    host = parsed.netloc.lower()
    parts = [p for p in parsed.path.split("/") if p]
    if not parts:
        _fail("Could not parse organization/project from AZDO_BOARD_URL.")

    organization: str
    project: str
    team: str | None = None

    if host.endswith("dev.azure.com"):
        if len(parts) < 2:
            _fail("For dev.azure.com URLs expected /{organization}/{project}/…")
        organization, project = parts[0], parts[1]
    elif host.endswith("visualstudio.com"):
        organization = host.split(".")[0]
        project = parts[0]
    else:
        _fail("Unsupported host. Use dev.azure.com or *.visualstudio.com URLs.")
        return "", "", None  # unreachable; keeps mypy happy

    # Try to detect the team from a board URL like …/_boards/board/t/{team}/…
    if "_boards" in parts:
        idx = parts.index("_boards")
        if (
            len(parts) > idx + 3
            and parts[idx + 1] == "board"
            and parts[idx + 2] == "t"
        ):
            team = parse.unquote(parts[idx + 3])

    return organization, project, team


def _get_context() -> AzdoContext:
    board_url = os.environ.get("AZDO_BOARD_URL", "").strip()
    pat = os.environ.get("AZDO_PAT", "").strip()
    if not board_url:
        _fail("Environment variable AZDO_BOARD_URL is not set.")
    if not pat:
        _fail("Environment variable AZDO_PAT is not set.")
    org, proj, team = _parse_board_url(board_url)
    return AzdoContext(organization=org, project=proj, team=team, pat=pat)


def _auth_header(pat: str) -> str:
    token = base64.b64encode(f":{pat}".encode()).decode()
    return f"Basic {token}"


def _call(
    ctx: AzdoContext,
    method: str,
    path: str,
    *,
    body: Any | None = None,
    content_type: str = "application/json",
) -> Any:
    sep = "&" if "?" in path else "?"
    url = f"{ctx.base_url}{path}{sep}api-version={API_VERSION}"

    data: bytes | None = None
    if body is not None:
        data = json.dumps(body).encode()

    req = request.Request(url=url, method=method, data=data)
    req.add_header("Authorization", _auth_header(ctx.pat))
    req.add_header("Accept", "application/json")
    if data is not None:
        req.add_header("Content-Type", content_type)

    try:
        with request.urlopen(req) as resp:
            raw = resp.read().decode()
            return json.loads(raw) if raw else None
    except error.HTTPError as exc:
        detail = exc.read().decode(errors="replace")
        _fail(f"API {method} {path} → {exc.code}: {detail}")
    except error.URLError as exc:
        _fail(f"Network error: {exc.reason}")


def _print_json(obj: Any) -> None:
    print(json.dumps(obj, indent=2))


def _work_item_summary(item: dict) -> dict:
    fields = item.get("fields", {})
    assigned = fields.get("System.AssignedTo")
    return {
        "id": item.get("id"),
        "type": fields.get("System.WorkItemType"),
        "title": fields.get("System.Title"),
        "state": fields.get("System.State"),
        "boardColumn": fields.get("System.BoardColumn"),
        "boardLane": fields.get("System.BoardLane"),
        "assignedTo": (
            assigned.get("displayName") if isinstance(assigned, dict) else assigned
        ),
        "tags": fields.get("System.Tags"),
        "url": item.get("_links", {}).get("html", {}).get("href"),
    }

# ---------------------------------------------------------------------------
# Commands
# ---------------------------------------------------------------------------

def cmd_create(args: argparse.Namespace) -> None:
    """Create a new work item."""
    ctx = _get_context()
    wi_type = parse.quote(args.type)

    ops: list[dict[str, str]] = [
        {"op": "add", "path": "/fields/System.Title", "value": args.title},
    ]
    optional = {
        "System.Description": args.description,
        "System.AssignedTo": args.assigned_to,
        "System.AreaPath": args.area_path,
        "System.IterationPath": args.iteration_path,
        "System.Tags": args.tags,
        "System.BoardColumn": args.board_column,
        "System.State": args.state,
    }
    if args.priority:
        optional["Microsoft.VSTS.Common.Priority"] = str(args.priority)
    if args.effort:
        optional["Microsoft.VSTS.Scheduling.Effort"] = str(args.effort)
    if args.parent_id:
        # add parent relation
        ops.append(
            {
                "op": "add",
                "path": "/relations/-",
                "value": {
                    "rel": "System.LinkTypes.Hierarchy-Reverse",
                    "url": f"{ctx.base_url}/_apis/wit/workitems/{args.parent_id}",
                },
            }
        )
    for field, value in optional.items():
        if value:
            ops.append({"op": "add", "path": f"/fields/{field}", "value": value})

    result = _call(
        ctx, "POST", f"/_apis/wit/workitems/${wi_type}",
        body=ops, content_type="application/json-patch+json",
    )
    _print_json(_work_item_summary(result))


def cmd_move(args: argparse.Namespace) -> None:
    """Move a work item to a specific state / board column / board lane."""
    ctx = _get_context()
    if not args.state and not args.board_column and not args.board_lane:
        _fail("move requires at least one of --state, --board-column, or --board-lane.")

    ops: list[dict[str, str]] = []
    if args.state:
        ops.append({"op": "add", "path": "/fields/System.State", "value": args.state})
    if args.reason:
        ops.append({"op": "add", "path": "/fields/System.Reason", "value": args.reason})
    if args.board_column:
        ops.append({"op": "add", "path": "/fields/System.BoardColumn", "value": args.board_column})
    if args.board_lane:
        ops.append({"op": "add", "path": "/fields/System.BoardLane", "value": args.board_lane})

    result = _call(
        ctx, "PATCH", f"/_apis/wit/workitems/{args.id}",
        body=ops, content_type="application/json-patch+json",
    )
    _print_json(_work_item_summary(result))


def _get_work_item(ctx: AzdoContext, item_id: int) -> dict:
    return _call(ctx, "GET", f"/_apis/wit/workitems/{item_id}?$expand=Fields")


def cmd_advance(args: argparse.Namespace) -> None:
    """Advance a work item to the next step in the configured flow."""
    ctx = _get_context()
    field = args.field  # "column" or "state"

    env_key = "AZDO_COLUMN_FLOW" if field == "column" else "AZDO_STATE_FLOW"
    default = "To Do,In Progress,Done" if field == "column" else "New,Approved,Committed,Done"
    flow = [v.strip() for v in os.environ.get(env_key, default).split(",") if v.strip()]
    if len(flow) < 2:
        _fail(f"{env_key} must contain at least 2 comma-separated values.")

    item = _get_work_item(ctx, args.id)
    fields = item.get("fields", {})
    current = (
        fields.get("System.BoardColumn") if field == "column" else fields.get("System.State")
    )
    if current not in flow:
        _fail(
            f"Current {field} '{current}' is not in {env_key} ({', '.join(flow)}). "
            f"Set {env_key} to match your board."
        )
    idx = flow.index(current)
    if idx == len(flow) - 1:
        _print_json({"id": args.id, "message": f"Already at final {field}: {current}"})
        return

    next_val = flow[idx + 1]
    move_ns = argparse.Namespace(
        id=args.id,
        state=next_val if field == "state" else None,
        board_column=next_val if field == "column" else None,
        board_lane=None,
        reason=args.reason,
    )
    cmd_move(move_ns)


def cmd_show(args: argparse.Namespace) -> None:
    """Show key details of a single work item."""
    ctx = _get_context()
    item = _get_work_item(ctx, args.id)
    _print_json(_work_item_summary(item))


def cmd_list(args: argparse.Namespace) -> None:
    """Run a WIQL query to list work items (default: current iteration items)."""
    ctx = _get_context()

    if args.wiql:
        wiql = args.wiql
    else:
        # Default: items in the current iteration, optionally filtered by state
        clauses = [
            "SELECT [System.Id] FROM WorkItems",
            "WHERE [System.TeamProject] = @project",
            "AND [System.IterationPath] = @currentIteration",
        ]
        if args.state:
            clauses.append(f"AND [System.State] = '{args.state}'")
        if args.type:
            clauses.append(f"AND [System.WorkItemType] = '{args.type}'")
        if args.assigned_to:
            clauses.append(f"AND [System.AssignedTo] = '{args.assigned_to}'")
        clauses.append("ORDER BY [System.ChangedDate] DESC")
        wiql = " ".join(clauses)

    query_result = _call(ctx, "POST", "/_apis/wit/wiql", body={"query": wiql})
    ids = [wi["id"] for wi in query_result.get("workItems", [])]
    if not ids:
        _print_json({"items": [], "count": 0})
        return

    # Fetch details in batches of 200
    all_items: list[dict] = []
    for i in range(0, len(ids), 200):
        batch = ids[i : i + 200]
        id_csv = ",".join(str(x) for x in batch)
        batch_result = _call(
            ctx, "GET",
            f"/_apis/wit/workitems?ids={id_csv}&$expand=Fields",
        )
        all_items.extend(batch_result.get("value", []))

    summaries = [_work_item_summary(item) for item in all_items]
    _print_json({"items": summaries, "count": len(summaries)})


def cmd_comment(args: argparse.Namespace) -> None:
    """Add a comment to a work item."""
    ctx = _get_context()
    body = {"text": args.text}
    _call(ctx, "POST", f"/_apis/wit/workitems/{args.id}/comments", body=body)
    _print_json({"id": args.id, "message": "Comment added."})


def cmd_link(args: argparse.Namespace) -> None:
    """Link two work items together (e.g. parent-child, related)."""
    ctx = _get_context()
    rel_map = {
        "parent": "System.LinkTypes.Hierarchy-Reverse",
        "child": "System.LinkTypes.Hierarchy-Forward",
        "related": "System.LinkTypes.Related",
    }
    rel_type = rel_map.get(args.relation)
    if not rel_type:
        _fail(f"Unknown relation '{args.relation}'. Use: {', '.join(rel_map)}")

    ops = [
        {
            "op": "add",
            "path": "/relations/-",
            "value": {
                "rel": rel_type,
                "url": f"{ctx.base_url}/_apis/wit/workitems/{args.target_id}",
            },
        }
    ]
    result = _call(
        ctx, "PATCH", f"/_apis/wit/workitems/{args.id}",
        body=ops, content_type="application/json-patch+json",
    )
    _print_json({"id": args.id, "linkedTo": args.target_id, "relation": args.relation})


# ---------------------------------------------------------------------------
# CLI parser
# ---------------------------------------------------------------------------

def _build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        description="Azure DevOps work-item management for the Scrum Master skill.",
    )
    sp = p.add_subparsers(dest="command", required=True)

    # --- create ---
    c = sp.add_parser("create", help="Create a new work item")
    c.add_argument("--type", default="User Story", help="Work item type (default: User Story)")
    c.add_argument("--title", required=True, help="Title")
    c.add_argument("--description", help="HTML or plain-text description")
    c.add_argument("--assigned-to", help="Assignee (email or display name)")
    c.add_argument("--area-path", help="Area path")
    c.add_argument("--iteration-path", help="Iteration path")
    c.add_argument("--tags", help="Semicolon-separated tags")
    c.add_argument("--board-column", help="Initial board column")
    c.add_argument("--state", help="Initial state")
    c.add_argument("--priority", type=int, choices=[1, 2, 3, 4], help="Priority (1-4)")
    c.add_argument("--effort", type=float, help="Effort / story points")
    c.add_argument("--parent-id", type=int, help="Parent work item ID to link under")
    c.set_defaults(func=cmd_create)

    # --- move ---
    m = sp.add_parser("move", help="Move a work item to a state / column / lane")
    m.add_argument("--id", required=True, type=int, help="Work item ID")
    m.add_argument("--state", help="Target state (e.g. Committed)")
    m.add_argument("--reason", help="State-change reason")
    m.add_argument("--board-column", help="Target board column")
    m.add_argument("--board-lane", help="Target board lane")
    m.set_defaults(func=cmd_move)

    # --- advance ---
    a = sp.add_parser("advance", help="Advance to the next column/state in the configured flow")
    a.add_argument("--id", required=True, type=int, help="Work item ID")
    a.add_argument("--field", choices=["column", "state"], default="column", help="Advance by column or state")
    a.add_argument("--reason", help="Reason (when advancing by state)")
    a.set_defaults(func=cmd_advance)

    # --- show ---
    s = sp.add_parser("show", help="Show work item details")
    s.add_argument("--id", required=True, type=int, help="Work item ID")
    s.set_defaults(func=cmd_show)

    # --- list ---
    l = sp.add_parser("list", help="List work items (current iteration by default)")
    l.add_argument("--wiql", help="Custom WIQL query (overrides other filters)")
    l.add_argument("--state", help="Filter by state")
    l.add_argument("--type", help="Filter by work item type")
    l.add_argument("--assigned-to", help="Filter by assignee")
    l.set_defaults(func=cmd_list)

    # --- comment ---
    cm = sp.add_parser("comment", help="Add a comment to a work item")
    cm.add_argument("--id", required=True, type=int, help="Work item ID")
    cm.add_argument("--text", required=True, help="Comment text")
    cm.set_defaults(func=cmd_comment)

    # --- link ---
    lk = sp.add_parser("link", help="Link two work items")
    lk.add_argument("--id", required=True, type=int, help="Source work item ID")
    lk.add_argument("--target-id", required=True, type=int, help="Target work item ID")
    lk.add_argument("--relation", required=True, choices=["parent", "child", "related"], help="Relationship type")
    lk.set_defaults(func=cmd_link)

    return p


def main() -> None:
    parser = _build_parser()
    args = parser.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
