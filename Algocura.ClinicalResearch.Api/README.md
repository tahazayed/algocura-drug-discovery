# Algocura Clinical Research API

Prototype service for registering public/de-identified biomedical research datasets and importing patient/research-subject records with provenance.

## Endpoints

- `GET/POST /api/v1/clinical-datasets`
- `GET/PUT/DELETE /api/v1/clinical-datasets/{id}`
- `GET /api/v1/research-subjects?datasetId={id}`
- `POST /api/v1/research-subjects/datasets/{datasetId}/import`

## Integration events

- `ClinicalDatasetRegistered`
- `ClinicalDatasetUpdated`
- `ClinicalDatasetStatusChanged`
- `ClinicalDatasetDeleted`
- `ResearchSubjectsImported`

The import endpoint upserts by `(ClinicalDatasetId, ExternalSubjectId)` and automatically moves a Draft dataset to Ready after a non-empty import.

For this delivery, database startup uses `EnsureCreatedAsync` so the prototype can run without a generated migration. Before production deployment, create an EF Core migration and switch the initializer to `MigrateAsync` to match the other Algocura services.

## Imaging + immunotherapy research prototype

Research-only endpoints added for the Badry demonstration workflow:

- `GET /api/v1/imaging-research/tcia/series?collection=...&patientId=...` — query public TCIA/NBIA series metadata.
- `POST /api/v1/imaging-research/tcia/curate` — retrieve and filter series by modality/body part/minimum image count.
- `GET /api/v1/imaging-research/tcia/series/{seriesInstanceUid}/download` — download the public DICOM series ZIP from TCIA.
- `POST /api/v1/imaging-research/immunotherapy-response/estimate` — deterministic logistic research-score prototype using normalized hypothetical features. Coefficients are illustrative and are not clinically fitted.
- `POST /api/v1/imaging-research/simulate-followup-ct` — accepts a rendered CT PNG/JPEG plus a manually identified lesion ROI and creates a synthetic follow-up visualization by shrinking the ROI according to the research response score.

The score and synthetic image are explicitly non-clinical. They are suitable for software/architecture demonstration only and must not be represented as validated patient predictions.

## Retrospective Public-Health Research (Wael Demonstration)

The Clinical Research API includes a research-only workflow for reproducing the data-integration pattern described in the Wael expert evaluation using registered public datasets:

- `WHO-COVID19-DAILY` — WHO country-level daily COVID-19 cases/deaths.
- `GOOGLE-COVID19-EPI` — Google regional epidemiology keyed by date + geographic key.
- `GOOGLE-COVID19-GEO` — Google geographic index used to resolve regional keys and aggregation levels.

Endpoints:

```text
GET  /api/v1/public-health-research/wael/datasets/validate
POST /api/v1/public-health-research/covid19/analyze
```

The analysis streams source CSV data, filters it to the requested country/date range, fuses Google regional observations with WHO country-level observations by date, calculates transparent retrospective trend/risk signals, and runs a deterministic-seed Monte Carlo resource-demand simulation with user-visible coefficients.

The resource model is illustrative and not scientifically/clinically validated. It must not be used for real healthcare-resource allocation or patient/public-health decisions.
