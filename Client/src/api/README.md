# api

`apiClient.js` is the only axios instance. It adds the JWT and turns every error into `{ status, errors: [...] }`. On a 401 it clears the session and signs the user out.
