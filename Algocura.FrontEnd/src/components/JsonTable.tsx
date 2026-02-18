import {
  Button,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography
} from "@mui/material";
import type { JsonRecord } from "../types/common";

const abbreviations = new Set(["id", "utc", "api", "url", "uri", "ai", "ml", "ip", "http", "https", "json", "xml", "sql", "ui", "uuid", "guid"]);

export function formatColumnName(value: string): string {
  if (!value) return "";
  return value
    .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
    .replace(/([A-Z]+)([A-Z][a-z])/g, "$1 $2")
    .replace(/[_-]+/g, " ")
    .trim()
    .split(/\s+/)
    .map((word) => abbreviations.has(word.toLowerCase()) ? word.toUpperCase() : word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");
}

function stringify(value: unknown): string {
  if (value === null || value === undefined) return "";
  if (typeof value === "object") return JSON.stringify(value);
  return String(value);
}

interface JsonTableProps {
  rows: JsonRecord[];
  canEdit?: boolean;
  canDelete?: boolean;
  onEdit?: (row: JsonRecord) => void;
  onDelete?: (row: JsonRecord) => void;
  valueFormatter?: (
    column: string,
    value: unknown,
    row: JsonRecord
  ) => string;
}

export function JsonTable({
  rows,
  canEdit = false,
  canDelete = false,
  onEdit,
  onDelete,
  valueFormatter
}: JsonTableProps) {
  if (rows.length === 0) {
    return (
      <Paper sx={{ p: 3 }}><Typography sx={{
        color: "text.secondary"
      }}>No records returned.</Typography></Paper>
    );
  }

  const columns = Array.from(new Set(rows.flatMap((row) => Object.keys(row)))).slice(0, 9);
  const hasActions = canEdit || canDelete;

  return (
    <TableContainer component={Paper} sx={{ maxHeight: "calc(100vh - 260px)", overflow: "auto" }}>
      <Table size="small" stickyHeader aria-label="service data table">
        <TableHead>
          <TableRow>
            {columns.map((column) => (
              <TableCell key={column} sx={{ bgcolor: "background.paper", fontWeight: 700, whiteSpace: "nowrap", zIndex: 3 }}>
                {formatColumnName(column)}
              </TableCell>
            ))}
            {hasActions ? (
              <TableCell align="right" sx={{ bgcolor: "background.paper", fontWeight: 700, whiteSpace: "nowrap", position: "sticky", right: 0, zIndex: 4 }}>
                Actions
              </TableCell>
            ) : null}
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row, index) => (
            <TableRow key={String(row.id ?? index)} hover>
              {columns.map((column) => {
                const rawValue = row[column];
                const displayValue = valueFormatter
                  ? valueFormatter(column, rawValue, row)
                  : stringify(rawValue);

                return (
                  <TableCell
                    key={column}
                    title={stringify(rawValue)}
                    sx={{
                      whiteSpace:
                        column === "description"
                          ? "normal"
                          : "nowrap",
                      maxWidth:
                        column === "description"
                          ? 320
                          : undefined,
                      minWidth:
                        column === "description"
                          ? 220
                          : undefined
                    }}
                  >
                    {displayValue}
                  </TableCell>
                );
              })}
              {hasActions ? (
                <TableCell align="right" sx={{ position: "sticky", right: 0, bgcolor: "background.paper" }}>
                  <Stack direction="row" spacing={1} sx={{ justifyContent: "flex-end" }}>
                    {canEdit ? <Button size="small" onClick={() => onEdit?.(row)}>Edit</Button> : null}
                    {canDelete ? <Button size="small" color="error" onClick={() => onDelete?.(row)}>Delete</Button> : null}
                  </Stack>
                </TableCell>
              ) : null}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
