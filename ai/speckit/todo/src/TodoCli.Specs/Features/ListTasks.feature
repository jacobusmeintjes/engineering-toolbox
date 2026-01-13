Feature: List Tasks
  As a user
  I want to view my tasks in a formatted table with sorting and filtering
  So that I can review my work and prioritize effectively

  Background:
    Given I have an empty task list

  Scenario: List all tasks in default order
    Given I have added the following tasks:
      | Title                | Priority | DueDate    | Tags        |
      | Write project report | High     | 2026-01-15 | work,urgent |
      | Buy groceries        | Medium   |            | personal    |
      | Review pull request  | Low      | 2026-01-20 | work        |
    When I list all tasks
    Then I should see 3 tasks in the output
    And the tasks should be sorted by created date

  Scenario: Filter tasks by status - incomplete only
    Given I have added the following tasks:
      | Title           | Priority | Status    |
      | Active task     | High     | Incomplete|
      | Completed task  | Medium   | Complete  |
      | Another active  | Low      | Incomplete|
    When I list tasks with status filter "incomplete"
    Then I should see 2 tasks in the output
    And I should only see incomplete tasks

  Scenario: Filter tasks by status - complete only
    Given I have added the following tasks:
      | Title           | Priority | Status    |
      | Active task     | High     | Incomplete|
      | Completed task  | Medium   | Complete  |
      | Another done    | Low      | Complete  |
    When I list tasks with status filter "complete"
    Then I should see 2 tasks in the output
    And I should only see complete tasks

  Scenario: Filter tasks by priority
    Given I have added the following tasks:
      | Title          | Priority |
      | Urgent work    | High     |
      | Normal work    | Medium   |
      | Low priority   | Low      |
      | Another urgent | High     |
    When I list tasks with priority filter "High"
    Then I should see 2 tasks in the output
    And all tasks should have priority "High"

  Scenario: Filter tasks by single tag
    Given I have added the following tasks:
      | Title            | Tags            |
      | Work task 1      | work,urgent     |
      | Personal task    | personal        |
      | Work task 2      | work            |
      | Home improvement | home,personal   |
    When I list tasks with tag filter "work"
    Then I should see 2 tasks in the output
    And all tasks should have tag "work"

  Scenario: Filter tasks by multiple tags (OR logic)
    Given I have added the following tasks:
      | Title            | Tags            |
      | Work task        | work            |
      | Personal task    | personal        |
      | Home task        | home            |
      | Urgent work      | work,urgent     |
    When I list tasks with tag filter "work,personal"
    Then I should see 3 tasks in the output
    And all tasks should have at least one of tags "work,personal"

  Scenario: Filter tasks by due date - overdue
    Given I have added the following tasks:
      | Title         | DueDate    |
      | Overdue task  | 2026-01-10 |
      | Today task    | 2026-01-13 |
      | Future task   | 2026-01-20 |
    When I list tasks with due date filter "overdue"
    Then I should see 1 tasks in the output
    And the task "Overdue task" should be in the output

  Scenario: Filter tasks by due date - today
    Given I have added the following tasks:
      | Title         | DueDate    |
      | Overdue task  | 2026-01-10 |
      | Today task    | 2026-01-13 |
      | Future task   | 2026-01-20 |
    When I list tasks with due date filter "today"
    Then I should see 1 tasks in the output
    And the task "Today task" should be in the output

  Scenario: Filter tasks by due date - week
    Given I have added the following tasks:
      | Title         | DueDate    |
      | Today task    | 2026-01-13 |
      | This week     | 2026-01-18 |
      | Next month    | 2026-02-15 |
    When I list tasks with due date filter "week"
    Then I should see 2 tasks in the output

  Scenario: Sort tasks by priority descending
    Given I have added the following tasks:
      | Title          | Priority |
      | Medium task    | Medium   |
      | High task      | High     |
      | Low task       | Low      |
    When I list tasks sorted by "priority"
    Then the first task should be "High task"
    And the last task should be "Low task"

  Scenario: Sort tasks by due date ascending
    Given I have added the following tasks:
      | Title       | DueDate    |
      | Far future  | 2026-03-01 |
      | Soon        | 2026-01-15 |
      | Very soon   | 2026-01-14 |
    When I list tasks sorted by "due"
    Then the first task should be "Very soon"
    And the last task should be "Far future"

  Scenario: Color coding for overdue tasks
    Given I have added the following tasks:
      | Title         | DueDate    |
      | Overdue task  | 2026-01-10 |
      | Future task   | 2026-01-20 |
    When I list all tasks
    Then the task "Overdue task" should be displayed in red
    And the task "Future task" should not be displayed in red

  Scenario: Color coding for tasks due today
    Given I have added the following tasks:
      | Title       | DueDate    |
      | Today task  | 2026-01-13 |
      | Future task | 2026-01-20 |
    When I list all tasks
    Then the task "Today task" should be displayed in yellow
    And the task "Future task" should not be displayed in yellow

  Scenario: Color coding for completed tasks
    Given I have added the following tasks:
      | Title           | Status    |
      | Completed task  | Complete  |
      | Active task     | Incomplete|
    When I list all tasks
    Then the task "Completed task" should be displayed in green
    And the task "Active task" should not be displayed in green

  Scenario: Visual indicators in task list
    Given I have added the following tasks:
      | Title           | Priority | DueDate    | Status    |
      | Overdue urgent  | High     | 2026-01-10 | Incomplete|
      | Completed task  | Medium   | 2026-01-15 | Complete  |
    When I list all tasks
    Then the task "Overdue urgent" should show indicator "[!]"
    And the task "Overdue urgent" should show priority "[H]"
    And the task "Completed task" should show indicator "[✓]"

  Scenario: List empty task list
    When I list all tasks
    Then I should see message "No tasks found"

  Scenario: List with combined filters
    Given I have added the following tasks:
      | Title            | Priority | Status    | Tags     |
      | High work task   | High     | Incomplete| work     |
      | High done task   | High     | Complete  | work     |
      | Low work task    | Low      | Incomplete| work     |
      | High personal    | High     | Incomplete| personal |
    When I list tasks with status filter "incomplete" and priority filter "High" and tag filter "work"
    Then I should see 1 tasks in the output
    And the task "High work task" should be in the output

  Scenario: Table fits within 80 columns
    Given I have added a task with a very long title "This is an extremely long task title that exceeds normal length and should be truncated to fit within the 80-column terminal width constraint while still remaining readable"
    When I list all tasks
    Then the output width should not exceed 80 columns
    And the long title should be truncated with ellipsis
