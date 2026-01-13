Feature: Add Task
  As a user
  I want to quickly add tasks with minimal input
  So that I can capture ideas without friction

  Background:
    Given the task storage is empty

  Scenario: Add task with title only
    When I add a task with title "Buy groceries"
    Then the task should be saved with ID
    And the task title should be "Buy groceries"
    And the task priority should be "Medium"
    And the task should be incomplete
    And the task should have creation timestamp

  Scenario: Add task with all metadata
    When I add a task with the following details:
      | Field       | Value                  |
      | Title       | Deploy v2.0            |
      | Description | Run deployment checklist |
      | Due         | 2026-01-20             |
      | Priority    | High                   |
      | Tags        | work,deployment        |
    Then the task should be saved with ID
    And the task title should be "Deploy v2.0"
    And the task description should be "Run deployment checklist"
    And the task due date should be "2026-01-20"
    And the task priority should be "High"
    And the task tags should contain "work"
    And the task tags should contain "deployment"

  Scenario: Add multiple tasks
    When I add a task with title "Task 1"
    And I add a task with title "Task 2"
    And I add a task with title "Task 3"
    Then there should be 3 tasks in storage

  Scenario: Task title validation - empty title
    When I attempt to add a task with empty title
    Then I should receive an error "Task title cannot be empty"

  Scenario: Task title validation - title too long
    When I attempt to add a task with title longer than 200 characters
    Then I should receive an error "Task title cannot exceed 200 characters"

  Scenario: Due date validation - past date
    When I attempt to add a task with due date in the past
    Then I should receive an error "Due date must be today or in the future"

  Scenario: Tag validation - invalid characters
    When I attempt to add a task with tag "work@home"
    Then I should receive an error "contains invalid characters"

  Scenario: Tag validation - too many tags
    When I attempt to add a task with 11 tags
    Then I should receive an error "Maximum 10 tags allowed"

  Scenario: Performance - add task under 50ms
    When I add a task with title "Performance test"
    Then the operation should complete in under 50 milliseconds
