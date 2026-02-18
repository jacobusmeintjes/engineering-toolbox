#!/usr/bin/env python3
"""
Bug Creation Script for Testers

Creates bugs in Azure DevOps with test recording artifacts and links them to user stories.

Usage:
    python3 create_bug.py --title "Login fails with invalid credentials" \
                          --userstory 433 \
                          --severity "2 - High" \
                          --repro-steps "1. Navigate to /login\n2. Enter invalid password\n3. Click submit" \
                          --expected "Error message displayed" \
                          --actual "Page hangs indefinitely" \
                          --recording-video "test-results/videos/login-test-001.webm" \
                          --recording-trace "test-results/traces/login-test-001.zip"
"""

import sys
import os
import subprocess
import argparse
import json

def run_command(cmd):
    """Run shell command and return output"""
    result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
    return result.stdout.strip(), result.stderr.strip(), result.returncode

def get_user_story_info(story_id):
    """Get user story details from Azure DevOps"""
    script_path = os.path.join(
        os.path.dirname(__file__),
        "../skills/scrum-master/azdo_workitems.py"
    )
    
    cmd = f"python3 {script_path} show --id {story_id}"
    stdout, stderr, code = run_command(cmd)
    
    if code != 0:
        return None, f"Failed to get user story: {stderr}"
    
    try:
        story_info = json.loads(stdout)
        return story_info, None
    except json.JSONDecodeError as e:
        return None, f"Failed to parse user story: {e}"

def create_bug(title, user_story_id, severity, repro_steps, expected, actual, 
               video_path=None, trace_path=None, har_path=None, screenshot_path=None):
    """Create a bug work item in Azure DevOps"""
    
    # Get user story info
    story_info, error = get_user_story_info(user_story_id)
    if error:
        return None, error
    
    story_title = story_info.get("title", f"User Story {user_story_id}")
    
    # Build description with recording artifacts
    description = f"""## Reproduction Steps
{repro_steps}

## Expected Result
{expected}

## Actual Result
{actual}

## Test Recordings
"""
    
    if video_path:
        description += f"\n**Video Recording:** `{video_path}`"
    if trace_path:
        description += f"\n**Playwright Trace:** `{trace_path}`"
        description += f"\n  - View with: `npx playwright show-trace {trace_path}`"
    if har_path:
        description += f"\n**HAR File:** `{har_path}`"
        description += f"\n  - View in browser DevTools → Network → Import HAR"
    if screenshot_path:
        description += f"\n**Screenshot:** `{screenshot_path}`"
    
    description += f"""

## Environment
- Test Framework: Playwright.NET / xUnit
- Related User Story: #{user_story_id} - {story_title}
- Date: {subprocess.check_output(['date', '+%Y-%m-%d %H:%M:%S']).decode().strip()}

## Instructions for Developer
1. Review the test recordings above to see the exact failure
2. Reproduce locally using the repro steps
3. Fix the issue
4. Update bug with fix details and move to Resolved
5. Notify tester for verification
"""
    
    # Create the bug using azdo_workitems.py
    script_path = os.path.join(
        os.path.dirname(__file__),
        "../skills/scrum-master/azdo_workitems.py"
    )
    
    # Escape description for command line
    escaped_desc = description.replace('"', '\\"').replace("'", "\\'")
    
    cmd = f"""python3 {script_path} create \
        --type Bug \
        --title "{title}" \
        --description "{escaped_desc}" \
        --tags "automated-test,needs-fix"
    """
    
    stdout, stderr, code = run_command(cmd)
    
    if code != 0:
        return None, f"Failed to create bug: {stderr}"
    
    # Parse bug ID from output
    try:
        bug_info = json.loads(stdout)
        bug_id = bug_info.get("id")
        
        # Link to user story as Related
        link_cmd = f"""python3 {script_path} link \
            --from-id {bug_id} \
            --to-id {user_story_id} \
            --link-type Related
        """
        run_command(link_cmd)
        
        # Set severity if provided
        if severity:
            # Update with severity field
            # Note: Severity field may need to be added via API directly
            pass
        
        return bug_id, None
    except json.JSONDecodeError as e:
        return None, f"Failed to parse bug creation response: {e}"

def main():
    parser = argparse.ArgumentParser(
        description="Create a bug in Azure DevOps with test recording artifacts"
    )
    parser.add_argument(
        "--title",
        required=True,
        help="Bug title (short summary)"
    )
    parser.add_argument(
        "--userstory",
        required=True,
        type=int,
        help="User Story ID this bug relates to"
    )
    parser.add_argument(
        "--severity",
        choices=["1 - Critical", "2 - High", "3 - Medium", "4 - Low"],
        default="3 - Medium",
        help="Bug severity"
    )
    parser.add_argument(
        "--repro-steps",
        required=True,
        help="Steps to reproduce (can use \\n for newlines)"
    )
    parser.add_argument(
        "--expected",
        required=True,
        help="Expected behavior"
    )
    parser.add_argument(
        "--actual",
        required=True,
        help="Actual behavior (the bug)"
    )
    parser.add_argument(
        "--recording-video",
        help="Path to video recording file"
    )
    parser.add_argument(
        "--recording-trace",
        help="Path to Playwright trace file (.zip)"
    )
    parser.add_argument(
        "--recording-har",
        help="Path to HAR file (API tests)"
    )
    parser.add_argument(
        "--screenshot",
        help="Path to screenshot file"
    )
    
    args = parser.parse_args()
    
    # Validate recording files exist
    for path_arg, path_val in [
        ("--recording-video", args.recording_video),
        ("--recording-trace", args.recording_trace),
        ("--recording-har", args.recording_har),
        ("--screenshot", args.screenshot)
    ]:
        if path_val and not os.path.exists(path_val):
            print(f"⚠️  Warning: {path_arg} file not found: {path_val}", file=sys.stderr)
    
    print(f"🐛 Creating bug: {args.title}")
    print(f"   Linked to User Story #{args.userstory}")
    print(f"   Severity: {args.severity}")
    
    bug_id, error = create_bug(
        title=args.title,
        user_story_id=args.userstory,
        severity=args.severity,
        repro_steps=args.repro_steps.replace("\\n", "\n"),
        expected=args.expected,
        actual=args.actual,
        video_path=args.recording_video,
        trace_path=args.recording_trace,
        har_path=args.recording_har,
        screenshot_path=args.screenshot
    )
    
    if error:
        print(f"❌ Error: {error}", file=sys.stderr)
        return 1
    
    print(f"✅ Bug #{bug_id} created successfully")
    print(f"   URL: https://dev.azure.com/phoenixcode/cf79c609-44af-4b35-aeed-80cab26b6c41/_workitems/edit/{bug_id}")
    
    if args.recording_video:
        print(f"   📹 Video: {args.recording_video}")
    if args.recording_trace:
        print(f"   🔍 Trace: {args.recording_trace}")
        print(f"      View with: npx playwright show-trace {args.recording_trace}")
    if args.recording_har:
        print(f"   📊 HAR: {args.recording_har}")
    
    print(f"\n🔧 Next steps:")
    print(f"   1. Developer reviews recordings and fixes issue")
    print(f"   2. Developer moves bug to Resolved")
    print(f"   3. Tester verifies fix and closes bug")
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
