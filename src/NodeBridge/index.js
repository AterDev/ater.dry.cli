import dotnet from 'node-api-dotnet';

import './bin/GeneratorForNode.js';
import './bin/Microsoft.OpenApi.Readers.js';
const Runner = dotnet.GeneratorForNode.Runner;

const url = 'http://localhost:5002/swagger/client/swagger.json';
const output = './output';
const type = 1;
const runner = new dotnet.GeneratorForNode.Runner(url, output, type);

await runner.ParseOpenApiAsync();
const baseServiceContent = await runner.GetBaseServiceContent(type);
console.log(baseServiceContent);

// await runner.GenerateRequestServicesAsync();
