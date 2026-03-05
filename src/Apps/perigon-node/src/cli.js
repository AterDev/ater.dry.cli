const { Command } = require('commander');
const { generateRequestClient, getSupportedRequestTypes } = require('./index');

function runCli(argv) {
  const program = new Command();

  program
    .name('drygen')
    .description('DryGen OpenAPI request client generator')
    .version('0.2.1');

  program
    .command('request')
    .argument('<pathOrUrl>', 'openapi json path or url')
    .argument('<outputPath>', 'output directory path')
    .option('-t, --type <type>', 'client type: angular|axios', 'angular')
    .option('-m, --only-model', 'generate models only', false)
    .action(async (pathOrUrl, outputPath, options) => {
      const type = String(options.type || 'angular').toLowerCase();
      if (!getSupportedRequestTypes().includes(type)) {
        throw new Error(`Invalid type: ${type}. Supported: ${getSupportedRequestTypes().join(', ')}`);
      }

      const files = await generateRequestClient({
        pathOrUrl,
        outputPath,
        type,
        onlyModel: Boolean(options.onlyModel),
      });

      console.log(`Generated ${files.length} file(s).`);
      files.forEach((f) => console.log(f));
    });

  return program.parseAsync(argv);
}

module.exports = { runCli };
