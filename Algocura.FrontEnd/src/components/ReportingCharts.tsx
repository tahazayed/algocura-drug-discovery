import {
  Box,
  Card,
  CardContent,
  Grid,
  Stack,
  Typography
} from "@mui/material";
import type {
  JsonRecord
} from "../types/common";

interface CountItem {
  label: string;
  count: number;
}

const reportStatusNames:
  Record<string, string> = {
    "1": "Draft",
    "2": "Active",
    "3": "Retired"
  };

const chartColors = [
  "#14532d",
  "#0f766e",
  "#2563eb",
  "#7c3aed",
  "#d97706",
  "#be123c"
];

function normalizeText(
  value: unknown,
  fallback: string
): string {
  if (
    value === null ||
    value === undefined
  ) {
    return fallback;
  }

  const text =
    String(value).trim();

  return text || fallback;
}

function statusLabel(
  value: unknown
): string {
  const raw =
    normalizeText(
      value,
      "Unknown"
    );

  return (
    reportStatusNames[raw] ??
    raw
  );
}

function countBy(
  rows: JsonRecord[],
  selector: (
    row: JsonRecord
  ) => string
): CountItem[] {
  const counts =
    new Map<
      string,
      number
    >();

  for (const row of rows) {
    const label =
      selector(row);

    counts.set(
      label,
      (counts.get(label) ?? 0) +
        1
    );
  }

  return Array.from(
    counts.entries()
  )
    .map(
      ([label, count]) => ({
        label,
        count
      })
    )
    .sort(
      (left, right) =>
        right.count -
        left.count ||
        left.label.localeCompare(
          right.label
        )
    );
}

function buildConicGradient(
  items: CountItem[]
): string {
  const total =
    items.reduce(
      (sum, item) =>
        sum + item.count,
      0
    );

  if (total === 0) {
    return "#e5e7eb";
  }

  let current =
    0;

  const segments =
    items.map(
      (item, index) => {
        const start =
          (current / total) *
          100;

        current +=
          item.count;

        const end =
          (current / total) *
          100;

        const color =
          chartColors[
            index %
              chartColors.length
          ];

        return `${color} ${start}% ${end}%`;
      }
    );

  return `conic-gradient(${segments.join(
    ", "
  )})`;
}

function StatusDonut({
  items
}: {
  items: CountItem[];
}) {
  const total =
    items.reduce(
      (sum, item) =>
        sum + item.count,
      0
    );

  return (
    <Stack
      direction={{
        xs: "column",
        sm: "row"
      }}
      spacing={3}
      sx={{
        alignItems: "center",
        justifyContent: "center",
        minHeight: 270
      }}>
      <Box
        sx={{
          position: "relative",
          width: 190,
          height: 190,
          flexShrink: 0
        }}>
        <Box
          sx={{
            width: "100%",
            height: "100%",
            borderRadius: "50%",

            background:
              buildConicGradient(
                items
              )
          }} />

        <Box
          sx={{
            position: "absolute",
            top: "50%",
            left: "50%",
            width: 112,
            height: 112,
            borderRadius: "50%",
            bgcolor: "background.paper",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",

            transform:
              "translate(-50%, -50%)"
          }}>
          <Typography
            variant="h4"
            sx={{
              fontWeight: 700
            }}
          >
            {total}
          </Typography>

          <Typography
            variant="caption"
            sx={{
              color: "text.secondary"
            }}
          >
            Reports
          </Typography>
        </Box>
      </Box>

      <Stack
        spacing={1.25}
        sx={{
          minWidth: 150
        }}
      >
        {items.map(
          (item, index) => (
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
                  flexShrink: 0,

                  bgcolor:
                    chartColors[
                      index %
                        chartColors.length
                    ]
                }} />

              <Typography
                variant="body2"
                sx={{
                  flexGrow: 1
                }}
              >
                {item.label}
              </Typography>

              <Typography
                variant="body2"
                sx={{
                  fontWeight: 700
                }}
              >
                {item.count}
              </Typography>
            </Stack>
          )
        )}
      </Stack>
    </Stack>
  );
}

function TypeBars({
  items
}: {
  items: CountItem[];
}) {
  const maxCount =
    Math.max(
      ...items.map(
        (item) =>
          item.count
      ),
      1
    );

  return (
    <Stack
      spacing={2}
      sx={{
        minHeight: 270,
        justifyContent: "center"
      }}>
      {items.map(
        (item, index) => {
          const width =
            Math.max(
              8,
              (item.count /
                maxCount) *
                100
            );

          return (
            <Box
              key={item.label}
            >
              <Stack
                direction="row"
                spacing={2}
                sx={{
                  justifyContent: "space-between",
                  mb: 0.5
                }}>
                <Typography
                  variant="body2"
                  noWrap
                  title={item.label}
                >
                  {item.label}
                </Typography>

                <Typography
                  variant="body2"
                  sx={{
                    fontWeight: 700
                  }}
                >
                  {item.count}
                </Typography>
              </Stack>

              <Box
                sx={{
                  height: 18,
                  borderRadius: 1,
                  bgcolor: "action.hover",
                  overflow: "hidden"
                }}>
                <Box
                  sx={{
                    height: "100%",
                    width: `${width}%`,
                    borderRadius: 1,

                    bgcolor:
                      chartColors[
                        index %
                          chartColors.length
                      ],

                    transition:
                      "width 300ms ease"
                  }} />
              </Box>
            </Box>
          );
        }
      )}
    </Stack>
  );
}

export function ReportingCharts({
  rows
}: {
  rows: JsonRecord[];
}) {
  if (rows.length === 0) {
    return null;
  }

  const statusData =
    countBy(
      rows,
      (row) =>
        statusLabel(
          row.status
        )
    );

  const reportTypeData =
    countBy(
      rows,
      (row) =>
        normalizeText(
          row.reportType,
          "Unspecified"
        )
    );

  return (
    <Grid
      container
      spacing={2}
      sx={{
        mb: 3
      }}
    >
      <Grid
        size={{
          xs: 12,
          lg: 5
        }}
      >
        <Card
          sx={{
            height: "100%"
          }}
        >
          <CardContent>
            <Typography
              variant="h6"
            >
              Report Status
            </Typography>

            <Typography
              variant="body2"
              sx={{
                color: "text.secondary"
              }}
            >
              Distribution of report
              definitions by lifecycle
              status.
            </Typography>

            <StatusDonut
              items={statusData}
            />
          </CardContent>
        </Card>
      </Grid>

      <Grid
        size={{
          xs: 12,
          lg: 7
        }}
      >
        <Card
          sx={{
            height: "100%"
          }}
        >
          <CardContent>
            <Typography
              variant="h6"
            >
              Reports by Type
            </Typography>

            <Typography
              variant="body2"
              sx={{
                color: "text.secondary"
              }}
            >
              Number of report
              definitions for each
              configured report type.
            </Typography>

            <TypeBars
              items={
                reportTypeData
              }
            />
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
}
