export type JsonRecord = Record<string, unknown>;

export interface FormOption {
  label: string;
  value: string | number;
}

export type FormFieldType =
  | "text"
  | "textarea"
  | "number"
  | "date"
  | "checkbox"
  | "select"
  | "string-list";

export interface FormFieldDefinition {
  name: string;
  label: string;
  type?: FormFieldType;
  required?: boolean;
  helperText?: string;
  placeholder?: string;
  min?: number;
  max?: number;
  step?: number;
  rows?: number;
  options?: FormOption[];
}

export interface CrudCapabilities {
  create?: boolean;
  edit?: boolean;
  delete?: boolean;
  createRoles?: string[];
  editRoles?: string[];
  deleteRoles?: string[];
  createTemplate?: JsonRecord;
  createFields?: FormFieldDefinition[];
  editFields?: FormFieldDefinition[];
}

export interface ServiceDefinition {
  key: string;
  name: string;
  shortName: string;
  description: string;
  baseUrl: string;
  route: string;
  primaryEndpoint: string;
  allowedRoles: string[];
  category: "Discovery" | "Platform" | "Operations";
  crud?: CrudCapabilities;
}
