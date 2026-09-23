---
name: Sequential Subagent Tasks
overview: "Parent orchestrator: run pasted tasks strictly in order, each as a new foreground subagent with a clean context. Each agent gets only its own task prompt plus whatever it needs from the repo."
todos:
  - id: run-sequential
    content: Run each pasted task as its own foreground subagent, waiting for each to finish before starting the next
    status: completed
isProject: false
---

# Sequential foreground subagent tasks

This chat is the **parent orchestrator only**. It does not implement the tasks.

Run these tasks in order as separate subagents. Wait for each to finish before starting the next. Do not run them in parallel. Each subagent gets only its own task prompt plus whatever it needs from the repo.

## Paste tasks here

For each of the following numbered points in Task data section, use the below Standard task description, with the link in the points list.

### Standard task description

read @ConvertRecipePrompt.md on instructions. The recipe to convert is attached as a link to a website. The website also contains an image of the dish, please download that and put into @UI/wwwroot/images/recipe-images with the next available id plus the dish name.

### Task data



## Orchestrator rules

- One new subagent per pasted task. Fresh context. Do not `resume` a previous agent.
- Foreground only (`run_in_background: false`). The parent blocks until that agent returns.
- Never launch two Task calls in the same message.
- The subagent prompt is **only that task’s pasted text**, plus that it may read the repo (workspace `C:\Users\trmo\RiderProjects\WhatsForDinner`) to do the work. Do not paste earlier chat, earlier task prompts, or earlier agent transcripts into the next agent.
- Later tasks see earlier work because it is already in the repo, not because the parent retells it.
- If a task fails or is blocked, stop the chain and report. Do not start the next task.

```mermaid
sequenceDiagram
    participant Parent as ParentOrchestrator
    participant A1 as SubagentTask1
    participant A2 as SubagentTask2
    participant AN as SubagentTaskN

    Parent->>A1: Foreground, task 1 prompt only
    A1-->>Parent: Done
    Parent->>A2: Foreground, task 2 prompt only
    A2-->>Parent: Done
    Parent->>AN: Repeat until the list is empty
```
