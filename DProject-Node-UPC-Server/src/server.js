import { readFile } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { dirname, resolve } from "node:path";
import {
  OPCUAServer,
  MessageSecurityMode,
  SecurityPolicy,
} from "node-opcua";

import { buildAddressSpace } from "./addressSpace.js";
import logger from "./logger.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const projectRoot = resolve(__dirname, "..");

async function loadTagConfig(path) {
  const raw = await readFile(path, "utf8");
  return JSON.parse(raw);
}

function parseArgs(argv) {
  const args = { taglist: resolve(projectRoot, "taglist.json"), port: 4840 };
  for (let i = 2; i < argv.length; i++) {
    const a = argv[i];
    if (a === "--taglist" && argv[i + 1]) {
      args.taglist = resolve(process.cwd(), argv[++i]);
    } else if (a === "--port" && argv[i + 1]) {
      args.port = parseInt(argv[++i], 10);
    }
  }
  return args;
}

async function main() {
  const args = parseArgs(process.argv);
  logger.info(`Loading taglist from ${args.taglist}`);
  const tagConfig = await loadTagConfig(args.taglist);

  const server = new OPCUAServer({
    port: args.port,
    resourcePath: "/UA/MteServer",
    serverInfo: {
      applicationName: "MteNodeServer",
      applicationUri: "urn:mte:opcua:node-server",
      productUri: "urn:mte:opcua:node",
      manufacturerName: "MTE",
      productName: "MteNodeOpcUaServer",
      softwareVersion: "1.0.0",
      buildNumber: "1",
    },
    securityModes: [MessageSecurityMode.None],
    securityPolicies: [SecurityPolicy.None],
    allowAnonymous: true,
    disableDiscovery: false,
  });

  await server.initialize();

  const tickDescriptors = buildAddressSpace(server, tagConfig);

  // For simulated tags, periodically "touch" the variable so monitors that
  // already subscribed receive updates. Without this, get() is only invoked on
  // client Read requests.
  const timers = tickDescriptors.map((d) =>
    setInterval(() => {
      try {
        d.uaNode.readValue(); // forces the getter to run
      } catch (err) {
        logger.warn(`Tick error on ${d.name}: ${err.message}`);
      }
    }, d.intervalMs),
  );

  await server.start();
  const endpoint = server.getEndpointUrl();
  logger.info(`OPC UA Server listening at ${endpoint}`);
  logger.info(`Use Ctrl+C to stop.`);

  let shuttingDown = false;
  async function shutdown(signal) {
    if (shuttingDown) return;
    shuttingDown = true;
    logger.info(`Received ${signal}, shutting down...`);
    for (const t of timers) clearInterval(t);
    try {
      await server.shutdown(1000);
      logger.info("Server stopped cleanly.");
    } catch (err) {
      logger.error(`Shutdown error: ${err.message}`);
    } finally {
      process.exit(0);
    }
  }

  process.on("SIGINT", () => shutdown("SIGINT"));
  process.on("SIGTERM", () => shutdown("SIGTERM"));
}

main().catch((err) => {
  logger.error(`Fatal: ${err.stack || err.message}`);
  process.exit(1);
});
