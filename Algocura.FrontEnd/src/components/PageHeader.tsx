import { Box, Typography } from "@mui/material";

export function PageHeader({
  title,
  description
}: {
  title: string;
  description?: string;
}) {
  return (
    <Box sx={{
      mb: 3
    }}>
      <Typography variant="h4" component="h1">
        {title}
      </Typography>
      {description ? (
        <Typography
          sx={{
            color: "text.secondary",
            mt: 0.5
          }}>
          {description}
        </Typography>
      ) : null}
    </Box>
  );
}
