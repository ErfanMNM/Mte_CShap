import {
  OPCUAClient,
  MessageSecurityMode,
  SecurityPolicy,
  NodeId,
  AttributeIds,
  DataType,
} from "node-opcua";
import { readFile } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { dirname, resolve } from "node:path";

const __dirname = dirname(fileURLToPath(import.meta.url));
const projectRoot = resolve(__dirname, "..");

async function loadTaglist() {
  const raw = await readFile(resolve(projectRoot, "taglist.json"), "utf8");
  return JSON.parse(raw);
}

async function browseFolder(session, nodeId, label) {
  console.log(`\n--- Browse ${label} (${nodeId.toString()}) ---`);
  const result = await session.browse(nodeId);
  if (!result || !result.references || result.references.length === 0) {
    console.log("  (no children)");
    return [];
  }
  for (const ref of result.references) {
    console.log(`  ${ref.browseName.toString()}  ->  ${ref.nodeId.toString()}`);
  }
  return result.references;
}

async function main() {
  const tagConfig = await loadTaglist();
  const endpoint = "opc.tcp://127.0.0.1:4840/UA/MteServer";

  const client = OPCUAClient.create({
    endpointMustExist: false,
    securityMode: MessageSecurityMode.None,
    securityPolicy: SecurityPolicy.None,
    requestedSessionTimeout: 60000,
  });

  console.log(`Connecting to ${endpoint} ...`);
  await client.connect(endpoint);
  console.log("Connected.");

  const session = await client.createSession();
  console.log("Session created.");

  try {
    // Walk Objects -> folderName
    const objectsFolder = "i=85"; // Standard OPC UA Objects folder
    const objects = await browseFolder(session, NodeId.resolveNodeId(objectsFolder), "Objects");
    const folderRef = objects.find(
      (r) =>
        r.browseName &&
        typeof r.browseName.name === "string" &&
        r.browseName.name === (tagConfig.objectsFolder || "MteServer"),
    );
    if (!folderRef) {
      console.warn(`Folder "${tagConfig.objectsFolder}" not found under Objects.`);
    } else {
      const tags = await browseFolder(session, folderRef.nodeId, tagConfig.objectsFolder);

      console.log("\n--- Read all tags ---");
      for (const tagRef of tags) {
        try {
          const dv = await session.read({
            nodeId: tagRef.nodeId,
            attributeId: AttributeIds.Value,
          });
          console.log(`  ${tagRef.nodeId.toString().padEnd(40)} = ${JSON.stringify(dv.value.value)}`);
        } catch (err) {
          console.error(`  Read failed for ${tagRef.nodeId.toString()}: ${err.message}`);
        }
      }

      // Try writing on the ReadWrite sample tag (StartCommand) if present
      const sampleWrite = "ns=2;s=StartCommand";
      if (
        tagConfig.tags.some(
          (t) => t.nodeId === sampleWrite && (t.accessLevel || "").toLowerCase() === "readwrite",
        )
      ) {
        console.log(`\n--- Write test on ${sampleWrite} ---`);
        try {
          await session.write({
            nodeId: sampleWrite,
            attributeId: AttributeIds.Value,
            value: {
              value: { dataType: DataType.Boolean, value: true },
            },
          });
          const dv = await session.read({
            nodeId: sampleWrite,
            attributeId: AttributeIds.Value,
          });
          console.log(`  After write: ${JSON.stringify(dv.value.value)}`);
          await session.write({
            nodeId: sampleWrite,
            attributeId: AttributeIds.Value,
            value: {
              value: { dataType: DataType.Boolean, value: false },
            },
          });
        } catch (err) {
          console.error(`  Write failed: ${err.message}`);
        }
      }
    }
  } finally {
    await session.close();
    await client.disconnect();
    console.log("\nDisconnected.");
  }
}

main().catch((err) => {
  console.error(`Quick-test failed: ${err.stack || err.message}`);
  process.exit(1);
});
