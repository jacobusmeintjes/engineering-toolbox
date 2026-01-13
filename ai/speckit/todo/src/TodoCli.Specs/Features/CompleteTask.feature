Feature: Complete Task
  As a user
  I want to mark tasks as complete
  So that I can track my progress and see completion timestamps

  Background:
    Given I have an empty task list

  Scenario: Complete a task with full ID
    Given I have added a task with title "Write report"
    When I complete the task by full ID
    Then the task should be marked as complete
    And the completion timestamp should be set
    And I should see a success message with duration

  Scenario: Complete a task with partial ID (8 characters)
    Given I have added a task with title "Review code"
    When I complete the task with partial ID using 8 characters
    Then the task should be marked as complete
    And the completion timestamp should be set

  Scenario: Complete a task with minimal partial ID (4 characters)
    Given I have added a task with title "Test feature"
    When I complete the task with partial ID using 4 characters
    Then the task should be marked as complete
    And the completion timestamp should be set

  Scenario: Reject completion with ambiguous partial ID
    Given I have added multiple tasks with IDs sharing prefix
    When I try to complete a task with ambiguous partial ID
    Then I should see an error about ambiguous ID
    And no tasks should be marked complete

  Scenario: Reject completion of already completed task
    Given I have added a task with title "Buy groceries"
    And I have completed that task
    When I try to complete the same task again
    Then I should see an error that task is already complete

  Scenario: Reject completion with non-existent ID
    When I try to complete a task with ID "nonexist"
    Then I should see an error that task was not found

  Scenario: Reject completion with partial ID less than 4 characters
    Given I have added a task with title "Deploy app"
    When I try to complete the task with only 3 characters
    Then I should see an error about minimum ID length

  Scenario: Display completion duration in human-readable format
    Given I have added a task with title "Quick task" 2 seconds ago
    When I complete the task by full ID
    Then I should see duration displayed as "2 seconds"

  Scenario: Completed task shows in list with completion indicator
    Given I have added a task with title "Finished work"
    When I complete the task by full ID
    And I list all tasks
    Then the task should show completion indicator "[✓]"
    And the task should show completion timestamp

  Scenario: Multiple tasks can be completed independently
    Given I have added the following tasks:
      | Title        |
      | Task 1       |
      | Task 2       |
      | Task 3       |
    When I complete "Task 1" and "Task 3"
    Then "Task 1" should be complete
    And "Task 2" should be incomplete
    And "Task 3" should be complete

  Scenario: Performance - Complete task under 100ms
    Given I have added a task with title "Performance test"
    When I complete the task by full ID
    Then the operation should complete in under 100 milliseconds
