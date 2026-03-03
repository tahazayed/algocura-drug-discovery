import { Route, Routes } from "react-router";
import { RequireAuth } from "../auth/RequireAuth";
import { RequireRole } from "../auth/RequireRole";
import { serviceByKey, services } from "../config/services";
import { AppShell } from "../layout/AppShell";
import { DashboardPage } from "../pages/DashboardPage";
import { ClinicalResearchPage } from "../pages/ClinicalResearchPage";
import { ForbiddenPage } from "../pages/ForbiddenPage";
import { GenericServicePage } from "../pages/GenericServicePage";
import { LoginPage } from "../pages/LoginPage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { SearchPage } from "../pages/SearchPage";
import { ReportingAnalyticsPage } from "../pages/ReportingAnalyticsPage";

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<RequireAuth />}>
        <Route element={<AppShell />}>
          <Route index element={<DashboardPage />} />
          <Route
            path="forbidden"
            element={<ForbiddenPage />}
          />

          {services
            .filter(
              (service) =>
                service.key !== "search" &&
                service.key !== "reporting" &&
                service.key !== "clinical-research"
            )
            .map((service) => (
              <Route
                key={service.key}
                path={service.route.slice(1)}
                element={
                  <RequireRole
                    roles={service.allowedRoles}
                  >
                    <GenericServicePage
                      service={service}
                    />
                  </RequireRole>
                }
              />
            ))}

          <Route
            path="clinical-research"
            element={
              <RequireRole roles={serviceByKey["clinical-research"].allowedRoles}>
                <ClinicalResearchPage service={serviceByKey["clinical-research"]} />
              </RequireRole>
            }
          />

          <Route
            path="reporting"
            element={
              <RequireRole
                roles={
                  serviceByKey.reporting.allowedRoles
                }
              >
                <GenericServicePage
                  service={
                    serviceByKey.reporting
                  }
                />
              </RequireRole>
            }
          />

          <Route
            path="reporting/analytics"
            element={
              <RequireRole
                roles={
                  serviceByKey.reporting.allowedRoles
                }
              >
                <ReportingAnalyticsPage />
              </RequireRole>
            }
          />

          <Route
            path="search"
            element={
              <RequireRole
                roles={
                  serviceByKey.search.allowedRoles
                }
              >
                <SearchPage />
              </RequireRole>
            }
          />

          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Route>
    </Routes>
  );
}
