// Runs before every test file: adds matchers such as toBeInTheDocument().
import "@testing-library/jest-dom/vitest";
import { afterEach } from "vitest";
import { cleanup } from "@testing-library/react";

afterEach(() => {
  cleanup();
  sessionStorage.clear();
});
