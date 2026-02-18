import { useState } from "react";
import {
  AppBar,
  Box,
  Button,
  Chip,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Toolbar,
  Typography
} from "@mui/material";
import { Link, Outlet, useLocation } from "react-router";
import { services } from "../config/services";
import { useAuth } from "../auth/AuthContext";

const drawerWidth = 250;

export function AppShell() {
  const auth = useAuth();
  const location = useLocation();
  const [drawerOpen, setDrawerOpen] = useState(false);

  const visibleServices = services.filter((service) =>
    auth.hasAnyRole(service.allowedRoles)
  );

  const closeDrawer = () => setDrawerOpen(false);

  return (
    <Box sx={{
      minHeight: "100vh"
    }}>
      <AppBar
        position="fixed"
        sx={{
          zIndex: (theme) => theme.zIndex.drawer + 1,
          left: 0,
          right: 0,
          width: "100%"
        }}
      >
        <Toolbar
          sx={{
            width: "100%",
            boxSizing: "border-box",
            gap: 1
          }}
        >
          <IconButton
            color="inherit"
            edge="start"
            aria-label="open navigation"
            onClick={() => setDrawerOpen(true)}
            sx={{ mr: 1 }}
          >
            <Box
              component="span"
              aria-hidden="true"
              sx={{
                fontSize: 28,
                lineHeight: 1,
                fontWeight: 400
              }}
            >
              ☰
            </Box>
          </IconButton>

          <Typography
            variant="h6"
            component={Link}
            to="/"
            noWrap
            sx={{
              color: "inherit",
              textDecoration: "none",
              flex: "1 1 auto",
              minWidth: 0,
              // color: "white",
              fontWeight: 600
            }}>
            Algocura Drug Discovery
          </Typography>

          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              gap: 1,
              flexShrink: 0,
              ml: 1
            }}
          >
            {auth.principal?.roles.map((role) => (
              <Chip
                key={role}
                label={role}
                size="small"
                sx={{
                  bgcolor: "rgba(255,255,255,.16)",
                  color: "white",
                  display: { xs: "none", sm: "inline-flex" }
                }}
              />
            ))}

            <Typography
              variant="body2"
              noWrap
              sx={{
                color: "white",
                maxWidth: 220,
                display: { xs: "none", md: "block" }
              }}
            >
              {auth.principal?.name || auth.principal?.email}
            </Typography>

            <Button
              color="inherit"
              onClick={auth.logout}
              sx={{
                color: "white",
                whiteSpace: "nowrap",
                flexShrink: 0
              }}
            >
              Sign out
            </Button>
          </Box>
        </Toolbar>
      </AppBar>

      <Drawer
        variant="temporary"
        open={drawerOpen}
        onClose={closeDrawer}
        ModalProps={{
          keepMounted: true
        }}
        sx={{
          "& .MuiDrawer-paper": {
            width: drawerWidth,
            boxSizing: "border-box"
          }
        }}
      >
        <Toolbar />
        <Box sx={{
          overflow: "auto"
        }}>
          <List>
            <ListItemButton
              component={Link}
              to="/"
              selected={location.pathname === "/"}
              onClick={closeDrawer}
            >
              <ListItemText primary="Dashboard" />
            </ListItemButton>
          </List>

          <Divider />

          <List>
            {visibleServices.map((service) =>
              service.key === "reporting" ? (
                <Box key={service.key}>
                  <ListItemButton
                    component={Link}
                    to="/reporting"
                    selected={
                      location.pathname ===
                      "/reporting"
                    }
                    onClick={closeDrawer}
                  >
                    <ListItemText
                      primary="Reporting"
                      secondary="Platform"
                    />
                  </ListItemButton>

                  <ListItemButton
                    component={Link}
                    to="/reporting/analytics"
                    selected={
                      location.pathname.startsWith(
                        "/reporting/analytics"
                      )
                    }
                    onClick={closeDrawer}
                    sx={{ pl: 4 }}
                  >
                    <ListItemText
                      primary="Analytics"
                    />
                  </ListItemButton>
                </Box>
              ) : (
                <ListItemButton
                  key={service.key}
                  component={Link}
                  to={service.route}
                  selected={
                    location.pathname.startsWith(
                      service.route
                    )
                  }
                  onClick={closeDrawer}
                >
                  <ListItemText
                    primary={service.shortName}
                    secondary={service.category}
                  />
                </ListItemButton>
              )
            )}
          </List>
        </Box>
      </Drawer>

      <Box
        component="main"
        sx={{
          width: "100%",
          boxSizing: "border-box",
          p: 3
        }}
      >
        <Toolbar />
        <Outlet />
      </Box>
    </Box>
  );
}