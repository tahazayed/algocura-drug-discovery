import { useState, type FormEvent } from "react";
import {
  Box,
  Button,
  Stack,
  TextField
} from "@mui/material";
import { apiRequest } from "../api/httpClient";
import { serviceByKey } from "../config/services";
import { ErrorState } from "../components/ErrorState";
import { JsonTable } from "../components/JsonTable";
import { LoadingState } from "../components/LoadingState";
import { PageHeader } from "../components/PageHeader";
import type { JsonRecord } from "../types/common";

const service = serviceByKey.search;

function normalize(data: unknown): JsonRecord[] {
  if (Array.isArray(data)) {
    return data as JsonRecord[];
  }

  if (data && typeof data === "object") {
    const record = data as JsonRecord;
    const candidates = [
      record.items,
      record.results,
      record.documents
    ];
    const array = candidates.find(Array.isArray);
    return Array.isArray(array)
      ? (array as JsonRecord[])
      : [record];
  }

  return [];
}

export function SearchPage() {
  const [query, setQuery] = useState("");
  const [rows, setRows] = useState<JsonRecord[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  async function submit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError("");

    try {
      const result = await apiRequest<unknown>(
        service.baseUrl,
        `/api/v1/search?query=${encodeURIComponent(
          query
        )}`
      );
      setRows(normalize(result));
    } catch (exception) {
      setError(
        exception instanceof Error
          ? exception.message
          : "Search failed."
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <PageHeader
        title="Search"
        description="Search across Algocura discovery projections."
      />

      <Box component="form" onSubmit={submit} sx={{
        mb: 3
      }}>
        <Stack direction="row" sx={{
          gap: 1
        }}>
          <TextField
            fullWidth
            label="Search"
            value={query}
            onChange={(event) =>
              setQuery(event.target.value)
            }
          />
          <Button
            type="submit"
            variant="contained"
            disabled={!query.trim() || busy}
          >
            Search
          </Button>
        </Stack>
      </Box>

      {busy ? <LoadingState /> : null}
      {error ? <ErrorState message={error} /> : null}
      {!busy && !error ? (
        <JsonTable rows={rows} />
      ) : null}
    </>
  );
}
