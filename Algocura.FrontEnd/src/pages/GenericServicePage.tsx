import { useCallback, useEffect, useMemo, useState } from "react";
import { Alert, Button, Snackbar, Stack } from "@mui/material";
import { apiRequest } from "../api/httpClient";
import { useAuth } from "../auth/AuthContext";
import { ErrorState } from "../components/ErrorState";
import { JsonTable } from "../components/JsonTable";
import { LoadingState } from "../components/LoadingState";
import { PageHeader } from "../components/PageHeader";
import { RecordDialog } from "../components/RecordDialog";
import { ReportingNavigation } from "../components/ReportingNavigation";
import type { JsonRecord, ServiceDefinition } from "../types/common";

function normalize(data: unknown): JsonRecord[] {
  if (Array.isArray(data)) return data.filter((item): item is JsonRecord => Boolean(item) && typeof item === "object" && !Array.isArray(item));
  if (data && typeof data === "object") {
    const record = data as JsonRecord;
    for (const key of ["items", "results", "data", "records"]) if (Array.isArray(record[key])) return normalize(record[key]);
    return [record];
  }
  return [];
}

function editablePayload(row: JsonRecord, fieldNames: string[]): JsonRecord {
  return Object.fromEntries(fieldNames.map((name) => [name, row[name]]));
}

function formatReportingValue(
  column: string,
  value: unknown
): string {
  if (column === "status") {
    const labels: Record<string, string> = {
      "1": "Draft",
      "2": "Active",
      "3": "Retired"
    };

    return labels[String(value)] ?? String(value ?? "");
  }

  if (column.endsWith("AtUtc") && value) {
    const date = new Date(String(value));

    if (!Number.isNaN(date.getTime())) {
      return date.toLocaleString();
    }
  }

  if (column === "id" && typeof value === "string" && value.length > 20) {
    return `${value.slice(0, 8)}…${value.slice(-4)}`;
  }

  if (value === null || value === undefined) return "";
  if (typeof value === "object") return JSON.stringify(value);
  return String(value);
}

export function GenericServicePage({ service }: { service: ServiceDefinition }) {
  const auth = useAuth();
  const [rows, setRows] = useState<JsonRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [dialog, setDialog] = useState<"create" | "edit" | null>(null);
  const [selected, setSelected] = useState<JsonRecord | null>(null);
  const [message, setMessage] = useState("");

  const canCreate = Boolean(service.crud?.create && auth.hasAnyRole(service.crud.createRoles ?? service.allowedRoles));
  const canEdit = Boolean(service.crud?.edit && auth.hasAnyRole(service.crud.editRoles ?? service.allowedRoles));
  const canDelete = Boolean(service.crud?.delete && auth.hasAnyRole(service.crud.deleteRoles ?? service.allowedRoles));

  const load = useCallback(async () => {
    setLoading(true); setError("");
    try { setRows(normalize(await apiRequest<unknown>(service.baseUrl, service.primaryEndpoint))); }
    catch (exception) { setError(exception instanceof Error ? exception.message : "Failed to load data."); }
    finally { setLoading(false); }
  }, [service]);

  useEffect(() => { void load(); }, [load]);

  const formFields = useMemo(
    () => dialog === "edit" ? (service.crud?.editFields ?? service.crud?.createFields ?? []) : (service.crud?.createFields ?? []),
    [dialog, service]
  );

  const initialValue = useMemo(
    () => dialog === "edit" && selected
      ? editablePayload(selected, formFields.map((field) => field.name))
      : (service.crud?.createTemplate ?? {}),
    [dialog, selected, service, formFields]
  );

  async function save(value: JsonRecord) {
    const id = selected?.id;
    const isEdit = dialog === "edit";
    if (isEdit && !id) throw new Error("The selected record does not contain an ID.");
    await apiRequest(service.baseUrl, isEdit ? `${service.primaryEndpoint}/${id}` : service.primaryEndpoint, {
      method: isEdit ? "PUT" : "POST",
      body: JSON.stringify(value)
    });
    setMessage(isEdit ? "Record updated." : "Record created.");
    await load();
  }

  async function remove(row: JsonRecord) {
    if (!row.id) { setError("The selected record does not contain an ID."); return; }
    if (!window.confirm("Delete this record? This action cannot be undone.")) return;
    try {
      await apiRequest(service.baseUrl, `${service.primaryEndpoint}/${row.id}`, { method: "DELETE" });
      setMessage("Record deleted.");
      await load();
    } catch (exception) { setError(exception instanceof Error ? exception.message : "Failed to delete record."); }
  }

  return (
    <>
      <PageHeader title={service.name} description={service.description} />

      {service.key === "reporting" ? (
        <ReportingNavigation />
      ) : null}

      <Stack
        direction="row"
        spacing={1}
        sx={{
          justifyContent: "flex-end",
          mb: 2
        }}>
        {canCreate ? <Button variant="contained" onClick={() => { setSelected(null); setDialog("create"); }}>Add</Button> : null}
        <Button variant="outlined" onClick={() => void load()}>Refresh</Button>
      </Stack>
      {loading ? <LoadingState /> : null}
      {error ? <ErrorState message={error} /> : null}
      {!loading && !error ? (
        <JsonTable
          rows={rows}
          canEdit={canEdit}
          canDelete={canDelete}
          valueFormatter={
            service.key === "reporting"
              ? (column, value) =>
                  formatReportingValue(
                    column,
                    value
                  )
              : undefined
          }
          onEdit={(row) => {
            setSelected(row);
            setDialog("edit");
          }}
          onDelete={(row) =>
            void remove(row)
          }
        />
      ) : null}
      <RecordDialog
        open={dialog !== null}
        title={dialog === "edit" ? `Edit ${service.shortName}` : `Add ${service.shortName}`}
        fields={formFields}
        initialValue={initialValue}
        onClose={() => setDialog(null)}
        onSubmit={save}
      />
      <Snackbar open={Boolean(message)} autoHideDuration={3000} onClose={() => setMessage("")}>
        <Alert severity="success" onClose={() => setMessage("")}>{message}</Alert>
      </Snackbar>
    </>
  );
}
