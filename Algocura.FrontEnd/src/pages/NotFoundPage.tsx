import { Button } from "@mui/material";
import { Link } from "react-router";
import { PageHeader } from "../components/PageHeader";

export function NotFoundPage() {
  return (
    <>
      <PageHeader
        title="Page not found"
        description="The requested frontend route does not exist."
      />
      <Button component={Link} to="/" variant="contained">
        Dashboard
      </Button>
    </>
  );
}
