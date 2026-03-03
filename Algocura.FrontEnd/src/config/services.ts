import type { FormFieldDefinition, ServiceDefinition } from "../types/common";

const env = import.meta.env;
const scientificRoles = ["Scientist", "Reviewer", "Administrator"];

const text = (name: string, label: string, required = false, helperText?: string): FormFieldDefinition => ({ name, label, type: "text", required, helperText });
const textarea = (name: string, label: string, required = false, helperText?: string): FormFieldDefinition => ({ name, label, type: "textarea", required, helperText, rows: 4 });
const number = (name: string, label: string, required = false, min?: number, max?: number, step?: number): FormFieldDefinition => ({ name, label, type: "number", required, min, max, step });
const select = (name: string, label: string, options: { label: string; value: string | number }[], required = true): FormFieldDefinition => ({ name, label, type: "select", required, options });

const publicationFields: FormFieldDefinition[] = [
  text("title", "Title", true),
  text("journal", "Journal"),
  textarea("abstract", "Abstract", true),
  text("externalId", "External ID"),
  text("sourceUrl", "Source URL"),
  { name: "publishedDate", label: "Published Date", type: "date" },
  { name: "authors", label: "Authors", type: "string-list", helperText: "Enter one author per line or separate authors with commas." }
];

const targetCreateFields: FormFieldDefinition[] = [
  text("targetHypothesisId", "Target Hypothesis ID", true),
  text("name", "Name", true),
  textarea("scientificRationale", "Scientific Rationale"),
  number("geneticEvidenceScore", "Genetic Evidence Score", true, 0, 100, 0.01),
  number("biologicalPlausibilityScore", "Biological Plausibility Score", true, 0, 100, 0.01),
  number("druggabilityScore", "Druggability Score", true, 0, 100, 0.01),
  number("safetyRiskScore", "Safety Risk Score", true, 0, 100, 0.01),
  number("translationalPotentialScore", "Translational Potential Score", true, 0, 100, 0.01),
  textarea("reviewNotes", "Review Notes")
];

const targetEditFields = targetCreateFields.filter((field) => !["targetHypothesisId", "name"].includes(field.name));

const compoundFields: FormFieldDefinition[] = [
  text("name", "Name", true),
  text("canonicalSmiles", "Canonical SMILES", true),
  text("inchiKey", "InChI Key", false, "Optional. When provided, the API requires 27 characters."),
  number("molecularWeight", "Molecular Weight", true, 0.001, 100000, 0.001),
  text("molecularFormula", "Molecular Formula"),
  text("source", "Source"),
  textarea("description", "Description"),
  select("status", "Status", [
    { label: "Draft", value: 1 },
    { label: "Registered", value: 2 },
    { label: "Under Evaluation", value: 3 },
    { label: "Selected", value: 4 },
    { label: "Rejected", value: 5 },
    { label: "Archived", value: 6 }
  ])
];

const screeningCreateFields: FormFieldDefinition[] = [
  text("name", "Name", true),
  text("screeningMethod", "Screening Method", true),
  textarea("description", "Description"),
  { name: "compoundIds", label: "Compound IDs", type: "string-list", required: true, helperText: "Enter one compound GUID per line or separate IDs with commas." }
];
const screeningEditFields = screeningCreateFields.filter((field) => field.name !== "compoundIds");

const assayFields: FormFieldDefinition[] = [
  text("screeningId", "Screening ID", true),
  text("name", "Name", true),
  select("assayType", "Assay Type", [
    { label: "Biochemical", value: 1 },
    { label: "Cell Based", value: 2 },
    { label: "Binding", value: 3 },
    { label: "Functional", value: 4 },
    { label: "Toxicity", value: 5 },
    { label: "Selectivity", value: 6 },
    { label: "ADMET", value: 7 }
  ]),
  text("biologicalTarget", "Biological Target"),
  textarea("protocol", "Protocol"),
  text("measurementUnit", "Measurement Unit", true),
  number("activityThreshold", "Activity Threshold", true, undefined, undefined, 0.01)
];

const predictionFields: FormFieldDefinition[] = [
  text("name", "Name", true),
  text("modelDefinitionId", "Model Definition ID", true),
  text("sourceScreeningId", "Source Screening ID"),
  textarea("description", "Description"),
  { name: "compoundIds", label: "Compound IDs", type: "string-list", helperText: "Optional. Enter one compound GUID per line or separate IDs with commas." }
];

const workflowFields: FormFieldDefinition[] = [
  text("name", "Name", true),
  text("diseaseName", "Disease Name", true),
  text("geneSymbol", "Gene Symbol"),
  textarea("objective", "Objective")
];

const notificationFields: FormFieldDefinition[] = [
  text("sourceEventType", "Source Event Type", true),
  text("sourceService", "Source Service", true),
  text("subject", "Subject", true),
  textarea("body", "Body", true),
  select("priority", "Priority", [
    { label: "Low", value: 1 }, { label: "Normal", value: 2 }, { label: "High", value: 3 }, { label: "Critical", value: 4 }
  ]),
  select("channel", "Channel", [
    { label: "Email", value: 1 }, { label: "Microsoft Teams", value: 2 }, { label: "Webhook", value: 3 }, { label: "In Application", value: 4 }
  ]),
  text("recipient", "Recipient", true),
  text("correlationId", "Correlation ID")
];

const clinicalDatasetFields: FormFieldDefinition[] = [
  text("name", "Dataset Name", true),
  text("sourceName", "Source Name", true, "For example: NCI GDC, cBioPortal, institutional research repository."),
  text("sourceUri", "Source URI", false, "URL or persistent identifier used to preserve provenance."),
  text("studyCode", "Study Code"),
  text("diseaseArea", "Disease Area"),
  textarea("description", "Description"),
  select("status", "Status", [
    { label: "Draft", value: 1 },
    { label: "Ready", value: 2 },
    { label: "Archived", value: 3 }
  ])
];

const reportingCreateFields: FormFieldDefinition[] = [
  text("name", "Name", true),
  textarea("description", "Description"),
  text("reportType", "Report Type", true),
  textarea("defaultParametersJson", "Default Parameters", false, "Optional report parameters. The Reporting API stores this value as JSON text.")
];
const reportingEditFields: FormFieldDefinition[] = [
  ...reportingCreateFields,
  select("status", "Status", [
    { label: "Draft", value: 1 },
    { label: "Active", value: 2 },
    { label: "Retired", value: 3 }
  ])
];

export const services: ServiceDefinition[] = [
  {
    key: "literature", name: "Literature Intelligence", shortName: "Publication",
    description: "Publications, evidence and target hypotheses.", baseUrl: env.VITE_LITERATURE_API_URL ?? "http://localhost:7001",
    route: "/literature", primaryEndpoint: "/api/v1/publications", allowedRoles: scientificRoles, category: "Discovery",
    crud: { create: true, delete: true, createRoles: scientificRoles, deleteRoles: scientificRoles,
      createTemplate: { title: "", abstract: "", journal: "", externalId: "", sourceUrl: "", publishedDate: null, authors: [] }, createFields: publicationFields }
  },
  {
    key: "target-discovery", name: "Target Discovery", shortName: "Target Assessment",
    description: "Target assessments and selection.", baseUrl: env.VITE_TARGET_DISCOVERY_API_URL ?? "http://localhost:7002",
    route: "/target-discovery", primaryEndpoint: "/api/v1/target-assessments", allowedRoles: scientificRoles, category: "Discovery",
    crud: { create: true, edit: true, delete: true, createRoles: scientificRoles, editRoles: scientificRoles, deleteRoles: scientificRoles,
      createTemplate: { targetHypothesisId: "", name: "", scientificRationale: "", geneticEvidenceScore: 0, biologicalPlausibilityScore: 0, druggabilityScore: 0, safetyRiskScore: 0, translationalPotentialScore: 0, reviewNotes: "" },
      createFields: targetCreateFields, editFields: targetEditFields }
  },
  {
    key: "chemical-design", name: "Chemical Design", shortName: "Design", description: "Design projects, structure searches and candidates.",
    baseUrl: env.VITE_CHEMICAL_DESIGN_API_URL ?? "http://localhost:7003", route: "/chemical-design", primaryEndpoint: "/api/v1/selected-targets",
    allowedRoles: scientificRoles, category: "Discovery"
  },
  {
    key: "compound-catalog", name: "Compound Catalog", shortName: "Compound", description: "Chemical compound registration and metadata.",
    baseUrl: env.VITE_COMPOUND_CATALOG_API_URL ?? "http://localhost:7004", route: "/compounds", primaryEndpoint: "/api/v1/compounds",
    allowedRoles: scientificRoles, category: "Discovery",
    crud: { create: true, edit: true, delete: true, createRoles: scientificRoles, editRoles: scientificRoles, deleteRoles: scientificRoles,
      createTemplate: { name: "", canonicalSmiles: "", inchiKey: "", molecularWeight: 0.001, molecularFormula: "", source: "", description: "", status: 1 },
      createFields: compoundFields, editFields: compoundFields }
  },
  {
    key: "screening", name: "Screening", shortName: "Screening", description: "Screening jobs and lifecycle management.",
    baseUrl: env.VITE_SCREENING_API_URL ?? "http://localhost:7005", route: "/screening", primaryEndpoint: "/api/v1/screenings",
    allowedRoles: scientificRoles, category: "Discovery",
    crud: { create: true, edit: true, delete: true, createRoles: scientificRoles, editRoles: scientificRoles, deleteRoles: scientificRoles,
      createTemplate: { name: "", screeningMethod: "", description: "", compoundIds: [] }, createFields: screeningCreateFields, editFields: screeningEditFields }
  },
  {
    key: "assays", name: "Assay Management", shortName: "Assay", description: "Assays, measurements and completed screenings.",
    baseUrl: env.VITE_ASSAY_MANAGEMENT_API_URL ?? "http://localhost:7006", route: "/assays", primaryEndpoint: "/api/v1/assays",
    allowedRoles: scientificRoles, category: "Discovery",
    crud: { create: true, createRoles: scientificRoles, createTemplate: { screeningId: "", name: "", assayType: 1, biologicalTarget: "", protocol: "", measurementUnit: "", activityThreshold: 0 }, createFields: assayFields }
  },
  {
    key: "ai-discovery", name: "AI Discovery", shortName: "Prediction", description: "Prediction models, adaptive routing and prioritization.",
    baseUrl: env.VITE_AI_DISCOVERY_API_URL ?? "http://localhost:7007", route: "/ai-discovery", primaryEndpoint: "/api/v1/predictions",
    allowedRoles: scientificRoles, category: "Discovery",
    crud: { create: true, delete: true, createRoles: scientificRoles, deleteRoles: scientificRoles,
      createTemplate: { name: "", modelDefinitionId: "", sourceScreeningId: "", description: "", compoundIds: [] }, createFields: predictionFields }
  },
  {
    key: "workflow", name: "Workflow", shortName: "Workflow", description: "Cross-service drug-discovery workflow orchestration.",
    baseUrl: env.VITE_WORKFLOW_API_URL ?? "http://localhost:7008", route: "/workflow", primaryEndpoint: "/api/v1/workflows",
    allowedRoles: scientificRoles, category: "Operations",
    crud: { create: true, createRoles: scientificRoles, createTemplate: { name: "", diseaseName: "", geneSymbol: "", objective: "" }, createFields: workflowFields }
  },
  {
    key: "notifications", name: "Notification", shortName: "Notification", description: "Notification delivery and lifecycle.",
    baseUrl: env.VITE_NOTIFICATION_API_URL ?? "http://localhost:7009", route: "/notifications", primaryEndpoint: "/api/v1/notifications",
    allowedRoles: scientificRoles, category: "Operations",
    crud: { create: true, createRoles: scientificRoles,
      createTemplate: { sourceEventType: "", sourceService: "", subject: "", body: "", priority: 2, channel: 1, recipient: "", correlationId: "" }, createFields: notificationFields }
  },
  {
    key: "clinical-research", name: "Clinical Research", shortName: "Clinical Dataset",
    description: "Public/de-identified biomedical datasets, patient-level research records, and provenance.",
    baseUrl: env.VITE_CLINICAL_RESEARCH_API_URL ?? "http://localhost:7015", route: "/clinical-research", primaryEndpoint: "/api/v1/clinical-datasets",
    allowedRoles: scientificRoles, category: "Discovery",
    crud: { create: true, edit: true, delete: true, createRoles: scientificRoles, editRoles: scientificRoles, deleteRoles: scientificRoles,
      createTemplate: { name: "", sourceName: "", sourceUri: "", studyCode: "", diseaseArea: "", description: "", status: 1 },
      createFields: clinicalDatasetFields, editFields: clinicalDatasetFields }
  },
  {
    key: "search", name: "Search", shortName: "Search", description: "Cross-domain search across discovery projections.",
    baseUrl: env.VITE_SEARCH_API_URL ?? "http://localhost:7010", route: "/search", primaryEndpoint: "/api/v1/search",
    allowedRoles: scientificRoles, category: "Platform"
  },
  {
    key: "reporting", name: "Reporting", shortName: "Report Definition", description: "Report definitions and report execution.",
    baseUrl: env.VITE_REPORTING_API_URL ?? "http://localhost:7011", route: "/reporting", primaryEndpoint: "/api/v1/report-definitions",
    allowedRoles: scientificRoles, category: "Platform",
    crud: { create: true, edit: true, delete: true, createRoles: scientificRoles, editRoles: scientificRoles, deleteRoles: scientificRoles,
      createTemplate: { name: "", description: "", reportType: "", defaultParametersJson: "" }, createFields: reportingCreateFields, editFields: reportingEditFields }
  },
  {
    key: "audit", name: "Audit", shortName: "Audit", description: "Central audit history and summaries.",
    baseUrl: env.VITE_AUDIT_API_URL ?? "http://localhost:7012", route: "/audit", primaryEndpoint: "/api/v1/audit",
    allowedRoles: ["Reviewer", "Administrator"], category: "Platform"
  },
  {
    key: "identity", name: "Identity", shortName: "Identity", description: "Users and role administration.",
    baseUrl: env.VITE_IDENTITY_API_URL ?? "http://localhost:7013", route: "/administration/users", primaryEndpoint: "/api/v1/users",
    allowedRoles: ["Administrator"], category: "Platform"
  }
];

export const serviceByKey = Object.fromEntries(services.map((service) => [service.key, service]));
