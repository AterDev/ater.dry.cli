#!/usr/bin/env node

import dotnet from 'node-api-dotnet';
import { Command } from "commander";

import './bin/GeneratorForNode.js';
import './bin/Microsoft.OpenApi.Readers.js';

const program = new Command();

program.name('drygen')
  .description('Generate request services(TS) from OpenAPI doc');

program.command('service')
  .description('生成请求服务代码')
  .argument('<url>', 'OpenAPI文档地址')
  .option('-o, --output <output>', '输出目录')
  .option('-t, --type [type]', '请求库类型，axios或ngHttp')
  .action((url, options) => {
    let typeValue = 0;
    switch (options.type) {
      case 'axios':
        typeValue = 1;
        break;
      case 'ngHttp':
        typeValue = 0;
        break;
      default:
        console.log('[type]参数仅支持axios或ngHttp');
        return;
    }
    const output = options.output || './output';
    const runner = new dotnet.GeneratorForNode.Runner(url, output, typeValue);
    runner.ParseOpenApiAsync();
  })

program
  .action(() => {
    console.log('No command provided. Use --help to see available commands.');
  });

program.on('--help', () => {
  console.log('Generate request services(TS) from OpenAPI doc');
  console.log('Examples:');
  console.log('  $ drygen http://swagger/swagger.json -o ./output -t axios');
});

program.parse(process.argv);