# SoloCompany — Product blueprint

## Product promise
SoloCompany helps independent builders understand their business, test decisions before committing resources, and organize repetitive work. It combines a configurable business model, scenario simulator, action planner and human-approved automation templates.

**Core loop:** Understand → Simulate → Act → Measure → Improve.

SoloCompany is not positioned as an autonomous replacement for the owner. It is a decision-support and workflow tool that makes a small operator more capable while keeping them in control.

## Audience
The product serves a range of independent businesses through industry workspaces on top of one shared engine.

### Initial workspace templates
- **Indie game development:** production scope, milestones, wishlist conversion assumptions, launch timing, platform fees, contractors, post-launch support and player feedback.
- **Physical product sellers:** unit economics, average order value, inventory, supplier lead time, shipping, returns, repeat purchases and acquisition costs.
- **Digital products and creators:** audience, traffic, conversion, product pricing, refunds, support load and content capacity.
- **Freelancers and consultants:** billable utilization, effective hourly earnings, rates, project scope, revision cycles, lead flow and client concentration.
- **Small agencies and studios:** client profitability, utilization, pipeline, rework, handoffs, staffing capacity and payment timing.
- **Local services:** bookings, travel time, average job value, cancellations, referrals, scheduling and service capacity.
- **Small SaaS / software:** activation, paid conversion, churn, recurring revenue, support demand, acquisition costs and reliability.
- **Events and communities:** registrations, attendance, venue costs, sponsors, organizer workload, repeat attendance and event break-even.
- **Custom business:** a generic workspace with editable assumptions.

Templates are starting points, not benchmarks. Users must be able to rename assumptions and adapt them to their real operation.

## Core modules

### 1. Business profile
Collect a business name, plain-language description, primary goal and selected workspace. Avoid requesting sensitive information in the prototype. Explain what is stored and where.

### 2. Business Flight Simulator
Let users edit revenue, costs, volume and available hours, choose a decision to test, set a scenario strength and enter additional spend. Show baseline versus scenario operating surplus, margin and assumptions. Explain uncertainty and missing variables. A future version should support editable formulas, ranges, multi-period cash flow, probability distributions and scenario comparison.

The current demo calculations are intentionally simplified. They must never be represented as a reliable financial forecast or as evidence of causal effect.

### 3. Industry workspaces
Each workspace offers relevant variables, decision questions and starter actions. All use the same simulation and planning engine, so new templates can be added without building a separate app.

### 4. Action planner
Users create, categorize, complete and remove tasks. The plan should connect simulation findings to small, measurable next steps. Export plans as JSON.

### 5. Digital worker templates
The prototype produces local drafts for:
- research briefs,
- marketing campaigns,
- operations checklists,
- decision experiments.

These are templates, not connected AI agents. Future agents should use scoped permissions, explicit approval for external actions, activity logs, retry controls, rate limits and clear rollback paths where possible.

### 6. Experiment log
Save scenario inputs and result summaries, revisit them and export them. Later, compare modelled outcomes with real outcomes and record the reasons for variance.

### 7. Data portability
The prototype stores profile, tasks and experiments in browser localStorage and supports JSON export/import. This is device-specific, not cloud sync. A production release needs authentication, encrypted transport, secure server-side storage, backup and deletion workflows, access controls and a privacy policy.

## Product principles
1. **Explain the assumptions.** Never make a single model output look like certainty.
2. **Show trade-offs.** A decision can improve revenue while harming workload, cash or quality.
3. **Keep humans in control.** Drafting and recommending are distinct from sending, publishing, purchasing or deleting.
4. **Local-first prototype.** Build useful deterministic features before adding paid AI calls.
5. **Industry-aware, not industry-locked.** Use adaptable templates and custom variables.
6. **Measure outcomes.** Treat recommendations as hypotheses and compare them with real results.
7. **Minimize data.** Ask only for data needed for a clear purpose; never ask for passwords or payment credentials.
8. **Accessible and responsive.** Make the core product usable on desktop and mobile, keyboard accessible and clear without relying on color alone.

## Suggested roadmap

### Phase 1 — Prototype (current)
- Multi-industry templates
- Editable baseline inputs
- Simple scenario calculations
- Visual baseline/scenario comparison
- Action planner
- Local draft templates for four digital workers
- Experiment log
- Browser-local persistence and JSON portability

### Phase 2 — Validate the problem
- Interview users from 2–3 target groups
- Observe which calculations they trust and which they find confusing
- Identify one recurring decision per segment
- Improve model assumptions and include sensitivity ranges
- Add guided onboarding and a sample workspace for each segment

### Phase 3 — Reliable personal workspace
- User accounts and cloud sync
- Versioned models and scenario comparison
- Editable formulas and units
- Multi-period cash-flow and capacity models
- Import/export CSV
- Activity history, backups and delete-my-data controls

### Phase 4 — Practical integrations
Prioritize integrations only after validating demand. Candidate categories include storefront orders, issue/project trackers, website analytics, calendars and invoicing. Read-only access first; external write actions require user authorization and audit trails.

### Phase 5 — Agent workflows
- Trigger → gather approved context → draft → validate → request approval → execute → log → measure.
- Separate low-risk internal organization from high-impact external actions.
- Require explicit approval for financial transactions, customer commitments, public posts, refunds, account changes and destructive actions.
- Add permissions, quotas, failure handling and undo/rollback where technically possible.

### Phase 6 — Learning system
Compare actual results against scenarios. Track which assumptions were inaccurate. Allow users to recalibrate models from their own data, and clearly separate measured facts from estimates and model-generated hypotheses.

## Success metrics
- Time to first useful model
- Percentage of users who complete a first scenario
- Percentage who turn a scenario into an action
- Weekly retained active workspaces
- Tasks or hours saved, self-reported and validated where possible
- Forecast error for any future validated forecasting feature
- Trust measures: users can identify assumptions and uncertainty
- Conversion to paid plans only after repeat value is demonstrated

## Business model hypotheses
A possible freemium approach: free core modelling, paid saved workspaces and advanced comparisons, and higher tiers for integrations and team collaboration. Pricing should be tested with real users and costs before being treated as final. Never promise that simulations will improve profit.

## Current prototype limitations
- No account system or cloud sync
- No external store, email, bank, analytics or project-tool integrations
- No live AI model connected
- Sample inputs and scenario multipliers are illustrative assumptions
- Calculations are not tax, accounting, investment or legal advice
- Browser-local data may be lost if browser storage is cleared
- Not yet independently security-reviewed or fully user-tested

## First validation milestone
Recruit a small group of game developers, product sellers and freelancers. Ask each to bring one real decision, model it, compare scenarios, and identify one action. Measure whether the process makes the decision clearer. Build the next feature based on observed repeated needs, not on assumptions about what every business might want.
