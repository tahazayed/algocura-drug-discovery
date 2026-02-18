import { useEffect, useMemo, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  TextField
} from "@mui/material";
import type { FormFieldDefinition, JsonRecord } from "../types/common";

interface RecordDialogProps {
  open: boolean;
  title: string;
  fields: FormFieldDefinition[];
  initialValue: JsonRecord;
  onClose: () => void;
  onSubmit: (value: JsonRecord) => Promise<void>;
}

type FormErrors = Record<string, string>;

function normalizeInitialValue(fields: FormFieldDefinition[], initialValue: JsonRecord): JsonRecord {
  const result: JsonRecord = {};

  for (const field of fields) {
    const value = initialValue[field.name];

    if (field.type === "checkbox") {
      result[field.name] = Boolean(value);
    } else if (field.type === "string-list") {
      result[field.name] = Array.isArray(value) ? value.join("\n") : String(value ?? "");
    } else {
      result[field.name] = value ?? "";
    }
  }

  return result;
}

function validate(fields: FormFieldDefinition[], values: JsonRecord): FormErrors {
  const errors: FormErrors = {};

  for (const field of fields) {
    const value = values[field.name];

    if (field.required && field.type !== "checkbox" && String(value ?? "").trim() === "") {
      errors[field.name] = `${field.label} is required.`;
      continue;
    }

    if (field.type === "number" && value !== "" && value !== null && value !== undefined) {
      const number = Number(value);
      if (!Number.isFinite(number)) errors[field.name] = `${field.label} must be a number.`;
      else if (field.min !== undefined && number < field.min) errors[field.name] = `${field.label} must be at least ${field.min}.`;
      else if (field.max !== undefined && number > field.max) errors[field.name] = `${field.label} must be at most ${field.max}.`;
    }
  }

  return errors;
}

function buildPayload(fields: FormFieldDefinition[], values: JsonRecord): JsonRecord {
  const payload: JsonRecord = {};

  for (const field of fields) {
    const value = values[field.name];

    switch (field.type) {
      case "number":
        payload[field.name] = value === "" || value === null || value === undefined ? null : Number(value);
        break;
      case "checkbox":
        payload[field.name] = Boolean(value);
        break;
      case "string-list":
        payload[field.name] = String(value ?? "")
          .split(/[\n,]/)
          .map((item) => item.trim())
          .filter(Boolean);
        break;
      case "date":
        payload[field.name] = String(value ?? "").trim() || null;
        break;
      case "select": {
        if (value === "" || value === null || value === undefined) {
          payload[field.name] = field.required ? value : null;
          break;
        }

        const selectedOption = (field.options ?? []).find(
          (option) => String(option.value) === String(value)
        );

        payload[field.name] = selectedOption?.value ?? value;
        break;
      }
      default: {
        const text = String(value ?? "");
        payload[field.name] = text.trim() === "" && !field.required ? null : text;
        break;
      }
    }
  }

  return payload;
}

export function RecordDialog({ open, title, fields, initialValue, onClose, onSubmit }: RecordDialogProps) {
  const normalized = useMemo(() => normalizeInitialValue(fields, initialValue), [fields, initialValue]);
  const [values, setValues] = useState<JsonRecord>(normalized);
  const [fieldErrors, setFieldErrors] = useState<FormErrors>({});
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (open) {
      setValues(normalized);
      setFieldErrors({});
      setError("");
    }
  }, [open, normalized]);

  function setValue(name: string, value: unknown) {
    setValues((current) => ({ ...current, [name]: value }));
    setFieldErrors((current) => ({ ...current, [name]: "" }));
  }

  async function submit() {
    const errors = validate(fields, values);
    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    try {
      setSaving(true);
      setError("");
      await onSubmit(buildPayload(fields, values));
      onClose();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Unable to save the record.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth="md">
      <DialogTitle>{title}</DialogTitle>
      <DialogContent dividers>
        {error ? <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert> : null}

        <Box
          component="form"
          noValidate
          sx={{
            display: "grid",
            gridTemplateColumns: { xs: "1fr", md: "repeat(2, minmax(0, 1fr))" },
            gap: 2,
            pt: 0.5
          }}
        >
          {fields.map((field) => {
            const value = values[field.name];
            const fieldError = fieldErrors[field.name];
            const fullWidth = field.type === "textarea" || field.type === "string-list";

            if (field.type === "checkbox") {
              return (
                <FormControl key={field.name} error={Boolean(fieldError)} sx={{ gridColumn: fullWidth ? "1 / -1" : undefined }}>
                  <FormControlLabel
                    control={<Checkbox checked={Boolean(value)} onChange={(event) => setValue(field.name, event.target.checked)} />}
                    label={field.label}
                  />
                  {field.helperText || fieldError ? <FormHelperText>{fieldError || field.helperText}</FormHelperText> : null}
                </FormControl>
              );
            }

            if (field.type === "select") {
              return (
                <FormControl key={field.name} fullWidth required={field.required} error={Boolean(fieldError)}>
                  <InputLabel id={`${field.name}-label`}>{field.label}</InputLabel>
                  <Select
                    labelId={`${field.name}-label`}
                    label={field.label}
                    value={value ?? ""}
                    onChange={(event) => {
                      const selectedValue = event.target.value;
                      const selectedOption = (field.options ?? []).find(
                        (option) => String(option.value) === String(selectedValue)
                      );

                      setValue(field.name, selectedOption?.value ?? selectedValue);
                    }}
                  >
                    {!field.required ? <MenuItem value=""><em>None</em></MenuItem> : null}
                    {(field.options ?? []).map((option) => (
                      <MenuItem key={String(option.value)} value={option.value}>{option.label}</MenuItem>
                    ))}
                  </Select>
                  {field.helperText || fieldError ? <FormHelperText>{fieldError || field.helperText}</FormHelperText> : null}
                </FormControl>
              );
            }

            return (
              <TextField
                key={field.name}
                label={field.label}
                value={value ?? ""}
                required={field.required}
                type={field.type === "number" ? "number" : field.type === "date" ? "date" : "text"}
                multiline={field.type === "textarea" || field.type === "string-list"}
                minRows={field.rows ?? (field.type === "textarea" ? 4 : field.type === "string-list" ? 3 : undefined)}
                placeholder={field.placeholder}
                error={Boolean(fieldError)}
                helperText={fieldError || field.helperText}
                onChange={(event) => setValue(field.name, event.target.value)}
                slotProps={{
                  inputLabel: field.type === "date" ? { shrink: true } : undefined,
                  htmlInput: field.type === "number" ? { min: field.min, max: field.max, step: field.step ?? "any" } : undefined
                }}
                sx={{ gridColumn: fullWidth ? "1 / -1" : undefined }}
                fullWidth
              />
            );
          })}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={saving}>Cancel</Button>
        <Button variant="contained" onClick={() => void submit()} disabled={saving || fields.length === 0}>
          {saving ? "Saving..." : "Save"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
