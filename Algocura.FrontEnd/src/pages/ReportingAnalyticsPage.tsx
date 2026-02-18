import { useCallback, useEffect, useMemo, useState } from "react";
import {
  Box,
  Card,
  CardContent,
  Grid,
  Stack,
  Typography
} from "@mui/material";
import { apiRequest } from "../api/httpClient";
import { ErrorState } from "../components/ErrorState";
import { LoadingState } from "../components/LoadingState";
import { PageHeader } from "../components/PageHeader";
import { ReportingNavigation } from "../components/ReportingNavigation";
import { serviceByKey } from "../config/services";

interface ReportingSummary {
  totalProjections: number;
  projectionCounts: Record<string, number>;
  activeReportDefinitions: number;
  completedReportRuns: number;
}

interface ReportMetric {
  name: string;
  value: number;
  valueType: number;
  unit?: string | null;
}

interface ReportRun {
  id: string;
  reportDefinitionId: string;
  name: string;
  status: number;
  failureReason?: string | null;
  resultLocation?: string | null;
  startedAtUtc?: string | null;
  completedAtUtc?: string | null;
  metrics: ReportMetric[];
}

interface CountItem {
  label: string;
  count: number;
}

const reporting = serviceByKey.reporting;

const runStatusLabels: Record<number, string> = {
  1: "Queued",
  2: "Running",
  3: "Completed",
  4: "Failed",
  5: "Cancelled"
};

const chartColors = [
  "#14532d",
  "#0f766e",
  "#2563eb",
  "#7c3aed",
  "#d97706",
  "#be123c"
];

function countRuns(runs: ReportRun[]): CountItem[] {
  const counts = new Map<string, number>();

  for (const run of runs) {
    const label = runStatusLabels[run.status] ?? `Status ${run.status}`;
    counts.set(label, (counts.get(label) ?? 0) + 1);
  }

  return Array.from(counts.entries()).map(([label, count]) => ({
    label,
    count
  }));
}

function DonutChart({ items }: { items: CountItem[] }) {
  const total = items.reduce((sum, item) => sum + item.count, 0);
  const radius = 72;
  const circumference = 2 * Math.PI * radius;
  let offset = 0;

  if (total === 0) {
    return (
      <Box
        sx={{
          py: 5,
          textAlign: "center"
        }}>
        <Typography sx={{
          color: "text.secondary"
        }}>No report runs available.</Typography>
      </Box>
    );
  }

  return (
    <Stack
      direction={{ xs: "column", sm: "row" }}
      spacing={3}
      sx={{
        alignItems: "center",
        justifyContent: "center",
        minHeight: 260
      }}>
      <Box sx={{ width: 190, height: 190, flexShrink: 0 }}>
        <svg
          viewBox="0 0 190 190"
          width="190"
          height="190"
          role="img"
          aria-label="Report run status distribution"
        >
          <circle
            cx="95"
            cy="95"
            r={radius}
            fill="none"
            stroke="#e5e7eb"
            strokeWidth="26"
          />
          {items.map((item, index) => {
            const length = (item.count / total) * circumference;
            const currentOffset = offset;
            offset += length;

            return (
              <circle
                key={item.label}
                cx="95"
                cy="95"
                r={radius}
                fill="none"
                stroke={chartColors[index % chartColors.length]}
                strokeWidth="26"
                strokeDasharray={`${length} ${circumference - length}`}
                strokeDashoffset={-currentOffset}
                strokeLinecap="butt"
                transform="rotate(-90 95 95)"
              />
            );
          })}
          <text
            x="95"
            y="88"
            textAnchor="middle"
            fontSize="30"
            fontWeight="700"
            fill="currentColor"
          >
            {total}
          </text>
          <text
            x="95"
            y="112"
            textAnchor="middle"
            fontSize="13"
            fill="#6b7280"
          >
            runs
          </text>
        </svg>
      </Box>

      <Stack spacing={1.25} sx={{
        minWidth: 170
      }}>
        {items.map((item, index) => (
          <Stack
            key={item.label}
            direction="row"
            spacing={1}
            sx={{
              alignItems: "center"
            }}
          >
            <Box
              sx={{
                width: 12,
                height: 12,
                borderRadius: "50%",
                bgcolor: chartColors[index % chartColors.length],
                flexShrink: 0
              }}
            />
            <Typography variant="body2" sx={{ flexGrow: 1 }}>
              {item.label}
            </Typography>
            <Typography variant="body2" sx={{
              fontWeight: 700
            }}>
              {item.count}
            </Typography>
          </Stack>
        ))}
      </Stack>
    </Stack>
  );
}

function HorizontalBars({ items }: { items: CountItem[] }) {
  const maxCount = Math.max(...items.map((item) => item.count), 1);

  if (items.length === 0) {
    return (
      <Box
        sx={{
          py: 5,
          textAlign: "center"
        }}>
        <Typography sx={{
          color: "text.secondary"
        }}>No projection data available.</Typography>
      </Box>
    );
  }

  return (
    <Stack spacing={1.75} sx={{
      mt: 2
    }}>
      {items.map((item, index) => (
        <Box key={item.label}>
          <Stack
            direction="row"
            sx={{
              justifyContent: "space-between",
              mb: 0.5
            }}>
            <Typography variant="body2">{item.label}</Typography>
            <Typography variant="body2" sx={{
              fontWeight: 700
            }}>
              {item.count}
            </Typography>
          </Stack>
          <Box
            sx={{
              height: 18,
              borderRadius: 1,
              bgcolor: "action.hover",
              overflow: "hidden"
            }}
          >
            <Box
              sx={{
                height: "100%",
                width: `${Math.max(4, (item.count / maxCount) * 100)}%`,
                bgcolor: chartColors[index % chartColors.length],
                borderRadius: 1
              }}
            />
          </Box>
        </Box>
      ))}
    </Stack>
  );
}

function MetricBars({ metrics }: { metrics: ReportMetric[] }) {
  const normalized = metrics
    .filter((metric) => Number.isFinite(Number(metric.value)))
    .slice(0, 8);

  if (normalized.length === 0) {
    return (
      <Box
        sx={{
          py: 4,
          textAlign: "center"
        }}>
        <Typography sx={{
          color: "text.secondary"
        }}>No metrics available.</Typography>
      </Box>
    );
  }

  const maxValue = Math.max(
    ...normalized.map((metric) => Math.abs(Number(metric.value))),
    1
  );

  return (
    <Stack spacing={1.75} sx={{
      mt: 2
    }}>
      {normalized.map((metric, index) => (
        <Box key={`${metric.name}-${index}`}>
          <Stack
            direction="row"
            sx={{
              justifyContent: "space-between",
              mb: 0.5
            }}>
            <Typography variant="body2">{metric.name}</Typography>
            <Typography variant="body2" sx={{
              fontWeight: 700
            }}>
              {Number(metric.value).toLocaleString()}
              {metric.unit ? ` ${metric.unit}` : ""}
            </Typography>
          </Stack>
          <Box
            sx={{
              height: 18,
              borderRadius: 1,
              bgcolor: "action.hover",
              overflow: "hidden"
            }}
          >
            <Box
              sx={{
                height: "100%",
                width: `${Math.max(
                  4,
                  (Math.abs(Number(metric.value)) / maxValue) * 100
                )}%`,
                bgcolor: chartColors[index % chartColors.length],
                borderRadius: 1
              }}
            />
          </Box>
        </Box>
      ))}
    </Stack>
  );
}

function KpiCard({ label, value }: { label: string; value: number }) {
  return (
    <Card sx={{ height: "100%" }}>
      <CardContent>
        <Typography variant="body2" sx={{
          color: "text.secondary"
        }}>
          {label}
        </Typography>
        <Typography
          variant="h3"
          sx={{
            mt: 1,
            fontWeight: 700
          }}>
          {value.toLocaleString()}
        </Typography>
      </CardContent>
    </Card>
  );
}

export function ReportingAnalyticsPage() {
  const [summary, setSummary] = useState<ReportingSummary | null>(null);
  const [runs, setRuns] = useState<ReportRun[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      const [summaryResult, runResult] = await Promise.all([
        apiRequest<ReportingSummary>(
          reporting.baseUrl,
          "/api/v1/reporting/summary"
        ),
        apiRequest<ReportRun[]>(
          reporting.baseUrl,
          "/api/v1/report-runs"
        )
      ]);

      setSummary(summaryResult);
      setRuns(runResult);
    } catch (exception) {
      setError(
        exception instanceof Error
          ? exception.message
          : "Failed to load reporting analytics."
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const projectionItems = useMemo<CountItem[]>(
    () =>
      Object.entries(summary?.projectionCounts ?? {})
        .map(([label, count]) => ({ label, count }))
        .sort((left, right) => right.count - left.count),
    [summary]
  );

  const runStatusItems = useMemo(() => countRuns(runs), [runs]);

  const latestMetrics = useMemo(
    () => runs.find((run) => run.metrics?.length)?.metrics ?? [],
    [runs]
  );

  return (
    <>
      <PageHeader
        title="Reporting Analytics"
        description="Visual summaries of reporting projections, report runs, and report metrics."
      />

      <ReportingNavigation />

      {loading ? <LoadingState /> : null}
      {error ? <ErrorState message={error} /> : null}

      {!loading && !error && summary ? (
        <>
          <Grid container spacing={2} sx={{
            mb: 3
          }}>
            <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
              <KpiCard label="Total Projections" value={summary.totalProjections} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
              <KpiCard
                label="Active Report Definitions"
                value={summary.activeReportDefinitions}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
              <KpiCard label="Completed Report Runs" value={summary.completedReportRuns} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
              <KpiCard label="Total Report Runs" value={runs.length} />
            </Grid>
          </Grid>

          <Grid container spacing={2} sx={{
            mb: 3
          }}>
            <Grid size={{ xs: 12, lg: 7 }}>
              <Card sx={{ height: "100%" }}>
                <CardContent>
                  <Typography variant="h6">Reporting Projections</Typography>
                  <Typography variant="body2" sx={{
                    color: "text.secondary"
                  }}>
                    Projection records currently available to the Reporting API.
                  </Typography>
                  <HorizontalBars items={projectionItems} />
                </CardContent>
              </Card>
            </Grid>

            <Grid size={{ xs: 12, lg: 5 }}>
              <Card sx={{ height: "100%" }}>
                <CardContent>
                  <Typography variant="h6">Report Run Status</Typography>
                  <Typography variant="body2" sx={{
                    color: "text.secondary"
                  }}>
                    Distribution of report runs by execution status.
                  </Typography>
                  <DonutChart items={runStatusItems} />
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          <Card>
            <CardContent>
              <Typography variant="h6">Latest Report Metrics</Typography>
              <Typography variant="body2" sx={{
                color: "text.secondary"
              }}>
                Metrics from the most recent report run that contains metric values.
              </Typography>
              <MetricBars metrics={latestMetrics} />
            </CardContent>
          </Card>
        </>
      ) : null}
    </>
  );
}
