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

1. https://spisbedre.dk/opskrifter/hurtig-kylling-korma
2. https://spisbedre.dk/opskrifter/fried-chicken-med-kaernemaelk
3. https://spisbedre.dk/opskrifter/kylling-med-honning-og-ingefaer-og-ris
4. https://spisbedre.dk/opskrifter/lynstegt-kylling-med-nudler-og-broccolini
4. https://spisbedre.dk/opskrifter/cremet-tandoorikylling-med-mynteyoghurt
5. https://spisbedre.dk/opskrifter/vegetarisk-gullasch-med-spidskommen-yoghurt
6. https://spisbedre.dk/opskrifter/ristet-tofu-med-ris-forarslog-og-koriander
7. https://spisbedre.dk/opskrifter/gronne-frikadeller-med-edamamebonner
8. https://spisbedre.dk/opskrifter/glasnudler-med-krydret-gris-pak-choy-og-peanuts
9. https://spisbedre.dk/opskrifter/afrikansk-gryderet-med-sode-kartofler-og-kikaerter
10. https://spisbedre.dk/opskrifter/indisk-gryderet-med-kylling-og-kartofler
11. https://spisbedre.dk/opskrifter/indisk-gryderet-med-kylling-og-kartofler
12. https://spisbedre.dk/opskrifter/kyllingenuggets-med-dip
13. https://spisbedre.dk/opskrifter/verdens-bedste-chili-con-carne
14. https://spisbedre.dk/opskrifter/chicken-korma-med-ris
15. https://spisbedre.dk/opskrifter/kylling-cashew-med-grontsager
16. https://spisbedre.dk/opskrifter/vegetarfrikadeller-med-quinoa-og-sode-kartofler
17. https://spisbedre.dk/opskrifter/quinoa-bowl-med-krydret-kylling
18. https://spisbedre.dk/opskrifter/wraps-med-quinoadeller-og-sod-chilidressing
19. https://spisbedre.dk/opskrifter/chili-sin-carne-den-bedste-opskrift
20. https://spisbedre.dk/opskrifter/orientalsk-risret-med-kylling-og-karry
21. https://spisbedre.dk/opskrifter/hovdingegryde-med-kartoffelmos
22. https://spisbedre.dk/opskrifter/one-pot-risret-med-kylling
23. https://spisbedre.dk/opskrifter/paprikagryde-med-oksekod-og-grontsager
24. https://spisbedre.dk/opskrifter/risret-med-oksekod-og-karry
25. https://spisbedre.dk/opskrifter/marokkansk-kylling-med-citron-mandler-og-oliven
26. https://spisbedre.dk/opskrifter/laks-med-sprod-crust-og-dampet-gront
27. https://spisbedre.dk/opskrifter/laks-i-paprikasauce-med-ris-og-broccolisalat
28. https://spisbedre.dk/opskrifter/honningmarinerede-kyllingespyd-med-nudelsalat
29. https://spisbedre.dk/opskrifter/kylling-i-tomatsauce-med-gremolata-og-frisk-pasta
30. https://spisbedre.dk/opskrifter/chili-sin-carne-med-bonner
31. https://spisbedre.dk/opskrifter/chicken-chowder
32. https://spisbedre.dk/opskrifter/paprikagryde-med-svinekod
33. https://spisbedre.dk/opskrifter/marokkansk-lammegryde
34. https://spisbedre.dk/opskrifter/kyllingegryde-med-hoisinsauce-og-ris
35. https://spisbedre.dk/opskrifter/kylling-stroganoff-med-ris
36. https://spisbedre.dk/opskrifter/butter-chicken-med-fladbrod
37. https://spisbedre.dk/opskrifter/kylling-kiev
38. https://spisbedre.dk/opskrifter/pasta-med-linsesauce
39. https://spisbedre.dk/opskrifter/karrygryde-med-kikaerter-og-blomkal
40. https://spisbedre.dk/opskrifter/ghormeh-sabzi-persisk-gryderet
41. https://spisbedre.dk/opskrifter/chili-con-kylling
42. https://spisbedre.dk/opskrifter/stir-fry-med-kylling
43. https://spisbedre.dk/opskrifter/sod-kartoffelcurry-med-kikaerter

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
