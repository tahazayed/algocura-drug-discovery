import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { JsonTable, formatColumnName } from "./JsonTable";

describe("formatColumnName", () => {
  it("formats camelCase API fields as readable headers", () => {
    expect(formatColumnName("serviceName")).toBe("Service Name");
    expect(formatColumnName("createdAtUtc")).toBe("Created At UTC");
    expect(formatColumnName("id")).toBe("ID");
  });
});

describe("JsonTable", () => {
  const rows = [{ id: "1", serviceName: "Reporting", createdAtUtc: "2026-08-12T00:00:00Z" }];

  it("renders a visible sticky header with formatted column names", () => {
    render(<JsonTable rows={rows} />);
    expect(screen.getByRole("columnheader", { name: "ID" })).toBeInTheDocument();
    expect(screen.getByRole("columnheader", { name: "Service Name" })).toBeInTheDocument();
    expect(screen.getByRole("columnheader", { name: "Created At UTC" })).toBeInTheDocument();
  });

  it("renders Edit and Delete only when authorized capabilities are supplied", () => {
    const onEdit = vi.fn();
    const onDelete = vi.fn();
    render(<JsonTable rows={rows} canEdit canDelete onEdit={onEdit} onDelete={onDelete} />);

    fireEvent.click(screen.getByRole("button", { name: "Edit" }));
    fireEvent.click(screen.getByRole("button", { name: "Delete" }));

    expect(onEdit).toHaveBeenCalledWith(rows[0]);
    expect(onDelete).toHaveBeenCalledWith(rows[0]);
  });

  it("does not render mutation actions for read-only users/pages", () => {
    render(<JsonTable rows={rows} />);
    expect(screen.queryByRole("button", { name: "Edit" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Delete" })).not.toBeInTheDocument();
    expect(screen.queryByRole("columnheader", { name: "Actions" })).not.toBeInTheDocument();
  });
});
