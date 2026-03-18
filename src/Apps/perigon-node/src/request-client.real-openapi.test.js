const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');
const os = require('node:os');
const fsp = require('node:fs/promises');

const { generateRequestClient } = require('./index');

test('real openapi json should generate expected angular files', async () => {
  const fixturePath = path.resolve(__dirname, '../../../../tests/StudioMod.Tests/Generate/Fixtures/request-client-special.openapi.json');
  const outDir = await fsp.mkdtemp(path.join(os.tmpdir(), 'perigon-node-openapi-'));

  try {
    const emitted = await generateRequestClient({
      pathOrUrl: fixturePath,
      outputPath: outDir,
      type: 'angular',
      onlyModel: false,
    });

    assert.ok(emitted.length > 0, 'should emit generated files');

    const servicePath = path.join(outDir, 'services', 'demo', 'services', 'user.service.ts');
    const modelPath = path.join(outDir, 'services', 'demo', 'models', 'demo', 'sample-dto.model.ts');

    const serviceContent = await fsp.readFile(servicePath, 'utf8');
    const modelContent = await fsp.readFile(modelPath, 'utf8');

    assert.match(serviceContent, /get\(apiVersion: string, xTenantId: string \| null\)/);
    assert.match(serviceContent, /`\/users\/\$\{apiVersion\}\?x-tenant-id=\$\{xTenantId \?\? ''\}`/);

    assert.match(modelContent, /'@id': string;/);
    assert.match(modelContent, /'#text': string;/);
    assert.match(modelContent, /'api-version': string;/);
  } finally {
    await fsp.rm(outDir, { recursive: true, force: true });
  }
});
