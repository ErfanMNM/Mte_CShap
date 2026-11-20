const headers = {
  "x-content-type-options": "nosniff",
  "access-control-allow-origin": "*",
};

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    if (request.method !== "GET" && request.method !== "HEAD") {
      return new Response("Method not allowed", { status: 405, headers: { ...headers, allow: "GET, HEAD" } });
    }

    const manifestMatch = url.pathname.match(/^\/v1\/updates\/(stable|beta)$/);
    if (manifestMatch) {
      if (url.searchParams.get("platform") !== "win-x64") return notFound();
      const object = await env.UPDATES_BUCKET.get(`channels/${manifestMatch[1]}/latest.json`);
      if (!object) return notFound();
      try {
        const envelope = JSON.parse(await object.text());
        const manifest = JSON.parse(new TextDecoder().decode(Uint8Array.from(atob(envelope.PayloadBase64), c => c.charCodeAt(0))));
        if (manifest.SchemaVersion !== 1 || manifest.AppId !== "TApp" || manifest.Channel !== manifestMatch[1] ||
            manifest.Platform !== "win-x64" || !validPackageKey(manifest.PackageKey, manifest.Version) ||
            !/^[a-fA-F0-9]{64}$/.test(manifest.Sha256) || !Number.isSafeInteger(manifest.Size) || manifest.Size <= 0 ||
            manifest.Size > 1_073_741_824 || typeof manifest.ReleaseNotes !== "string" || manifest.ReleaseNotes.length > 4000 ||
            typeof envelope.SignatureBase64 !== "string") return notFound();
        return new Response(request.method === "HEAD" ? null : JSON.stringify(envelope), {
          headers: { ...headers, "content-type": "application/json; charset=utf-8", "cache-control": "no-store, max-age=0" },
        });
      } catch {
        return new Response("Invalid release manifest", { status: 503, headers: { ...headers, "cache-control": "no-store" } });
      }
    }

    const packagePath = decodePackagePath(url.pathname);
    if (!packagePath) return notFound();
    const object = await env.UPDATES_BUCKET.get(packagePath);
    if (!object) return notFound();

    const responseHeaders = new Headers(headers);
    responseHeaders.set("content-type", "application/zip");
    responseHeaders.set("content-length", String(object.size));
    responseHeaders.set("content-disposition", 'attachment; filename="TApp-win-x64.zip"');
    responseHeaders.set("cache-control", "public, max-age=31536000, immutable");
    if (object.httpEtag) responseHeaders.set("etag", object.httpEtag);
    return new Response(request.method === "HEAD" ? null : object.body, { headers: responseHeaders });
  },
};

function validPackageKey(key, version) {
  return typeof key === "string" && typeof version === "string" &&
    /^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$/.test(version) &&
    /^releases\/(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?\/win-x64\/TApp-win-x64\.zip$/.test(key) &&
    key === `releases/${version}/win-x64/TApp-win-x64.zip`;
}

function decodePackagePath(pathname) {
  const prefix = "/v1/packages/";
  if (!pathname.startsWith(prefix)) return null;
  const key = pathname.slice(prefix.length);
  try {
    const decoded = decodeURIComponent(key);
    const match = decoded.match(/^releases\/(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?\/win-x64\/TApp-win-x64\.zip$/);
    return match ? decoded : null;
  } catch { return null; }
}

function notFound() {
  return new Response("Not found", { status: 404, headers: { ...headers, "cache-control": "no-store" } });
}
