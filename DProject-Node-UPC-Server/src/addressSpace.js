import {
  DataType,
  Variant,
  AccessLevelFlag,
  StatusCodes,
  NodeId,
} from "node-opcua";
import { createSimulator, needsTimer, tickIntervalMs } from "./simulators.js";
import logger from "./logger.js";

const DATA_TYPE_MAP = {
  Boolean: DataType.Boolean,
  Byte: DataType.Byte,
  SByte: DataType.SByte,
  Int16: DataType.Int16,
  UInt16: DataType.UInt16,
  Int32: DataType.Int32,
  UInt32: DataType.UInt32,
  Int64: DataType.Int64,
  UInt64: DataType.UInt64,
  Float: DataType.Float,
  Double: DataType.Double,
  String: DataType.String,
  DateTime: DataType.DateTime,
  Guid: DataType.Guid,
  ByteString: DataType.ByteString,
};

function mapDataType(name) {
  const dt = DATA_TYPE_MAP[name];
  if (dt === undefined) {
    throw new Error(`Unsupported dataType in taglist: ${name}`);
  }
  return dt;
}

function mapAccessLevel(level) {
  switch ((level || "Read").toLowerCase()) {
    case "read":
      return AccessLevelFlag.CurrentRead;
    case "readwrite":
    case "rw":
      return AccessLevelFlag.CurrentRead | AccessLevelFlag.CurrentWrite;
    default:
      return AccessLevelFlag.CurrentRead;
  }
}

function toVariant(dataType, value) {
  return new Variant({ dataType, value });
}

function parseNodeIdString(nodeIdString) {
  // Accept formats like "ns=2;s=Temperature", "ns=2;i=1001", or raw numbers like "i=85"
  return NodeId.resolveNodeId(nodeIdString);
}

/**
 * Builds the OPC UA address space from a parsed taglist config object.
 * Returns an array of runnable tick descriptors for src/server.js to schedule.
 */
export function buildAddressSpace(server, tagConfig) {
  const engine = server.engine;
  const addressSpace = engine.addressSpace;
  if (!addressSpace) throw new Error("AddressSpace not initialized");

  // Register a dedicated namespace (ns=2) so user tag NodeIds match the
  // ns=2;s=... convention used in taglist.json and by C# clients.
  const desiredUri = tagConfig.namespaceUri || "urn:mte:opcua:server";
  let namespace;
  const existingIndex = addressSpace.getNamespaceIndex(desiredUri);
  if (existingIndex > 0) {
    namespace = addressSpace.getNamespace(existingIndex);
  } else {
    namespace = addressSpace.registerNamespace(desiredUri);
  }

  const folderName = tagConfig.objectsFolder || "MteServer";
  const deviceObj = namespace.addObject({
    organizedBy: addressSpace.rootFolder.objects,
    browseName: folderName,
    description: `Simulated tags folder (${folderName})`,
  });

  const descriptors = [];

  for (const tag of tagConfig.tags || []) {
    const dataType = mapDataType(tag.dataType);
    const accessLevel = mapAccessLevel(tag.accessLevel);
    const nextValue = createSimulator(tag);
    // Prime initial value so static tags have a sensible first read.
    const initialValue = nextValue().value;

    const ns2 = parseNodeIdString(tag.nodeId);
    const uaNode = namespace.addVariable({
      componentOf: deviceObj,
      browseName: tag.name,
      nodeId: ns2,
      dataType,
      value: {
        get: () => {
          try {
            const { value } = nextValue();
            return toVariant(dataType, value);
          } catch (err) {
            logger.error(
              `Simulator error on tag "${tag.name}": ${err.message}`,
            );
            return toVariant(dataType, initialValue);
          }
        },
        set: (variant) => {
          // Allow writes from clients; persist the new value for static-like tags.
          tag.value = variant.value;
          return StatusCodes.Good;
        },
      },
      accessLevel,
      userAccessLevel: accessLevel,
    });

    logger.info(
      `Added tag ${tag.nodeId} (${tag.name}, ${tag.dataType}, ${tag.accessLevel || "Read"}, sim=${tag.simulator || "static"})`,
    );

    if (needsTimer(tag)) {
      descriptors.push({
        nodeId: tag.nodeId,
        name: tag.name,
        intervalMs: tickIntervalMs(tag),
        uaNode,
      });
    }
  }

  return descriptors;
}
