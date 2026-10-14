# Client

This is the React 19 front end, built with Vite.

```bash
npm install
npm run dev            # http://localhost:5173 (calls to /api go to the Server)
npm test               # unit tests (Vitest + Testing Library)
npm run build
```

| Folder (`src/`) | Holds |
|---|---|
| `api/` | The single axios instance (token and error shape) |
| `services/` | One file per API area. **Components call these, never axios.** |
| `context/` | `AuthContext` for who is signed in |
| `hooks/` | Reusable data-loading hooks |
| `components/` | Reusable UI pieces, one folder each |
| `Pages/` | Full pages, one folder each |
| `constants/` | Routes, roles, config and client-side messages |
| `utils/` | Pure helpers (validators, role checks) |
| `styles/` | Design tokens (`global.css`) |

Tests sit next to the file they test (`authService.test.js`).
