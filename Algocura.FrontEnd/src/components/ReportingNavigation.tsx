import { Button, Stack } from "@mui/material";
import { Link, useLocation } from "react-router";

export function ReportingNavigation() {
  const location = useLocation();
  const analyticsSelected =
    location.pathname.startsWith("/reporting/analytics");

  return (
    <Stack
      direction="row"
      spacing={1}
      useFlexGap
      sx={{
        mb: 3,
        flexWrap: "wrap"
      }}>
      <Button
        component={Link}
        to="/reporting"
        variant={analyticsSelected ? "outlined" : "contained"}
      >
        Report Definitions
      </Button>

      <Button
        component={Link}
        to="/reporting/analytics"
        variant={analyticsSelected ? "contained" : "outlined"}
      >
        Analytics
      </Button>
    </Stack>
  );
}
