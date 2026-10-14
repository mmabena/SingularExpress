import { describe, it, expect } from "vitest";
import { isAllowedEmail, isStrongPassword, isValidPin } from "./validators";

describe("validators", () => {
  it.each([
    ["test.user@singular.co.za", true],
    [" TEST@SINGULAR.CO.ZA ", true],
    ["test@gmail.com", false],
    ["@singular.co.za", false],
    ["", false],
    [undefined, false],
  ])("isAllowedEmail(%s) is %s", (email, expected) => {
    expect(isAllowedEmail(email)).toBe(expected);
  });

  it.each([
    ["Correct-Horse-1!", true],
    ["short1!", false],
    ["nouppercase1!", false],
    ["NOLOWERCASE1!", false],
    ["NoDigits!!", false],
    ["NoSpecial123", false],
  ])("isStrongPassword(%s) is %s", (password, expected) => {
    expect(isStrongPassword(password)).toBe(expected);
  });

  it.each([
    ["123456", true],
    ["12345", false],
    ["1234567", false],
    ["12a456", false],
  ])("isValidPin(%s) is %s", (pin, expected) => {
    expect(isValidPin(pin)).toBe(expected);
  });
});
