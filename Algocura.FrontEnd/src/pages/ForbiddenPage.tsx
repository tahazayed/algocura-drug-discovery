import { Alert, Button, Stack } from "@mui/material";
import { Link } from "react-router";
import { PageHeader } from "../components/PageHeader";

export function ForbiddenPage() {
  return (
    <>
      <PageHeader title="Access denied" />
      <Stack
        sx={{
          gap: 2,
          alignItems: "flex-start"
        }}>
        <Alert severity="warning">
          Your account is authenticated but does not have a
          role permitted for this area.
        </Alert>
        <Button component={Link} to="/" variant="contained">
          Return to dashboard
        </Button>
      </Stack>
    </>
  );
}
