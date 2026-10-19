# Frontend detailed component and routing inventory

The following are source-derived declarations, route registrations, API call sites, and UI strings. Exact layouts and interactions require original source and visual acceptance tests.

## Algocura.FrontEnd/src/App.tsx

Lines: 5

- L3: `export default function App() {`

## Algocura.FrontEnd/src/api/httpClient.ts

Lines: 73

- L3: `export const tokenStorage = {`
- L43: `const response = await fetch(`${baseUrl}${path}`, {`

## Algocura.FrontEnd/src/api/identityApi.ts

Lines: 23

- L7: `export function login(request: LoginRequest): Promise<TokenResponse> {`
- L18: `export function getCurrentUser(): Promise<Record<string, unknown>> {`

## Algocura.FrontEnd/src/auth/AuthContext.tsx

Lines: 118

- L8: `type PropsWithChildren`
- L19: `interface AuthContextValue {`
- L53: `export function AuthProvider({ children }: PropsWithChildren) {`
- L86: `const handler = () => logout();`
- L111: `export function useAuth(): AuthContextValue {`

## Algocura.FrontEnd/src/auth/RequireAuth.tsx

Lines: 19

- L4: `export function RequireAuth() {`

## Algocura.FrontEnd/src/auth/RequireRole.tsx

Lines: 20

- L5: `interface RequireRoleProps extends PropsWithChildren {`
- L9: `export function RequireRole({`

## Algocura.FrontEnd/src/auth/jwt.test.ts

Lines: 30

- L5: `const encode = (value: object) =>`

## Algocura.FrontEnd/src/auth/jwt.ts

Lines: 58

- L21: `export function parseJwt(token: string): JwtPrincipal {`
- L33: `const roles = (`
- L54: `export function isExpired(principal: JwtPrincipal): boolean {`

## Algocura.FrontEnd/src/components/ErrorState.tsx

Lines: 9

- L3: `export function ErrorState({`

## Algocura.FrontEnd/src/components/JsonTable.test.tsx

Lines: 41


## Algocura.FrontEnd/src/components/JsonTable.tsx

Lines: 130

- L17: `export function formatColumnName(value: string): string {`
- L35: `interface JsonTableProps {`
- L48: `export function JsonTable({`

## Algocura.FrontEnd/src/components/LoadingState.tsx

Lines: 20

- L3: `export function LoadingState() {`

## Algocura.FrontEnd/src/components/PageHeader.tsx

Lines: 28

- L3: `export function PageHeader({`

## Algocura.FrontEnd/src/components/RecordDialog.test.tsx

Lines: 86


## Algocura.FrontEnd/src/components/RecordDialog.tsx

Lines: 249

- L21: `interface RecordDialogProps {`
- L30: `type FormErrors = Record<string, string>;`
- L100: `const selectedOption = (field.options ?? []).find(`
- L118: `export function RecordDialog({ open, title, fields, initialValue, onClose, onSubmit }: RecordDialogProps) {`
- L200: `const selectedOption = (field.options ?? []).find(`

## Algocura.FrontEnd/src/components/ReportingCharts.tsx

Lines: 491

- L13: `interface CountItem {`
- L380: `export function ReportingCharts({`

## Algocura.FrontEnd/src/components/ReportingNavigation.tsx

Lines: 35

- L4: `export function ReportingNavigation() {`

## Algocura.FrontEnd/src/config/services.test.ts

Lines: 29


## Algocura.FrontEnd/src/config/services.ts

Lines: 233

- L6: `const text = (name: string, label: string, required = false, helperText?: string): FormFieldDefinition => ({ name, label, type: "text", required, helperText });`
- L7: `const textarea = (name: string, label: string, required = false, helperText?: string): FormFieldDefinition => ({ name, label, type: "textarea", required, helperText, rows: 4 });`
- L8: `const number = (name: string, label: string, required = false, min?: number, max?: number, step?: number): FormFieldDefinition => ({ name, label, type: "number", required, min, max, step });`
- L9: `const select = (name: string, label: string, options: { label: string; value: string | number }[], required = true): FormFieldDefinition => ({ name, label, type: "select", required, options });`
- L138: `export const services: ServiceDefinition[] = [`
- L233: `export const serviceByKey = Object.fromEntries(services.map((service) => [service.key, service]));`

## Algocura.FrontEnd/src/layout/AppShell.tsx

Lines: 240

- L22: `export function AppShell() {`
- L31: `const closeDrawer = () => setDrawerOpen(false);`

## Algocura.FrontEnd/src/main.tsx

Lines: 25


## Algocura.FrontEnd/src/pages/ClinicalResearchPage.tsx

Lines: 449

- L25: `interface ClinicalDataset extends JsonRecord {`
- L38: `interface TciaSeries extends JsonRecord {`
- L50: `interface ResponseEstimate extends JsonRecord {`
- L58: `interface ResearchSubject extends JsonRecord {`
- L96: `export function ClinicalResearchPage({ service }: { service: ServiceDefinition }) {`
- L239: `const response = await fetch(`${service.baseUrl}/api/v1/imaging-research/tcia/series/${encodeURIComponent(series.seriesInstanceUid)}/download`, { headers });`
- L289: `const response = await fetch(`${service.baseUrl}/api/v1/imaging-research/simulate-followup-ct`, { method: "POST", headers, body: form });`

## Algocura.FrontEnd/src/pages/DashboardPage.tsx

Lines: 64

- L14: `export function DashboardPage() {`

## Algocura.FrontEnd/src/pages/ForbiddenPage.tsx

Lines: 24

- L5: `export function ForbiddenPage() {`

## Algocura.FrontEnd/src/pages/GenericServicePage.tsx

Lines: 170

- L58: `export function GenericServicePage({ service }: { service: ServiceDefinition }) {`

## Algocura.FrontEnd/src/pages/LoginPage.tsx

Lines: 162

- L1: `import { useState, type FormEvent } from "react";`
- L15: `export function LoginPage() {`

## Algocura.FrontEnd/src/pages/NotFoundPage.tsx

Lines: 17

- L5: `export function NotFoundPage() {`

## Algocura.FrontEnd/src/pages/ReportingAnalyticsPage.tsx

Lines: 491

- L17: `interface ReportingSummary {`
- L24: `interface ReportMetric {`
- L31: `interface ReportRun {`
- L43: `interface CountItem {`
- L127: `const length = (item.count / total) * circumference;`
- L356: `export function ReportingAnalyticsPage() {`

## Algocura.FrontEnd/src/pages/SearchPage.tsx

Lines: 107

- L1: `import { useState, type FormEvent } from "react";`
- L39: `export function SearchPage() {`

## Algocura.FrontEnd/src/routes/AppRoutes.tsx

Lines: 108

- L15: `export function AppRoutes() {`
- L18: `<Route path="/login" element={<LoginPage />} />`
- L20: `<Route element={<RequireAuth />}>`
- L21: `<Route element={<AppShell />}>`
- L22: `<Route index element={<DashboardPage />} />`
- L23: `<Route`
- L24: `path="forbidden"`
- L36: `<Route`
- L38: `path={service.route.slice(1)}`
- L51: `<Route`
- L52: `path="clinical-research"`
- L60: `<Route`
- L61: `path="reporting"`
- L77: `<Route`
- L78: `path="reporting/analytics"`
- L90: `<Route`
- L91: `path="search"`
- L103: `<Route path="*" element={<NotFoundPage />} />`

## Algocura.FrontEnd/src/test/setup.ts

Lines: 7


## Algocura.FrontEnd/src/theme.ts

Lines: 48

- L3: `export const theme = createTheme({`

## Algocura.FrontEnd/src/types/auth.ts

Lines: 23

- L1: `export type PlatformRole =`
- L7: `export interface LoginRequest {`
- L12: `export interface TokenResponse {`
- L17: `export interface JwtPrincipal {`

## Algocura.FrontEnd/src/types/common.ts

Lines: 54

- L1: `export type JsonRecord = Record<string, unknown>;`
- L3: `export interface FormOption {`
- L8: `export type FormFieldType =`
- L17: `export interface FormFieldDefinition {`
- L31: `export interface CrudCapabilities {`
- L43: `export interface ServiceDefinition {`

## Algocura.FrontEnd/src/vite-env.d.ts

Lines: 21

- L3: `interface ImportMetaEnv {`
- L19: `interface ImportMeta {`
