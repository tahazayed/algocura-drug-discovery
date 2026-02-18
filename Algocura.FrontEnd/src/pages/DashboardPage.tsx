import {
  Card,
  CardActionArea,
  CardContent,
  Chip,
  Grid,
  Typography
} from "@mui/material";
import { Link } from "react-router";
import { useAuth } from "../auth/AuthContext";
import { services } from "../config/services";
import { PageHeader } from "../components/PageHeader";

export function DashboardPage() {
  const auth = useAuth();

  const visible = services.filter((service) =>
    auth.hasAnyRole(service.allowedRoles)
  );

  return (
    <>
      <PageHeader
        title="Discovery Workspace"
        description="Access Algocura services based on your assigned platform roles."
      />

      <Grid container spacing={2}>
        {visible.map((service) => (
          <Grid
            key={service.key}
            size={{ xs: 12, sm: 6, lg: 4 }}
          >
            <Card sx={{ height: "100%" }}>
              <CardActionArea
                component={Link}
                to={service.route}
                sx={{ height: "100%" }}
              >
                <CardContent>
                  <Chip
                    size="small"
                    label={service.category}
                    sx={{ mb: 1.5 }}
                  />
                  <Typography variant="h6">
                    {service.name}
                  </Typography>
                  <Typography
                    sx={{
                      color: "text.secondary",
                      mt: 1
                    }}>
                    {service.description}
                  </Typography>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    </>
  );
}
