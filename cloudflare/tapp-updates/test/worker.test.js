import test from "node:test";
import assert from "node:assert/strict";
import worker from "../src/index.js";

const payload = {
  SchemaVersion: 1,
  AppId: "TApp",
  Channel: "stable",
  Version: "9.0.1",
  Platform: "win-x64",
  PackageKey: "releases/9.0.1/win-x64/TApp-win-x64.zip",
  Sha256: "a".repeat(64),
  Size: 2048,
  ReleaseNotes: "Bản sửa lỗi",
};
const envelope = {
  PayloadBase64: Buffer.from(JSON.stringify(payload)).toString("base64"),
  SignatureBase64: "test-signature",
};

function envWith(objects = {}) {
  return { UPDATES_BUCKET: { async get(key) { return objects[key] ?? null; } } };
}

test("serves channel manifests without caching", async () => {
  const env = envWith({ "channels/stable/latest.json": { text: async () => JSON.stringify(envelope) } });
  const response = await worker.fetch(new Request("https://updates.test/v1/updates/stable?platform=win-x64"), env);
  assert.equal(response.status, 200);
  assert.match(response.headers.get("cache-control"), /no-store/);
  assert.deepEqual(await response.json(), envelope);
});

test("rejects an unsupported platform and malformed package path", async () => {
  const env = envWith();
  assert.equal((await worker.fetch(new Request("https://updates.test/v1/updates/stable?platform=win-arm64"), env)).status, 404);
  assert.equal((await worker.fetch(new Request("https://updates.test/v1/packages/releases/9.0.1/../../secret.zip"), env)).status, 404);
});

test("streams package objects from the private bucket", async () => {
  const expected = "releases/9.0.1/win-x64/TApp-win-x64.zip";
  let requested;
  const env = envWith({ [expected]: { size: 12, httpEtag: '"abc"', body: new Response("package").body } });
  const originalGet = env.UPDATES_BUCKET.get;
  env.UPDATES_BUCKET.get = async key => { requested = key; return originalGet(key); };
  const response = await worker.fetch(new Request(`https://updates.test/v1/packages/${expected}`), env);
  assert.equal(response.status, 200);
  assert.equal(requested, expected);
  assert.match(response.headers.get("cache-control"), /immutable/);
  assert.equal(await response.text(), "package");
});
