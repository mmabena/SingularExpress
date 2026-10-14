import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { AuthProvider } from "../../context/AuthContext";
import * as authService from "../../services/authService";
import SignIn from "./SignIn";
import { MESSAGES } from "../../constants/messages";

vi.mock("../../services/authService", () => ({ login: vi.fn(), logout: vi.fn() }));

const renderSignIn = () =>
  render(
    <MemoryRouter initialEntries={["/login"]}>
      <AuthProvider>
        <SignIn />
      </AuthProvider>
    </MemoryRouter>,
  );

describe("SignIn", () => {
  beforeEach(() => vi.clearAllMocks());

  it("rejects emails outside @singular.co.za without calling the API", async () => {
    renderSignIn();
    await userEvent.type(screen.getByLabelText("Email"), "someone@gmail.com");
    await userEvent.type(screen.getByLabelText("Password"), "Secret-1!");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    expect(screen.getByRole("alert")).toHaveTextContent(MESSAGES.EMAIL_DOMAIN);
    expect(authService.login).not.toHaveBeenCalled();
  });

  it("shows the server's error message when sign-in fails", async () => {
    authService.login.mockRejectedValue({ status: 401, errors: ["Invalid email or password."] });
    renderSignIn();
    await userEvent.type(screen.getByLabelText("Email"), "test.user@singular.co.za");
    await userEvent.type(screen.getByLabelText("Password"), "wrong");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Invalid email or password.");
  });

  it("can show and hide the password", async () => {
    renderSignIn();
    const password = screen.getByLabelText("Password");
    expect(password).toHaveAttribute("type", "password");

    await userEvent.click(screen.getByRole("button", { name: "Show" }));

    expect(password).toHaveAttribute("type", "text");
  });
});
