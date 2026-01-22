---
name: ux-designer
description: Expert guidance for UX/UI design including accessibility (WCAG), responsive design, design systems, user research, information architecture, interaction patterns, and design-to-development handoff best practices
---

# UX Designer Expert

## When to use this skill

Use this skill when:
- Designing user interfaces and experiences
- Implementing accessibility standards (WCAG)
- Creating design systems and component libraries
- Conducting user research and usability testing
- Designing responsive and mobile-first interfaces
- Creating wireframes and prototypes
- Implementing interaction patterns
- Collaborating with developers on implementation

## Accessibility (WCAG) Standards

### WCAG 2.1 Principles (POUR)

**Perceivable** - Information must be presentable to users
**Operable** - UI components must be operable
**Understandable** - Information and UI must be understandable
**Robust** - Content must work with assistive technologies

### Level A (Must Have)

```html
<!-- ✅ Text alternatives for images -->
<img src="logo.png" alt="Company Logo" />
<img src="decorative.png" alt="" /> <!-- Decorative images -->

<!-- ✅ Captions for videos -->
<video controls>
    <source src="video.mp4" type="video/mp4">
    <track kind="captions" src="captions.vtt" srclang="en" label="English">
</video>

<!-- ✅ Keyboard accessible -->
<button onclick="handleClick()">Submit</button>
<!-- NOT: <div onclick="handleClick()">Submit</div> -->

<!-- ✅ Semantic HTML -->
<nav>
    <ul>
        <li><a href="/">Home</a></li>
        <li><a href="/about">About</a></li>
    </ul>
</nav>

<!-- ✅ Form labels -->
<label for="email">Email Address</label>
<input type="email" id="email" name="email" required>

<!-- ✅ Clear focus indicators -->
<style>
button:focus {
    outline: 2px solid #0066cc;
    outline-offset: 2px;
}
</style>
```

### Level AA (Should Have)

```html
<!-- ✅ Color contrast ratios -->
<!-- Text: 4.5:1 minimum -->
<!-- Large text (18pt+): 3:1 minimum -->
<style>
.text-primary {
    color: #0066cc; /* On white: 7.5:1 ✓ */
    background: #ffffff;
}

.text-secondary {
    color: #666666; /* On white: 5.7:1 ✓ */
    background: #ffffff;
}
</style>

<!-- ✅ Resize text up to 200% -->
<style>
body {
    font-size: 1rem; /* Use relative units */
}

@media (min-width: 1200px) {
    body {
        font-size: 1.125rem; /* 18px */
    }
}
</style>

<!-- ✅ Multiple ways to find pages -->
<nav aria-label="Main navigation">
    <!-- Site navigation -->
</nav>

<nav aria-label="Breadcrumb">
    <ol>
        <li><a href="/">Home</a></li>
        <li><a href="/customers">Customers</a></li>
        <li aria-current="page">Details</li>
    </ol>
</nav>

<form role="search">
    <label for="search">Search</label>
    <input type="search" id="search">
</form>

<!-- ✅ Descriptive headings and labels -->
<h1>Customer Management</h1>
<h2>Customer List</h2>
<h3>Active Customers</h3>

<label for="start-date">
    Start Date
    <span class="hint">(MM/DD/YYYY)</span>
</label>
<input type="date" id="start-date" aria-describedby="date-hint">
<span id="date-hint" class="sr-only">Format: Month, Day, Year</span>
```

### Level AAA (Nice to Have)

```html
<!-- ✅ Enhanced contrast -->
<style>
.text-enhanced {
    color: #000000; /* 21:1 on white */
    background: #ffffff;
}
</style>

<!-- ✅ Sign language interpretation -->
<video controls>
    <source src="video.mp4">
    <track kind="captions" src="captions.vtt">
    <track kind="sign" src="sign-language.vtt">
</video>

<!-- ✅ Extended audio descriptions -->
<video controls>
    <source src="video.mp4">
    <track kind="descriptions" src="descriptions.vtt">
</video>
```

### ARIA (Accessible Rich Internet Applications)

```html
<!-- ✅ ARIA roles -->
<div role="banner">
    <h1>Site Header</h1>
</div>

<div role="navigation" aria-label="Main">
    <ul><!-- Navigation items --></ul>
</div>

<main role="main">
    <!-- Main content -->
</main>

<div role="complementary">
    <h2>Related Information</h2>
</div>

<!-- ✅ ARIA states and properties -->
<button 
    aria-expanded="false" 
    aria-controls="menu-dropdown"
    aria-haspopup="true">
    Menu
</button>
<div id="menu-dropdown" aria-hidden="true">
    <!-- Dropdown content -->
</div>

<!-- ✅ ARIA live regions -->
<div aria-live="polite" aria-atomic="true">
    <p>Saving changes...</p>
</div>

<div role="alert" aria-live="assertive">
    <p>Error: Please correct the highlighted fields.</p>
</div>

<!-- ✅ ARIA labels -->
<button aria-label="Close dialog">
    <svg><!-- X icon --></svg>
</button>

<nav aria-labelledby="nav-heading">
    <h2 id="nav-heading">Customer Navigation</h2>
    <!-- Navigation items -->
</nav>

<!-- ✅ Form validation -->
<input 
    type="email" 
    id="email"
    aria-invalid="true"
    aria-describedby="email-error">
<span id="email-error" role="alert">
    Please enter a valid email address
</span>

<!-- ✅ Progress indicators -->
<div 
    role="progressbar" 
    aria-valuenow="45" 
    aria-valuemin="0" 
    aria-valuemax="100">
    <div style="width: 45%"></div>
</div>

<!-- ✅ Tab panels -->
<div role="tablist" aria-label="Customer Information">
    <button 
        role="tab" 
        aria-selected="true" 
        aria-controls="panel-1">
        Details
    </button>
    <button 
        role="tab" 
        aria-selected="false" 
        aria-controls="panel-2">
        Orders
    </button>
</div>
<div role="tabpanel" id="panel-1">
    <!-- Details content -->
</div>
```

### Screen Reader Only Content

```css
.sr-only {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border-width: 0;
}

.sr-only-focusable:focus {
    position: static;
    width: auto;
    height: auto;
    padding: inherit;
    margin: inherit;
    overflow: visible;
    clip: auto;
    white-space: normal;
}
```

```html
<button>
    <svg><!-- Icon --></svg>
    <span class="sr-only">Delete customer</span>
</button>

<a href="#main-content" class="sr-only-focusable">
    Skip to main content
</a>
```

## Responsive Design

### Mobile-First Approach

```css
/* Base styles (mobile) */
.container {
    padding: 1rem;
    width: 100%;
}

.grid {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

/* Tablet and up */
@media (min-width: 768px) {
    .container {
        padding: 2rem;
        max-width: 720px;
        margin: 0 auto;
    }
    
    .grid {
        flex-direction: row;
        flex-wrap: wrap;
    }
    
    .grid > * {
        flex: 1 1 calc(50% - 0.5rem);
    }
}

/* Desktop */
@media (min-width: 1024px) {
    .container {
        max-width: 960px;
    }
    
    .grid > * {
        flex: 1 1 calc(33.333% - 0.67rem);
    }
}

/* Large desktop */
@media (min-width: 1280px) {
    .container {
        max-width: 1200px;
    }
}
```

### Responsive Typography

```css
:root {
    /* Fluid typography */
    --font-size-sm: clamp(0.875rem, 0.85rem + 0.125vw, 1rem);
    --font-size-base: clamp(1rem, 0.95rem + 0.25vw, 1.25rem);
    --font-size-lg: clamp(1.125rem, 1rem + 0.5vw, 1.5rem);
    --font-size-xl: clamp(1.5rem, 1.25rem + 1vw, 2.5rem);
    
    /* Line heights */
    --line-height-tight: 1.25;
    --line-height-normal: 1.5;
    --line-height-relaxed: 1.75;
}

body {
    font-size: var(--font-size-base);
    line-height: var(--line-height-normal);
}

h1 {
    font-size: var(--font-size-xl);
    line-height: var(--line-height-tight);
}

p {
    max-width: 65ch; /* Optimal reading length */
}
```

### Responsive Images

```html
<!-- Responsive image -->
<img 
    src="image-800w.jpg"
    srcset="image-400w.jpg 400w,
            image-800w.jpg 800w,
            image-1200w.jpg 1200w"
    sizes="(max-width: 768px) 100vw,
           (max-width: 1200px) 50vw,
           33vw"
    alt="Customer profile">

<!-- Art direction -->
<picture>
    <source 
        media="(min-width: 768px)" 
        srcset="desktop-hero.jpg">
    <source 
        media="(min-width: 480px)" 
        srcset="tablet-hero.jpg">
    <img src="mobile-hero.jpg" alt="Hero image">
</picture>

<!-- Modern formats with fallback -->
<picture>
    <source type="image/avif" srcset="image.avif">
    <source type="image/webp" srcset="image.webp">
    <img src="image.jpg" alt="Optimized image">
</picture>
```

### Container Queries

```css
/* Component responsive to container, not viewport */
.card-container {
    container-type: inline-size;
    container-name: card;
}

.card {
    display: flex;
    flex-direction: column;
}

@container card (min-width: 400px) {
    .card {
        flex-direction: row;
    }
    
    .card-image {
        width: 200px;
    }
}

@container card (min-width: 600px) {
    .card {
        padding: 2rem;
    }
}
```

## Design Systems

### Design Tokens

```css
:root {
    /* Colors */
    --color-primary-50: #e6f2ff;
    --color-primary-100: #b3d9ff;
    --color-primary-500: #0066cc;
    --color-primary-700: #004c99;
    --color-primary-900: #003366;
    
    --color-neutral-50: #f9fafb;
    --color-neutral-100: #f3f4f6;
    --color-neutral-500: #6b7280;
    --color-neutral-900: #111827;
    
    --color-success: #10b981;
    --color-warning: #f59e0b;
    --color-error: #ef4444;
    --color-info: #3b82f6;
    
    /* Spacing */
    --space-1: 0.25rem;   /* 4px */
    --space-2: 0.5rem;    /* 8px */
    --space-3: 0.75rem;   /* 12px */
    --space-4: 1rem;      /* 16px */
    --space-6: 1.5rem;    /* 24px */
    --space-8: 2rem;      /* 32px */
    --space-12: 3rem;     /* 48px */
    --space-16: 4rem;     /* 64px */
    
    /* Typography */
    --font-family-sans: 'Inter', system-ui, -apple-system, sans-serif;
    --font-family-mono: 'Fira Code', 'Courier New', monospace;
    
    --font-weight-normal: 400;
    --font-weight-medium: 500;
    --font-weight-semibold: 600;
    --font-weight-bold: 700;
    
    /* Borders */
    --border-radius-sm: 0.25rem;
    --border-radius-md: 0.375rem;
    --border-radius-lg: 0.5rem;
    --border-radius-xl: 0.75rem;
    --border-radius-full: 9999px;
    
    --border-width: 1px;
    --border-width-2: 2px;
    
    /* Shadows */
    --shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 0.05);
    --shadow-md: 0 4px 6px -1px rgb(0 0 0 / 0.1);
    --shadow-lg: 0 10px 15px -3px rgb(0 0 0 / 0.1);
    --shadow-xl: 0 20px 25px -5px rgb(0 0 0 / 0.1);
    
    /* Transitions */
    --transition-fast: 150ms ease-in-out;
    --transition-base: 200ms ease-in-out;
    --transition-slow: 300ms ease-in-out;
    
    /* Z-index scale */
    --z-dropdown: 1000;
    --z-sticky: 1020;
    --z-fixed: 1030;
    --z-modal-backdrop: 1040;
    --z-modal: 1050;
    --z-popover: 1060;
    --z-tooltip: 1070;
}
```

### Component Library Structure

```
design-system/
├── foundations/
│   ├── colors.css
│   ├── typography.css
│   ├── spacing.css
│   └── elevation.css
├── components/
│   ├── button.css
│   ├── input.css
│   ├── card.css
│   ├── modal.css
│   └── ...
├── patterns/
│   ├── forms.css
│   ├── navigation.css
│   └── data-display.css
└── utilities/
    ├── display.css
    ├── flexbox.css
    └── spacing.css
```

### Component Anatomy - Button

```css
/* Button Base */
.btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-2);
    
    padding: var(--space-2) var(--space-4);
    border: var(--border-width) solid transparent;
    border-radius: var(--border-radius-md);
    
    font-family: var(--font-family-sans);
    font-size: var(--font-size-base);
    font-weight: var(--font-weight-medium);
    line-height: 1.5;
    
    cursor: pointer;
    user-select: none;
    white-space: nowrap;
    
    transition: all var(--transition-base);
}

/* Button Variants */
.btn-primary {
    background-color: var(--color-primary-500);
    color: white;
}

.btn-primary:hover {
    background-color: var(--color-primary-600);
}

.btn-primary:focus-visible {
    outline: 2px solid var(--color-primary-500);
    outline-offset: 2px;
}

.btn-primary:disabled {
    background-color: var(--color-neutral-300);
    cursor: not-allowed;
    opacity: 0.6;
}

/* Button Sizes */
.btn-sm {
    padding: var(--space-1) var(--space-3);
    font-size: var(--font-size-sm);
}

.btn-lg {
    padding: var(--space-3) var(--space-6);
    font-size: var(--font-size-lg);
}

/* Button with Icon */
.btn-icon-left {
    padding-left: var(--space-3);
}

.btn-icon-right {
    padding-right: var(--space-3);
}
```

## User Interface Patterns

### Form Patterns

```html
<!-- Inline validation -->
<div class="form-group">
    <label for="email" class="form-label">
        Email Address
        <span class="required" aria-label="required">*</span>
    </label>
    <input 
        type="email" 
        id="email" 
        class="form-input"
        aria-required="true"
        aria-invalid="false"
        aria-describedby="email-help email-error">
    <p id="email-help" class="form-help">
        We'll never share your email with anyone else.
    </p>
    <p id="email-error" class="form-error" role="alert" hidden>
        Please enter a valid email address.
    </p>
</div>

<!-- Multi-step form with progress -->
<div class="form-wizard">
    <nav aria-label="Form progress">
        <ol class="wizard-steps">
            <li class="step-complete" aria-current="false">
                <span class="step-number">1</span>
                <span class="step-label">Personal Info</span>
            </li>
            <li class="step-active" aria-current="step">
                <span class="step-number">2</span>
                <span class="step-label">Contact Details</span>
            </li>
            <li class="step-incomplete" aria-current="false">
                <span class="step-number">3</span>
                <span class="step-label">Review</span>
            </li>
        </ol>
    </nav>
    
    <div class="wizard-content">
        <!-- Current step content -->
    </div>
    
    <div class="wizard-actions">
        <button type="button" class="btn btn-secondary">Previous</button>
        <button type="button" class="btn btn-primary">Next</button>
    </div>
</div>

<!-- Search with autocomplete -->
<div class="search-wrapper">
    <label for="search">Search customers</label>
    <input 
        type="search" 
        id="search"
        role="combobox"
        aria-expanded="false"
        aria-controls="search-results"
        aria-autocomplete="list">
    
    <ul id="search-results" role="listbox" hidden>
        <li role="option" tabindex="-1">
            John Doe
        </li>
        <li role="option" tabindex="-1">
            Jane Smith
        </li>
    </ul>
</div>
```

### Navigation Patterns

```html
<!-- Responsive navigation -->
<nav aria-label="Main navigation">
    <div class="nav-container">
        <a href="/" class="nav-logo">
            <img src="logo.svg" alt="Company Name">
        </a>
        
        <button 
            class="nav-toggle" 
            aria-expanded="false"
            aria-controls="nav-menu"
            aria-label="Toggle navigation">
            <span class="hamburger"></span>
        </button>
        
        <ul id="nav-menu" class="nav-menu">
            <li><a href="/" aria-current="page">Home</a></li>
            <li><a href="/customers">Customers</a></li>
            <li><a href="/orders">Orders</a></li>
            <li><a href="/reports">Reports</a></li>
        </ul>
    </div>
</nav>

<!-- Breadcrumbs -->
<nav aria-label="Breadcrumb">
    <ol class="breadcrumb">
        <li>
            <a href="/">
                <svg aria-hidden="true"><!-- Home icon --></svg>
                <span class="sr-only">Home</span>
            </a>
        </li>
        <li>
            <a href="/customers">Customers</a>
        </li>
        <li aria-current="page">
            John Doe
        </li>
    </ol>
</nav>

<!-- Tabs -->
<div class="tabs">
    <div role="tablist" aria-label="Customer information">
        <button 
            role="tab" 
            aria-selected="true" 
            aria-controls="tab-panel-1"
            id="tab-1">
            Details
        </button>
        <button 
            role="tab" 
            aria-selected="false" 
            aria-controls="tab-panel-2"
            id="tab-2"
            tabindex="-1">
            Orders
        </button>
        <button 
            role="tab" 
            aria-selected="false" 
            aria-controls="tab-panel-3"
            id="tab-3"
            tabindex="-1">
            Activity
        </button>
    </div>
    
    <div 
        role="tabpanel" 
        id="tab-panel-1" 
        aria-labelledby="tab-1"
        tabindex="0">
        <!-- Details content -->
    </div>
    
    <div 
        role="tabpanel" 
        id="tab-panel-2" 
        aria-labelledby="tab-2"
        hidden
        tabindex="0">
        <!-- Orders content -->
    </div>
</div>
```

### Feedback Patterns

```html
<!-- Toast notifications -->
<div 
    role="status" 
    aria-live="polite" 
    aria-atomic="true"
    class="toast toast-success">
    <svg aria-hidden="true"><!-- Success icon --></svg>
    <div class="toast-content">
        <p class="toast-title">Success</p>
        <p class="toast-message">Customer created successfully</p>
    </div>
    <button 
        class="toast-close" 
        aria-label="Close notification">
        ×
    </button>
</div>

<!-- Loading states -->
<button class="btn btn-primary" disabled>
    <span class="spinner" role="status" aria-hidden="true"></span>
    <span>Loading...</span>
</button>

<!-- Empty states -->
<div class="empty-state">
    <img src="empty.svg" alt="" aria-hidden="true">
    <h2>No customers found</h2>
    <p>Get started by creating your first customer</p>
    <button class="btn btn-primary">Create Customer</button>
</div>

<!-- Error states -->
<div role="alert" class="alert alert-error">
    <svg aria-hidden="true"><!-- Error icon --></svg>
    <div>
        <h3>Error loading data</h3>
        <p>Unable to load customer information. Please try again.</p>
        <button class="btn btn-sm btn-error">Retry</button>
    </div>
</div>
```

### Modal Patterns

```html
<!-- Accessible modal dialog -->
<div 
    role="dialog" 
    aria-modal="true"
    aria-labelledby="modal-title"
    aria-describedby="modal-description"
    class="modal">
    
    <div class="modal-overlay" aria-hidden="true"></div>
    
    <div class="modal-content">
        <div class="modal-header">
            <h2 id="modal-title">Confirm Delete</h2>
            <button 
                class="modal-close" 
                aria-label="Close dialog">
                ×
            </button>
        </div>
        
        <div class="modal-body">
            <p id="modal-description">
                Are you sure you want to delete this customer?
                This action cannot be undone.
            </p>
        </div>
        
        <div class="modal-footer">
            <button class="btn btn-secondary" data-dismiss="modal">
                Cancel
            </button>
            <button class="btn btn-danger" autofocus>
                Delete
            </button>
        </div>
    </div>
</div>
```

## Information Architecture

### Content Organization

```
Site Structure:
├── Home
├── Customers
│   ├── List
│   ├── Create
│   ├── Edit
│   └── Details
│       ├── Overview
│       ├── Orders
│       ├── Activity
│       └── Settings
├── Orders
│   ├── List
│   ├── Create
│   └── Details
├── Reports
│   ├── Sales
│   ├── Customers
│   └── Inventory
└── Settings
    ├── Account
    ├── Team
    └── Preferences
```

### Navigation Hierarchy

```
Primary Navigation (Main tasks)
└── Customers, Orders, Products, Reports

Secondary Navigation (Context-specific)
└── Customer Details: Overview, Orders, Activity, Settings

Utility Navigation (Account/System)
└── Profile, Settings, Help, Logout

Footer Navigation (Supplementary)
└── About, Privacy, Terms, Contact
```

## User Research

### User Personas Template

```
Persona Name: Sarah the Sales Manager

Demographics:
- Age: 35-45
- Role: Sales Team Manager
- Experience: 10+ years in sales
- Tech Savviness: Medium

Goals:
- Track team performance metrics
- Manage customer relationships
- Generate reports for leadership
- Improve team efficiency

Pain Points:
- Current system is slow and clunky
- Too many clicks to complete tasks
- Difficulty finding customer information
- Manual data entry is time-consuming

Behaviors:
- Uses system daily (2-4 hours)
- Primarily desktop user
- Multi-tasks frequently
- Works with multiple customers simultaneously

Needs:
- Quick access to customer information
- Easy report generation
- Real-time data updates
- Mobile access for on-the-go

Quote: "I need to spend less time in the system
and more time with customers."
```

### Usability Testing Checklist

```
Test Planning:
□ Define test objectives
□ Create test scenarios
□ Recruit participants (5-8 users)
□ Prepare test environment
□ Create observation guide

During Testing:
□ Explain think-aloud protocol
□ Observe without interfering
□ Note pain points and confusion
□ Record session (with permission)
□ Ask follow-up questions

Post-Testing:
□ Analyze recordings
□ Identify patterns
□ Prioritize issues by severity
□ Create recommendations
□ Share findings with team
```

## Design-to-Development Handoff

### Component Specifications

```markdown
# Button Component

## Variants
- Primary (CTA)
- Secondary (Alternative action)
- Tertiary (Low emphasis)
- Danger (Destructive action)

## Sizes
- Small: 32px height, 12px padding
- Medium: 40px height, 16px padding
- Large: 48px height, 20px padding

## States
- Default
- Hover (10% darker)
- Active (15% darker)
- Focus (2px outline, 2px offset)
- Disabled (50% opacity, no hover)
- Loading (spinner + disabled state)

## Accessibility
- Minimum touch target: 44x44px
- Focus visible indicator
- Supports keyboard navigation
- ARIA attributes as needed

## Code Example
```html
<button class="btn btn-primary btn-md">
    Click Me
</button>
```

## Design Tokens
- Primary color: var(--color-primary-500)
- Border radius: var(--border-radius-md)
- Transition: var(--transition-base)
```

### Figma to Code Guidelines

```markdown
# Design Handoff Checklist

## Spacing & Layout
□ Use 8px grid system
□ Specify padding and margins
□ Define breakpoints
□ Document flex/grid properties

## Typography
□ Font families with fallbacks
□ Font sizes in rem/em
□ Line heights
□ Letter spacing
□ Font weights

## Colors
□ Hex codes or design tokens
□ Opacity values
□ Gradient specifications
□ Color contrast ratios

## Interactive States
□ Hover effects
□ Focus styles
□ Active states
□ Disabled states
□ Loading states
□ Error states

## Animations
□ Transition properties
□ Duration and easing
□ Transform properties
□ Keyframe animations

## Assets
□ Export icons as SVG
□ Provide image variants
□ Document alt text
□ Specify lazy loading
```

## Performance and Best Practices

### Performance Guidelines

```css
/* ✅ Use CSS containment */
.card {
    contain: layout style paint;
}

/* ✅ Optimize animations */
.animated {
    /* GPU-accelerated properties only */
    transform: translateX(100px);
    opacity: 0.5;
    will-change: transform, opacity;
}

/* ❌ Avoid expensive properties */
.slow {
    box-shadow: 0 0 50px rgba(0,0,0,0.5); /* Expensive */
    filter: blur(10px); /* Very expensive */
}

/* ✅ Use CSS custom properties for themes */
[data-theme="dark"] {
    --bg-primary: #1a1a1a;
    --text-primary: #ffffff;
}

/* ✅ Lazy load images */
img {
    loading: lazy;
    decoding: async;
}
```

### Best Practices Checklist

```markdown
## Accessibility
□ WCAG 2.1 AA compliance minimum
□ Keyboard navigation works
□ Screen reader tested
□ Color contrast validated
□ Focus indicators visible
□ ARIA used correctly

## Responsive
□ Mobile-first approach
□ Touch targets 44x44px minimum
□ Text scalable to 200%
□ No horizontal scrolling
□ Breakpoints tested

## Performance
□ Images optimized
□ CSS/JS minified
□ Fonts subset and preloaded
□ Critical CSS inlined
□ Lazy loading implemented

## Usability
□ Clear visual hierarchy
□ Consistent patterns
□ Error messages helpful
□ Loading states present
□ Success feedback provided

## Browser Support
□ Tested in Chrome/Edge
□ Tested in Firefox
□ Tested in Safari
□ Graceful degradation
□ Progressive enhancement
```

## Tools and Resources

### Design Tools
- Figma - Interface design and prototyping
- Adobe XD - Design and prototyping
- Sketch - macOS design tool
- Penpot - Open-source alternative

### Accessibility Testing
- axe DevTools - Browser extension
- WAVE - Web accessibility evaluator
- Lighthouse - Chrome DevTools
- Screen readers: NVDA, JAWS, VoiceOver

### Color Tools
- WebAIM Contrast Checker
- Coolors - Color palette generator
- ColorBox - Color system builder

### Prototyping
- ProtoPie - Interactive prototypes
- Principle - Animation tool
- Framer - Code-based prototyping

## Resources

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [A11y Project](https://www.a11yproject.com/)
- [Inclusive Components](https://inclusive-components.design/)
- [Laws of UX](https://lawsofux.com/)
- [Nielsen Norman Group](https://www.nngroup.com/)
- [Material Design](https://material.io/)
- [Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/)
