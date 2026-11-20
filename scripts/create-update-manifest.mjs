import { createSign } from "node:crypto";
import { readFile, writeFile } from "node:fs/promises";

const required = ["UPDATE_PRIVATE_KEY_PEM", "UPDATE_VERSION", "UPDATE_SHA256", "UPDATE_SIZE", "UPDATE_CHANNEL"];
for (const name of required) {
  if (!process.env[name]) throw new Error(`Missing ${name}`);
}

const { UPDATE_PRIVATE_KEY_PEM, UPDATE_VERSION, UPDATE_SHA256, UPDATE_SIZE, UPDATE_CHANNEL } = process.env;
const packagePath = `releases/${UPDATE_VERSION}/win-x64/TApp-win-x64.zip`;
const versionMatch = UPDATE_VERSION.match(/^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$/);
if (!versionMatch || versionMatch.slice(1, 4).some(part => Number(part) > 2147483647) ||
    (versionMatch[4] ?? "").split(".").some(part => /^0\d+$/.test(part))) {
  throw new Error("UPDATE_VERSION must be a SemVer release version.");
}
if (!/^[a-fA-F0-9]{64}$/.test(UPDATE_SHA256) || !/^\d+$/.test(UPDATE_SIZE) || Number(UPDATE_SIZE) <= 0) {
  throw new Error("Invalid package hash or size.");
}
if (UPDATE_CHANNEL !== "stable" && UPDATE_CHANNEL !== "beta") throw new Error("Invalid release channel.");

const payload = Buffer.from(JSON.stringify({
  SchemaVersion: 1,
  AppId: "TApp",
  Channel: UPDATE_CHANNEL,
  Version: UPDATE_VERSION,
  Platform: "win-x64",
  PackageKey: packagePath,
  Sha256: UPDATE_SHA256.toUpperCase(),
  Size: Number(UPDATE_SIZE),
  ReleaseNotes: process.env.UPDATE_RELEASE_NOTES ?? "",
}), "utf8");
const signer = createSign("RSA-SHA256");
signer.update(payload);
signer.end();
const envelope = {
  PayloadBase64: payload.toString("base64"),
  SignatureBase64: signer.sign(UPDATE_PRIVATE_KEY_PEM).toString("base64"),
};
await writeFile(process.env.UPDATE_MANIFEST_OUT ?? "manifest.json", JSON.stringify(envelope));
console.log(`Signed ${UPDATE_CHANNEL} manifest for ${UPDATE_VERSION}`);
