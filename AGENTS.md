# MinimalUnityGameFramework writing rules

Apply these rules by default whenever you add or update this project's documentation, including documentation affected by code changes. Do not ask the author to repeat these preferences or announce routine compliance.

The reference voice is Angus's Digital Garden: warm, candid and quietly personal. The README introduces his work; a tutorial guides a friend through using it. Assume basic Unity and C# knowledge, but no familiarity with this framework.

## 01 Voice

- In the README, use first person where it helps explain why the framework exists and why it is designed this way. Do not invent personal stories or experience for the author.
- In tutorials, say what to do, where to do it and what happens next. Add a short reason when it helps the reader make a choice or avoid a mistake.
- Use active verbs, familiar words, short paragraphs and natural transitions. Keep necessary technical terms and explain them at first use when needed.
- Avoid marketing claims, bureaucratic wording, literal translation and unnecessary abstractions. Do not make every paragraph chatty or add repeated greetings, jokes, emojis or reassurance.
- Be precise about failures, cancellation, ownership and cleanup. Friendlier wording must not weaken a technical condition or imply behavior that has not been verified.

For example, “Your game owns cross-scene audio policy” becomes “Your game decides which sounds continue across scenes.”

## 02 English and Chinese

- Reader-facing documentation defaults to English, with complete Simplified Chinese counterparts: `README.md` / `README.zh-CN.md` and matching tutorial pairs.
- Write each language naturally. Match meaning, steps and technical conditions rather than translating sentence by sentence.
- Update both languages in the same change. Keep code examples, API names, menu labels, field names, paths, section numbering and verification scope consistent.
- Keep navigation in the current language and provide a link to the other language.

## 03 Structure and layout

- Keep the README high-level, in this order: introduction and philosophy → architecture → module index → directory layout → Try Demo → environment and scope. Link to tutorials for detailed operations and edge cases.
- A tutorial should make its goal, prerequisites, actions, code placement and expected result clear. End with a useful next step when one exists. Use only the sections the task needs; do not pad a short guide to fit a template.
- Preserve the numbering: H1 is unnumbered, H2 uses `01`, H3 uses `1.1`, H4 uses `1.1.1`. Keep operation steps separate from section numbers. Preserve historical anchors when headings change.
- Use numbered lists for sequences, tables for comparisons and short paragraphs for explanations. Keep whitespace clear. Put advanced contracts in expandable sections when they interrupt the first successful run; retain the information.

## 04 Examples and screenshots

- Give the smallest example that works in its stated context. Say which file and method to edit, which imports or prior steps it needs, and whether to add or replace code. Do not leave readers to guess where a snippet belongs.
- Keep braces on control statements, separate logical blocks with blank lines and avoid `#region`. Preserve lifecycle and cleanup requirements in examples.
- State an observable result: a window opens, a Console message appears or a generated file exists. Give the next recovery step for likely errors.
- Add screenshots only when they clarify a setting, location or result that is hard to describe. Use real, current application views; do not fabricate or redraw UI. Reuse suitable images across languages with translated captions.
- When UI or paths change, check the affected screenshots and replace stale ones. Keep optional images expandable where useful; do not add an image to every step.

## 05 Routine updates

- Use the current README and tutorials as examples of the intended voice. Preserve correct, readable passages; make the smallest coherent change.
- When behavior changes, update the affected instructions and both language versions as part of the same task. Keep implementation history in project records, not in the reader's first-use path.
- Before finishing, check local links, anchors, heading numbers, code blocks, images and English/Chinese consistency. Check that the described order matches actual behavior and that the reader knows the next action.
- Report only checks actually performed. Do not turn a documentation review or one-platform test into a claim of broader runtime or platform support.

Keep these rules aligned with the project's writing rules in its Notebook archive when both repositories are available. This repository copy must remain usable without access to that private archive.
