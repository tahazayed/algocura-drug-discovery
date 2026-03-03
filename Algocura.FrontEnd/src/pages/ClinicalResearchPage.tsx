import { useCallback, useEffect, useMemo, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Snackbar,
  Stack,
  TextField,
  Typography
} from "@mui/material";
import { apiRequest, tokenStorage } from "../api/httpClient";
import { useAuth } from "../auth/AuthContext";
import { ErrorState } from "../components/ErrorState";
import { JsonTable } from "../components/JsonTable";
import { LoadingState } from "../components/LoadingState";
import { PageHeader } from "../components/PageHeader";
import { RecordDialog } from "../components/RecordDialog";
import type { JsonRecord, ServiceDefinition } from "../types/common";

interface ClinicalDataset extends JsonRecord {
  id: string;
  name: string;
  sourceName: string;
  sourceUri?: string | null;
  studyCode?: string | null;
  diseaseArea?: string | null;
  description?: string | null;
  status: number | string;
  subjectCount: number;
}


interface TciaSeries extends JsonRecord {
  collection: string;
  patientId: string;
  studyInstanceUid: string;
  seriesInstanceUid: string;
  modality?: string | null;
  bodyPartExamined?: string | null;
  seriesDescription?: string | null;
  imageCount?: number | null;
  studyDate?: string | null;
}

interface ResponseEstimate extends JsonRecord {
  researchResponseScore: number;
  modelName: string;
  modelVersion: string;
  featureContributions: Record<string, number>;
  disclaimer: string;
}

interface ResearchSubject extends JsonRecord {
  id: string;
  clinicalDatasetId: string;
  externalSubjectId: string;
  ageAtDiagnosis?: number | null;
  sex?: string | null;
  primarySite?: string | null;
  diseaseStage?: string | null;
  diagnosis?: string | null;
}

const sampleImport = JSON.stringify(
  {
    subjects: [
      {
        externalSubjectId: "CASE-001",
        ageAtDiagnosis: 63,
        sex: "Female",
        primarySite: "Lung",
        diseaseStage: "Stage IIIA",
        vitalStatus: "Alive",
        diagnosis: "Lung adenocarcinoma",
        treatmentSummary: "Public research dataset example",
        molecularFeaturesJson: "{\"EGFR\":\"Mutation\",\"TP53\":\"Mutation\"}",
        additionalAttributesJson: "{\"sourceFile\":\"clinical.tsv\"}"
      }
    ]
  },
  null,
  2
);

function normalize<T extends JsonRecord>(value: unknown): T[] {
  return Array.isArray(value)
    ? value.filter((item): item is T => Boolean(item) && typeof item === "object" && !Array.isArray(item))
    : [];
}

export function ClinicalResearchPage({ service }: { service: ServiceDefinition }) {
  const auth = useAuth();
  const canWrite = auth.hasAnyRole(service.allowedRoles);
  const [datasets, setDatasets] = useState<ClinicalDataset[]>([]);
  const [subjects, setSubjects] = useState<ResearchSubject[]>([]);
  const [selectedDataset, setSelectedDataset] = useState<ClinicalDataset | null>(null);
  const [loading, setLoading] = useState(true);
  const [subjectsLoading, setSubjectsLoading] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [datasetDialog, setDatasetDialog] = useState<"create" | "edit" | null>(null);
  const [importOpen, setImportOpen] = useState(false);
  const [importJson, setImportJson] = useState(sampleImport);
  const [importing, setImporting] = useState(false);

  const [tciaCollection, setTciaCollection] = useState("NSCLC-Radiomics");
  const [tciaPatientId, setTciaPatientId] = useState("1.3.6.1.4.1.9328.50.1.0001");
  const [tciaSeries, setTciaSeries] = useState<TciaSeries[]>([]);
  const [tciaLoading, setTciaLoading] = useState(false);
  const [age, setAge] = useState("61");
  const [tumorBurden, setTumorBurden] = useState("0.4");
  const [pdL1, setPdL1] = useState("0.8");
  const [tmb, setTmb] = useState("0.7");
  const [immuneInfiltration, setImmuneInfiltration] = useState("0.6");
  const [priorTreatmentBurden, setPriorTreatmentBurden] = useState("0.2");
  const [responseEstimate, setResponseEstimate] = useState<ResponseEstimate | null>(null);
  const [ctImage, setCtImage] = useState<File | null>(null);
  const [lesionX, setLesionX] = useState("256");
  const [lesionY, setLesionY] = useState("256");
  const [lesionRadius, setLesionRadius] = useState("40");
  const [simulatedImageUrl, setSimulatedImageUrl] = useState<string | null>(null);

  const datasetFields = useMemo(() => service.crud?.createFields ?? [], [service]);

  const loadDatasets = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const result = await apiRequest<unknown>(service.baseUrl, service.primaryEndpoint);
      const rows = normalize<ClinicalDataset>(result);
      setDatasets(rows);
      setSelectedDataset((current) => current ? rows.find((x) => x.id === current.id) ?? null : null);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load clinical datasets.");
    } finally {
      setLoading(false);
    }
  }, [service]);

  const loadSubjects = useCallback(async (dataset: ClinicalDataset | null) => {
    if (!dataset) {
      setSubjects([]);
      return;
    }
    setSubjectsLoading(true);
    try {
      const result = await apiRequest<unknown>(
        service.baseUrl,
        `/api/v1/research-subjects?datasetId=${encodeURIComponent(dataset.id)}`
      );
      setSubjects(normalize<ResearchSubject>(result));
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load research subjects.");
    } finally {
      setSubjectsLoading(false);
    }
  }, [service.baseUrl]);

  useEffect(() => { void loadDatasets(); }, [loadDatasets]);
  useEffect(() => { void loadSubjects(selectedDataset); }, [loadSubjects, selectedDataset]);

  async function saveDataset(value: JsonRecord) {
    const isEdit = datasetDialog === "edit";
    const id = selectedDataset?.id;
    await apiRequest(service.baseUrl, isEdit ? `${service.primaryEndpoint}/${id}` : service.primaryEndpoint, {
      method: isEdit ? "PUT" : "POST",
      body: JSON.stringify(value)
    });
    setMessage(isEdit ? "Clinical dataset updated." : "Clinical dataset registered.");
    await loadDatasets();
  }

  async function deleteDataset(row: JsonRecord) {
    if (!row.id || !window.confirm("Delete this clinical dataset and its imported research subjects?")) return;
    try {
      await apiRequest(service.baseUrl, `${service.primaryEndpoint}/${row.id}`, { method: "DELETE" });
      if (selectedDataset?.id === row.id) setSelectedDataset(null);
      setMessage("Clinical dataset deleted.");
      await loadDatasets();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to delete clinical dataset.");
    }
  }

  async function importSubjects() {
    if (!selectedDataset) return;
    try {
      setImporting(true);
      const payload = JSON.parse(importJson) as JsonRecord;
      const result = await apiRequest<{ insertedCount: number; updatedCount: number; totalSubjectCount: number }>(
        service.baseUrl,
        `/api/v1/research-subjects/datasets/${selectedDataset.id}/import`,
        { method: "POST", body: JSON.stringify(payload) }
      );
      setMessage(`Imported ${result.insertedCount} new and updated ${result.updatedCount}; ${result.totalSubjectCount} total subjects.`);
      setImportOpen(false);
      await Promise.all([loadDatasets(), loadSubjects(selectedDataset)]);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to import research subjects. Check the JSON payload.");
    } finally {
      setImporting(false);
    }
  }


  async function curateTciaSeries() {
    try {
      setTciaLoading(true);
      const result = await apiRequest<{ retrievedCount: number; curatedCount: number; series: TciaSeries[] }>(
        service.baseUrl,
        "/api/v1/imaging-research/tcia/curate",
        {
          method: "POST",
          body: JSON.stringify({ collection: tciaCollection, patientId: tciaPatientId, modality: "CT", minimumImageCount: 10 })
        }
      );
      setTciaSeries(result.series);
      setMessage(`Retrieved ${result.retrievedCount} series; ${result.curatedCount} CT series passed curation.`);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to retrieve TCIA imaging series.");
    } finally {
      setTciaLoading(false);
    }
  }


  async function downloadFirstTciaSeries() {
    const series = tciaSeries[0];
    if (!series) return;
    try {
      const headers = new Headers();
      const token = tokenStorage.get();
      if (token) headers.set("Authorization", `Bearer ${token}`);
      const response = await fetch(`${service.baseUrl}/api/v1/imaging-research/tcia/series/${encodeURIComponent(series.seriesInstanceUid)}/download`, { headers });
      if (!response.ok) throw new Error(`TCIA series download failed with status ${response.status}.`);
      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement("a");
      anchor.href = url;
      anchor.download = `tcia-${series.seriesInstanceUid}.zip`;
      document.body.appendChild(anchor);
      anchor.click();
      anchor.remove();
      URL.revokeObjectURL(url);
      setMessage("Downloaded the first curated TCIA DICOM series through Algocura.");
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to download TCIA imaging series.");
    }
  }

  async function estimateImmunotherapyResponse() {
    try {
      const result = await apiRequest<ResponseEstimate>(service.baseUrl, "/api/v1/imaging-research/immunotherapy-response/estimate", {
        method: "POST",
        body: JSON.stringify({
          age: Number(age),
          normalizedTumorBurden: Number(tumorBurden),
          normalizedPdL1: Number(pdL1),
          normalizedTmb: Number(tmb),
          normalizedImmuneInfiltration: Number(immuneInfiltration),
          priorTreatmentBurden: Number(priorTreatmentBurden),
          cancerType: "Research demo",
          seriesInstanceUid: tciaSeries[0]?.seriesInstanceUid ?? null
        })
      });
      setResponseEstimate(result);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to calculate experimental research score.");
    }
  }

  async function simulateFollowUpCt() {
    if (!ctImage) { setError("Select a rendered CT slice (PNG/JPEG) first."); return; }
    try {
      const form = new FormData();
      form.append("image", ctImage);
      form.append("responseScore", String(responseEstimate?.researchResponseScore ?? 0.5));
      form.append("lesionCenterX", lesionX);
      form.append("lesionCenterY", lesionY);
      form.append("lesionRadiusPixels", lesionRadius);
      const headers = new Headers();
      const token = tokenStorage.get();
      if (token) headers.set("Authorization", `Bearer ${token}`);
      const response = await fetch(`${service.baseUrl}/api/v1/imaging-research/simulate-followup-ct`, { method: "POST", headers, body: form });
      if (!response.ok) throw new Error(`Simulation failed with status ${response.status}.`);
      const blob = await response.blob();
      if (simulatedImageUrl) URL.revokeObjectURL(simulatedImageUrl);
      setSimulatedImageUrl(URL.createObjectURL(blob));
      setMessage("Generated research-only simulated follow-up CT visualization.");
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to generate simulated follow-up image.");
    }
  }

  return (
    <>
      <PageHeader
        title="Clinical Research"
        description="Register public or de-identified biomedical datasets, preserve source provenance, and ingest patient/research-subject records for downstream analysis."
      />

      <Stack direction="row" spacing={1} sx={{ justifyContent: "flex-end", mb: 2 }}>
        {canWrite ? <Button variant="contained" onClick={() => { setSelectedDataset(null); setDatasetDialog("create"); }}>Register Dataset</Button> : null}
        <Button variant="outlined" onClick={() => void loadDatasets()}>Refresh</Button>
      </Stack>

      {loading ? <LoadingState /> : null}
      {error ? <ErrorState message={error} /> : null}
      {!loading && !error ? (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h6" sx={{ mb: 1 }}>Datasets</Typography>
          <JsonTable
            rows={datasets}
            canEdit={canWrite}
            canDelete={canWrite}
            onEdit={(row) => { setSelectedDataset(row as ClinicalDataset); setDatasetDialog("edit"); }}
            onDelete={(row) => void deleteDataset(row)}
          />
          <TextField
            select
            fullWidth
            label="Active dataset"
            value={selectedDataset?.id ?? ""}
            onChange={(event) => setSelectedDataset(datasets.find((x) => x.id === event.target.value) ?? null)}
            sx={{ mt: 2, maxWidth: 700 }}
          >
            <MenuItem value=""><em>Select a dataset to view/import subjects</em></MenuItem>
            {datasets.map((dataset) => (
              <MenuItem key={dataset.id} value={dataset.id}>
                {dataset.name} — {dataset.sourceName}{dataset.studyCode ? ` (${dataset.studyCode})` : ""}
              </MenuItem>
            ))}
          </TextField>
        </Box>
      ) : null}

      {selectedDataset ? (
        <Box>
          <Stack direction="row" spacing={1} sx={{ alignItems: "center", justifyContent: "space-between", mb: 1 }}>
            <Typography variant="h6">Research Subjects — {selectedDataset.name}</Typography>
            {canWrite ? <Button variant="contained" onClick={() => setImportOpen(true)}>Import Subjects</Button> : null}
          </Stack>
          {selectedDataset.sourceUri ? (
            <Alert severity="info" sx={{ mb: 2 }}>
              Source provenance: {selectedDataset.sourceName} — {selectedDataset.sourceUri}
            </Alert>
          ) : null}
          {subjectsLoading ? <LoadingState /> : <JsonTable rows={subjects} />}
        </Box>
      ) : null}


      <Box sx={{ mt: 5 }}>
        <Typography variant="h5" sx={{ mb: 1 }}>Imaging + Experimental Response Research Demo</Typography>
        <Alert severity="warning" sx={{ mb: 2 }}>
          Research prototype only. Scores and simulated images are not clinically validated and must not be used for diagnosis, prognosis, or treatment decisions.
        </Alert>

        <Typography variant="h6" sx={{ mt: 2, mb: 1 }}>1. Retrieve and curate public TCIA CT series</Typography>
        <Stack direction={{ xs: "column", md: "row" }} spacing={1} sx={{ mb: 2 }}>
          <TextField label="TCIA collection" value={tciaCollection} onChange={(e) => setTciaCollection(e.target.value)} fullWidth />
          <TextField label="TCIA patient ID" value={tciaPatientId} onChange={(e) => setTciaPatientId(e.target.value)} fullWidth />
          <Button variant="contained" onClick={() => void curateTciaSeries()} disabled={tciaLoading}>{tciaLoading ? "Retrieving..." : "Retrieve + Curate"}</Button>
        </Stack>
        {tciaSeries.length ? (<>
          <JsonTable rows={tciaSeries} />
          <Button variant="outlined" sx={{ mt: 1 }} onClick={() => void downloadFirstTciaSeries()}>Download first curated DICOM series</Button>
        </>) : null}

        <Typography variant="h6" sx={{ mt: 3, mb: 1 }}>2. Experimental immunotherapy-response score</Typography>
        <Stack direction={{ xs: "column", md: "row" }} spacing={1} sx={{ mb: 1 }}>
          <TextField label="Age" value={age} onChange={(e) => setAge(e.target.value)} />
          <TextField label="Tumor burden (0-1)" value={tumorBurden} onChange={(e) => setTumorBurden(e.target.value)} />
          <TextField label="PD-L1 (0-1)" value={pdL1} onChange={(e) => setPdL1(e.target.value)} />
          <TextField label="TMB (0-1)" value={tmb} onChange={(e) => setTmb(e.target.value)} />
          <TextField label="Immune infiltration (0-1)" value={immuneInfiltration} onChange={(e) => setImmuneInfiltration(e.target.value)} />
          <TextField label="Prior treatment (0-1)" value={priorTreatmentBurden} onChange={(e) => setPriorTreatmentBurden(e.target.value)} />
        </Stack>
        <Button variant="contained" onClick={() => void estimateImmunotherapyResponse()}>Estimate Research Score</Button>
        {responseEstimate ? (
          <Alert severity="info" sx={{ mt: 2 }}>
            Experimental response score: <strong>{responseEstimate.researchResponseScore.toFixed(4)}</strong> — {responseEstimate.disclaimer}
          </Alert>
        ) : null}

        <Typography variant="h6" sx={{ mt: 3, mb: 1 }}>3. Simulated follow-up CT visualization</Typography>
        <Stack direction={{ xs: "column", md: "row" }} spacing={1} sx={{ alignItems: "center" }}>
          <Button variant="outlined" component="label">
            Select CT PNG/JPEG
            <input hidden type="file" accept="image/png,image/jpeg" onChange={(e) => setCtImage(e.target.files?.[0] ?? null)} />
          </Button>
          <Typography>{ctImage?.name ?? "No image selected"}</Typography>
          <TextField label="Lesion X" value={lesionX} onChange={(e) => setLesionX(e.target.value)} />
          <TextField label="Lesion Y" value={lesionY} onChange={(e) => setLesionY(e.target.value)} />
          <TextField label="Lesion radius px" value={lesionRadius} onChange={(e) => setLesionRadius(e.target.value)} />
          <Button variant="contained" onClick={() => void simulateFollowUpCt()}>Simulate Follow-up</Button>
        </Stack>
        {simulatedImageUrl ? (
          <Box sx={{ mt: 2 }}>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>Research-only simulated follow-up visualization</Typography>
            <Box component="img" src={simulatedImageUrl} alt="Simulated follow-up CT research visualization" sx={{ maxWidth: 512, width: "100%", borderRadius: 1 }} />
          </Box>
        ) : null}
      </Box>

      <RecordDialog
        open={datasetDialog !== null}
        title={datasetDialog === "edit" ? "Edit Clinical Dataset" : "Register Clinical Dataset"}
        fields={datasetFields}
        initialValue={datasetDialog === "edit" && selectedDataset ? selectedDataset : (service.crud?.createTemplate ?? {})}
        onClose={() => setDatasetDialog(null)}
        onSubmit={saveDataset}
      />

      <Dialog open={importOpen} onClose={importing ? undefined : () => setImportOpen(false)} fullWidth maxWidth="md">
        <DialogTitle>Import Research Subjects</DialogTitle>
        <DialogContent dividers>
          <Alert severity="warning" sx={{ mb: 2 }}>
            Import only public, appropriately de-identified, or institutionally authorized research data. Preserve the original dataset identifier and source URI for provenance.
          </Alert>
          <TextField
            label="Import JSON"
            value={importJson}
            onChange={(event) => setImportJson(event.target.value)}
            multiline
            minRows={18}
            fullWidth
            sx={{ fontFamily: "monospace" }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setImportOpen(false)} disabled={importing}>Cancel</Button>
          <Button variant="contained" onClick={() => void importSubjects()} disabled={importing}>
            {importing ? "Importing..." : "Import"}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={Boolean(message)} autoHideDuration={4000} onClose={() => setMessage("")}>
        <Alert severity="success" onClose={() => setMessage("")}>{message}</Alert>
      </Snackbar>
    </>
  );
}
