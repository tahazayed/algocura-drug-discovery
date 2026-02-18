import { Box, CircularProgress, Typography } from "@mui/material";

export function LoadingState() {
  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        gap: 2,
        py: 4
      }}>
      <CircularProgress size={24} />
      <Typography sx={{
        color: "text.secondary"
      }}>
        Loading...
      </Typography>
    </Box>
  );
}
