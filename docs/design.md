# UI Design — MultiTenantShop

## Design Principles

1. **Competent, calm, fast** — daily driver for shop owners. No splash pages. No wasted motion.
2. **10 minutes to first sale** — every screen answers "what do I do next?".
3. **Bilingual first-class** — Persian (RTL) and English (LTR), user-selectable per admin account.

---

## Font

### Primary: Vazirmatn

Open-source, variable-weight humanist sans serif. Covers both Persian and Latin scripts with harmonious rhythm.

- Google Fonts / CDN
- Single `font-family: "Vazirmatn"` for the entire app
- No separate font for English — Vazirmatn's Latin glyphs are based on Inter

### Typography Scale

- **Body**: 16px (Latin), same size works for Persian at normal weight
- **Small / labels**: 14px
- **H1**: 28px — only on dashboards and page titles
- **H2**: 20px
- **Line-height**: 1.6 (both scripts)

---

## Layout & Navigation

### Admin Shell (shop owner + platform admin shared)

```
+------------------------------------------+
| Top bar  |  Tenant name | Lang | Avatar |  ← fixed, 56px
+----------+-------------------------------+
|          |                                |
|  Nav     |  Content area                  |
|  sidebar |  (MudContainer / Daisy        |
|  240px   |   container-fluid)            |
|          |                                |
|          |  Breadcrumb                    |
|          |  + page content                |
|          |                                |
+----------+--------------------------------+
```

- **Sidebar**: collapsible, on the right in RTL, on the left in LTR
- **Top bar**: tenant name, quick actions (add product, view orders), language toggle, user avatar
- **Content**: spacious padding, max-width 1400px, centered

### Two Modes

| Mode | Who | Nav Items | Color |
|---|---|---|---|
| **Admin** | Shop owner | Dashboard, Orders, Products, Customers, Settings, Billing | Tenant brand accent |
| **Super admin** | Platform staff | All of above + Tenants, Plans, Global settings | Neutral (slate/gray) |

Same layout component, different nav items injected based on role.

---

---

## Icons

Two icon sets used side by side:

| Set | Where |
|---|---|
| **Heroicons** (outline) | Navigation sidebar, top bar actions, empty states — clean, consistent stroke weight |
| **Lucide** | Data tables (sort, edit, delete, chevrons), status badges, inline actions — more expressive at small sizes |

Both are SVG-based, use `currentColor`, adapt to RTL automatically. No icon font files.

- DaisyUI + inline SVGs or simple `Icon` component that wraps either set
- 20px default for inline, 24px for nav/section icons
- No mixing on the same screen level — pick one per context

---

## Color & Theming

### DaisyUI Themes

- **Admin/Shop owner**: light mode by default with the tenant's brand color as `primary`
- **Super admin**: neutral gray-based theme
- **Dark mode**: toggleable per user, stored in preference

### Palette

| Token | Usage |
|---|---|
| `primary` | Buttons, active nav, links — one per tenant |
| `secondary` | Badges, highlights |
| `neutral` / `base-100` | Cards, surfaces, backgrounds |
| `info` / `success` / `warning` / `error` | Status badges, order states, alerts |

Tenant brand colors only appear in:
- Primary buttons
- Active nav item
- Status badges (if applicable)
- Logo area

Never paint large areas with brand colors — keep the UI calm.

---

## RTL / Bilingual

- `<html dir="rtl" />` or `"ltr"` based on user locale preference
- DaisyUI RTL support via `dir` attribute (flips grid, flex, margin/padding)
- `dir="auto"` on any user-generated content block (product descriptions, etc.)
- Language toggle in top bar — stored in user preference, not tenant-wide
- No layout direction toggle per page — it's a full-app concern

---

## Components & Patterns

### Data Tables

- Paginated, sortable, filterable
- Row actions (edit, delete, duplicate) as icon buttons or dropdown
- Empty state with call-to-action
- Right-align numbers in RTL mode

### Forms

- One page per entity (not wizards)
- Clear section headers
- Save / Cancel at top-right and bottom
- Validation inline, below fields

### Detail Panels

- Slide-over drawer (not full page nav) for viewing/editing a single record within a list
- Sidebar drawer pattern — keeps context

### Dashboard

- Summary cards (today's orders, revenue, low-stock count)
- Recent orders table
- Quick action button row
- Sparkline charts (opt-in, not blocking)

### Status Badges

- `pending` → warning
- `processing` → info
- `shipped` → success
- `cancelled` → error
- `refunded` → neutral / secondary

---

## Empty States

Every list page has a helpful empty state:

> **No products yet**
>
> Add your first product, or import from CSV.
>
> [Add Product] [Import CSV]

---

## Dark Mode

- DaisyUI `data-theme="dark"` toggle
- SVG icons use `currentColor` so they adapt
- All component colors use DaisyUI semantic tokens, never hardcoded
- User preference persisted, applied on load before paint

---

## Screen-by-Screen (to be expanded)

- [ ] **Login** — clean centered card, no hero/images
- [ ] **Dashboard** — summary cards + recent orders + quick actions
- [ ] **Orders** — filterable table → slide-over detail → edit status
- [ ] **Products** — table/card toggle → slide-over edit → media upload
- [ ] **Customers** — table with order count, total spent
- [ ] **Settings** — shop info, payment config, shipping zones, team members
- [ ] **Tenants** (super admin only) — list, create, suspend, plan management
- [ ] **Billing** — subscription plan, invoices, payment method
