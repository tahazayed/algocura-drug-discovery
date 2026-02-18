import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { RecordDialog } from "./RecordDialog";
import type { FormFieldDefinition } from "../types/common";

const fields: FormFieldDefinition[] = [
  { name: "name", label: "Name", type: "text", required: true },
  { name: "description", label: "Description", type: "textarea" },
  { name: "status", label: "Status", type: "select", required: true, options: [
    { label: "Draft", value: 1 },
    { label: "Active", value: 2 }
  ] },
  { name: "score", label: "Score", type: "number", min: 0, max: 100 },
  { name: "authors", label: "Authors", type: "string-list" },
  { name: "enabled", label: "Enabled", type: "checkbox" }
];

describe("RecordDialog", () => {
  it("renders real form controls rather than one JSON request textbox", () => {
    render(
      <RecordDialog
        open
        title="Add Record"
        fields={fields}
        initialValue={{ status: 1, enabled: true }}
        onClose={() => undefined}
        onSubmit={async () => undefined}
      />
    );

    expect(screen.getByRole("textbox", { name: "Name" })).toBeInTheDocument();
    expect(screen.getByRole("textbox", { name: "Description" })).toBeInTheDocument();
    expect(screen.getByRole("combobox", { name: "Status" })).toBeInTheDocument();
    expect(screen.getByRole("spinbutton", { name: "Score" })).toBeInTheDocument();
    expect(screen.getByRole("textbox", { name: "Authors" })).toBeInTheDocument();
    expect(screen.getByRole("checkbox", { name: "Enabled" })).toBeChecked();
    expect(screen.queryByLabelText("request body")).not.toBeInTheDocument();
  });

  it("builds the API payload from the controls", async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(
      <RecordDialog
        open
        title="Add Record"
        fields={fields}
        initialValue={{ status: 1, enabled: false }}
        onClose={() => undefined}
        onSubmit={onSubmit}
      />
    );

    fireEvent.change(screen.getByRole("textbox", { name: "Name" }), { target: { value: "Example" } });
    fireEvent.change(screen.getByRole("spinbutton", { name: "Score" }), { target: { value: "42.5" } });
    fireEvent.change(screen.getByRole("textbox", { name: "Authors" }), { target: { value: "Ada Lovelace\nGrace Hopper" } });
    fireEvent.click(screen.getByRole("checkbox", { name: "Enabled" }));
    fireEvent.click(screen.getByRole("button", { name: "Save" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledTimes(1));
    expect(onSubmit).toHaveBeenCalledWith(expect.objectContaining({
      name: "Example",
      status: 1,
      score: 42.5,
      authors: ["Ada Lovelace", "Grace Hopper"],
      enabled: true
    }));
  });

  it("shows required field validation", async () => {
    const onSubmit = vi.fn();
    render(
      <RecordDialog
        open
        title="Add Record"
        fields={fields}
        initialValue={{ status: 1 }}
        onClose={() => undefined}
        onSubmit={onSubmit}
      />
    );

    fireEvent.click(screen.getByRole("button", { name: "Save" }));
    expect(await screen.findByText("Name is required.")).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });
});
