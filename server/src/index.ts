import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { registerTools } from "./tools/register.js";
import { getDatabase } from "./database/db.js";
import { createRequire } from "module";

const require = createRequire(import.meta.url);
const { version } = require("../package.json");

// Create server instance
const server = new McpServer({
  name: "mcp-server-for-revit",
  version,
}, {
  instructions:
    "Work safely with the active Revit model. First inspect the model with scoped read tools, then state the exact target and planned change. Do not mutate or delete model data until the user confirms the scope; use dry-run or preview options when available. Keep requests and responses narrow, verify each completed change with a read tool, and summarize the result. Never use send_code_to_revit unless the user explicitly requests custom code and confirms the in-Revit safety prompt.",
});

// Start server
async function main() {
  // Initialize database (sql.js is async)
  await getDatabase();

  // Register tools
  await registerTools(server);

  // Connect to transport layer
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error("Revit MCP Server started successfully");
}

main().catch((error) => {
  console.error("Error starting Revit MCP Server:", error);
  process.exit(1);
});
