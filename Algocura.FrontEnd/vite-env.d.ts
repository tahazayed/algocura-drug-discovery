/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_IDENTITY_API_URL?: string;
  readonly VITE_LITERATURE_API_URL?: string;
  readonly VITE_TARGET_DISCOVERY_API_URL?: string;
  readonly VITE_CHEMICAL_DESIGN_API_URL?: string;
  readonly VITE_COMPOUND_CATALOG_API_URL?: string;
  readonly VITE_SCREENING_API_URL?: string;
  readonly VITE_ASSAY_MANAGEMENT_API_URL?: string;
  readonly VITE_AI_DISCOVERY_API_URL?: string;
  readonly VITE_WORKFLOW_API_URL?: string;
  readonly VITE_NOTIFICATION_API_URL?: string;
  readonly VITE_SEARCH_API_URL?: string;
  readonly VITE_REPORTING_API_URL?: string;
  readonly VITE_AUDIT_API_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

interface ImportMetaEnv { readonly VITE_CLINICAL_RESEARCH_API_URL?: string }
interface ImportMeta { readonly env: ImportMetaEnv }
