import { useState, type FormEvent } from "react";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Container,
  TextField,
  Typography
} from "@mui/material";
import { Navigate, useLocation, useNavigate } from "react-router";
import { useAuth } from "../auth/AuthContext";

export function LoginPage() {
  const auth = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState(
    "scientist@algocura.local"
  );
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  if (auth.isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError("");

    try {
      await auth.login({ email, password });

      const state = location.state as
        | { from?: string }
        | null;

      navigate(state?.from ?? "/", {
        replace: true
      });
    } catch {
      setError(
        "Login failed. Verify the email, password, and Identity API."
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "grid",

        gridTemplateColumns: {
          xs: "1fr",
          md: "1.2fr 1fr"
        },

        bgcolor: "background.default"
      }}>
      <Box
        sx={{
          display: { xs: "none", md: "flex" },
          flexDirection: "column",
          justifyContent: "center",
          px: 8,
          color: "white",

          background:
            "linear-gradient(145deg, #052e16 0%, #14532d 58%, #0f766e 100%)"
        }}>
        <Typography variant="h2" sx={{
          fontWeight: 800
        }}>
          Algocura
        </Typography>
        <Typography
          variant="h5"
          sx={{
            mt: 2,
            maxWidth: 600
          }}>
          AI-driven drug-discovery platform for literature,
          targets, chemical design, screening, assays and
          adaptive molecular-model routing.
        </Typography>
      </Box>

      <Container
        maxWidth="sm"
        sx={{
          display: "flex",
          alignItems: "center"
        }}
      >
        <Card sx={{ width: "100%" }}>
          <CardContent sx={{ p: 4 }}>
            <Typography variant="h4">
              Sign in
            </Typography>
            <Typography
              sx={{
                color: "text.secondary",
                mb: 3
              }}>
              Use an Algocura Identity account.
            </Typography>

            {error ? (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            ) : null}

            <Box component="form" onSubmit={submit}>
              <TextField
                label="Email"
                type="email"
                fullWidth
                required
                value={email}
                onChange={(event) =>
                  setEmail(event.target.value)
                }
                margin="normal"
              />

              <TextField
                label="Password"
                type="password"
                fullWidth
                required
                value={password}
                onChange={(event) =>
                  setPassword(event.target.value)
                }
                margin="normal"
              />

              <Button
                type="submit"
                variant="contained"
                fullWidth
                size="large"
                disabled={busy}
                sx={{ mt: 2 }}
              >
                {busy ? "Signing in..." : "Sign in"}
              </Button>
            </Box>
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}
