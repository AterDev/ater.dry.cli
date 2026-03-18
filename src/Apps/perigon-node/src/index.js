const fsp = require('node:fs/promises');
const http = require('node:http');
const https = require('node:https');
const path = require('node:path');
const SwaggerParser = require('@apidevtools/swagger-parser');

const SUPPORTED_TYPES = ['angular', 'axios'];

function getSupportedRequestTypes() {
  return SUPPORTED_TYPES;
}

async function generateRequestClient({ pathOrUrl, outputPath, type = 'angular', onlyModel = false }) {
  if (!pathOrUrl || !outputPath) {
    throw new Error('Both pathOrUrl and outputPath are required.');
  }

  const normalizedType = String(type).toLowerCase();
  if (!SUPPORTED_TYPES.includes(normalizedType)) {
    throw new Error(`Invalid type: ${normalizedType}. Supported: ${SUPPORTED_TYPES.join(', ')}`);
  }

  const doc = await loadOpenApiDocument(pathOrUrl);
  const clientName = getClientName(doc);
  const emitted = [];
  const clientRootDir = path.join(outputPath, 'services', clientName);

  await cleanupGeneratedDirs(clientRootDir);

  if (!onlyModel) {
    const baseServicePath = path.join(clientRootDir, 'base.service.ts');
    const baseExists = await fileExists(baseServicePath);
    if (!baseExists) {
      const baseServiceContent = getBaseService(normalizedType).replaceAll('BASE_URL', `${clientName.toUpperCase()}_BASE_URL`);
      await writeFile(baseServicePath, baseServiceContent);
      emitted.push(baseServicePath);
    }
  }

  if (normalizedType === 'angular') {
    const pipePath = path.join(outputPath, 'pipe', clientName, 'enum-text.pipe.ts');
    const pipeContent = buildEnumPipe(doc?.components?.schemas || {});
    await writeFile(pipePath, pipeContent);
    emitted.push(pipePath);
  }

  const schemaMetas = parseSchemas(doc?.components?.schemas || {}, doc);
  for (const meta of schemaMetas) {
    const nsDir = toHyphen(getNamespaceFirstPart(meta.namespace));
    const filePath = path.join(outputPath, 'services', clientName, 'models', nsDir, `${toHyphen(meta.name)}.model.ts`);
    const content = generateModel(meta);
    await writeFile(filePath, content);
    emitted.push(filePath);
  }

  if (!onlyModel) {
    const operations = parseOperations(doc);
    if (normalizedType === 'angular') {
      const serviceNames = [];
      const grouped = groupByTag(operations);
      for (const [tag, funcs] of grouped.entries()) {
        const filePath = path.join(outputPath, 'services', clientName, 'services', `${toHyphen(tag)}.service.ts`);
        const content = generateAngularService(tag, funcs, schemaMetas, doc);
        await writeFile(filePath, content);
        emitted.push(filePath);
        serviceNames.push(tag);
      }

      const clientFilePath = path.join(outputPath, 'services', clientName, `${clientName}-client.ts`);
      const clientContent = generateAngularClient(clientName, serviceNames, doc.tags || []);
      await writeFile(clientFilePath, clientContent);
      emitted.push(clientFilePath);
    } else {
      const grouped = groupByTag(operations);
      for (const [tag, funcs] of grouped.entries()) {
        const filePath = path.join(outputPath, 'services', clientName, 'services', `${toHyphen(tag)}.service.ts`);
        const content = generateAxiosService(tag, funcs, schemaMetas, doc);
        await writeFile(filePath, content);
        emitted.push(filePath);
      }
    }
  }

  return emitted;
}

function getBaseService(type) {
  if (type === 'angular') {
    return `import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { from, map, Observable, switchMap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BaseService {
  protected baseUrl: string | null;
  constructor(
    protected http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    if (baseUrl.endsWith('/')) {
      this.baseUrl = baseUrl.slice(0, -1);
    } else {
      this.baseUrl = baseUrl;
    }
  }

  protected request<T = any>(method: string, path: string, body?: any): Observable<T> {
    const url = this.baseUrl + path;
    return this.http.request(method, url, {
      headers: this.getHeaders(),
      body,
      responseType: 'blob',
      observe: 'response'
    }).pipe(
      switchMap((resp: HttpResponse<Blob>) => {
        const contentType = (resp.headers.get('Content-Type') || '').toLowerCase();
        const disposition = resp.headers.get('Content-Disposition') || '';
        const blob = resp.body as Blob;

        const isAttachment = /attachment/i.test(disposition);
        const isBinaryType = this.isBinaryContentType(contentType);
        const treatAsFile = isAttachment || isBinaryType;

        if (treatAsFile) {
          return from(Promise.resolve(blob as unknown as T));
        }
        return from(blob.text()).pipe(
          map(text => this.parseAuto<T>(text, contentType))
        );
      })
    );
  }

  private parseAuto<T>(text: string, contentType: string): T {
    if (contentType.includes('application/json') || contentType.includes('text/json')) {
      return JSON.parse(text) as T;
    }
    return text as unknown as T;
  }

  private isBinaryContentType(ct: string): boolean {
    if (!ct) return false;
    return (
      ct.startsWith('application/octet-stream') ||
      ct.startsWith('application/pdf') ||
      ct.startsWith('application/zip') ||
      ct.startsWith('application/vnd') ||
      ct.startsWith('image/') ||
      ct.startsWith('video/') ||
      ct.startsWith('audio/') ||
      ct.startsWith('font/') ||
      ct.includes('application/msword') ||
      ct.includes('application/excel')
    );
  }

  protected openFile(blob: Blob, filename: string) {
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  protected getHeaders(): HttpHeaders {
    return new HttpHeaders({
      Accept: 'application/json, text/plain, */*',
      Authorization: 'Bearer ' + localStorage.getItem('accessToken')
    });
  }
  public isMobile(): boolean {
    const ua = navigator.userAgent;
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua)) {
      return true;
    }
    return false;
  }
}
export interface ErrorResult {
  title: string;
  detail: string;
  status: number;
}
`;
  }

  return `import axios from 'axios'
import type {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  Method
} from 'axios'

declare module 'axios' {
  interface AxiosRequestConfig {
    loading?: boolean
    retry?: boolean
    __retryCount?: number
    cancel?: boolean | {
      repeat?: boolean
    }
    errorMsg?: boolean
  }
}

interface Options {
  base: AxiosRequestConfig
  interceptors?: {
    request?: (config: AxiosRequestConfig) => void
    response?: (response: AxiosResponse) => void
    responseError?: (response: AxiosResponse) => void
  }
  loadingService?: {
    loadingStart: () => void
    loadingClose: () => void
  }
}

export class BaseService {
  private http: AxiosInstance
  constructor() {
    this.http = axios.create(options.base)
  }

  protected request<R>(
    method: Method,
    path: string,
    body?: any,
    ext?: ExtOptions
  ): Promise<R> {
    return this.http.request<any, R, any>({
      url: path,
      method,
      params: ['get', 'delete'].includes(method) ? body : undefined,
      data: ['post', 'put'].includes(method) ? body : undefined,
      ...ext
    })
  }
}

export type ExtOptions = AxiosRequestConfig

const { VITE_APP_SERVER_URL } = import.meta.env
const options: Options = {
  base: {
    baseURL: VITE_APP_SERVER_URL
  }
}
`;
}

function buildEnumPipe(schemas) {
  const blocks = [];
  for (const [schemaKey, schema] of Object.entries(schemas)) {
    const items = getEnumData(schema);
    if (!items.length) continue;
    const enumType = formatSchemaKey(schemaKey);
    blocks.push(`      case '${enumType}':`);
    blocks.push(`        switch (value) {`);
    for (const item of items) {
      blocks.push(`          case ${item.value}: result = '${escapeSingle(item.description || item.name || String(item.value))}'; break;`);
    }
    blocks.push(`          default: result = '默认'; break;`);
    blocks.push(`        }`);
    blocks.push(`        break;`);
  }

  return `// 该文件自动生成，会被覆盖更新
import { Injectable, Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumText'
})
@Injectable({ providedIn: 'root' })
export class EnumTextPipe implements PipeTransform {
  transform(value: unknown, type: string): string {
    let result = '';
    switch (type) {
${blocks.join('\n')}
      default:
        break;
    }
    return result;
  }
}
`;
}

function parseSchemas(schemas, doc) {
  return Object.entries(schemas).map(([schemaKey, schema]) => parseSchemaMeta(schemaKey, schema, doc));
}

function parseSchemaMeta(schemaKey, schema, doc) {
  const genericParams = parseGenericParams(schemaKey);
  const meta = {
    name: formatSchemaKey(schemaKey),
    fullName: schemaKey,
    namespace: getNamespace(schemaKey),
    comment: schema?.description || '',
    isEnum: isEnumSchema(schema),
    propertyInfos: [],
    genericParams,
  };

  if (meta.isEnum) {
    meta.propertyInfos = getEnumData(schema).map((i) => ({
      name: i.name,
      defaultValue: String(i.value),
      commentSummary: i.description || i.name,
      type: 'Enum(int)',
      isEnum: true,
      isList: false,
      isNullable: false,
      isRequired: true,
      navigationName: '',
    }));
    return meta;
  }

  const merged = collectSchemaProperties(schema, doc);
  const required = new Set(Array.isArray(schema?.required) ? schema.required : []);

  for (const [propName, propSchema] of Object.entries(merged)) {
    const navName = getRootRef(propSchema);
    let propType = getLanguageType(propSchema, doc);
    const nullable = isNullableSchema(propSchema);

    if (meta.genericParams.length > 0) {
      const normalizedPropType = formatSchemaKey(stripArraySuffix(propType));
      const genericTypeIndex = meta.genericParams.findIndex((p) => p === normalizedPropType);
      if (genericTypeIndex >= 0) {
        const placeholder = `T${genericTypeIndex + 1}`;
        if (propType.endsWith('[]')) {
          propType = `${placeholder}[]`;
        } else {
          propType = placeholder;
        }
      }
    }

    meta.propertyInfos.push({
      name: propName,
      type: propType,
      commentSummary: propSchema?.description || propName,
      isEnum: isEnumSchema(propSchema),
      isList: isArraySchema(propSchema),
      isNullable: nullable,
      isRequired: required.has(propName) && !nullable,
      navigationName: navName || '',
      referencedTypes: collectSchemaRefNames(propSchema),
    });
  }

  return meta;
}

function collectSchemaProperties(schema, doc) {
  const result = {};
  if (!schema) return result;

  if (Array.isArray(schema.allOf)) {
    for (const part of schema.allOf) {
      const partSchema = resolveSchema(part, doc);
      Object.assign(result, collectSchemaProperties(partSchema, doc));
    }
  }

  if (schema.properties && typeof schema.properties === 'object') {
    Object.assign(result, schema.properties);
  }

  const resolved = resolveSchema(schema, doc);
  if (resolved !== schema && resolved?.properties && typeof resolved.properties === 'object') {
    Object.assign(result, resolved.properties);
  }

  return result;
}

function parseOperations(doc) {
  const functions = [];
  const paths = doc?.paths || {};

  for (const [routePath, pathItem] of Object.entries(paths)) {
    const pathParams = Array.isArray(pathItem.parameters) ? pathItem.parameters : [];
    for (const [method, operation] of Object.entries(pathItem)) {
      if (!isHttpMethod(method) || !operation || typeof operation !== 'object') continue;

      const tag = operation.tags?.[0] || 'default';
      let name = operation.operationId || `${method}${routePath.split('/').filter(Boolean).at(-1) || 'Api'}`;

      const reqSchema = operation.requestBody?.content
        ? Object.values(operation.requestBody.content)[0]?.schema
        : undefined;

      const responses = operation.responses || {};
      const responseKeys = Object.keys(responses);
      let selected = null;
      if (responseKeys.length === 1) {
        selected = responses[responseKeys[0]];
      } else if (responses['200']) {
        selected = responses['200'];
      } else if (responseKeys.length > 0) {
        selected = responses[responseKeys[0]];
      }
      const respSchema = selected?.content ? Object.values(selected.content)[0]?.schema : undefined;

      const allParams = [...pathParams, ...(Array.isArray(operation.parameters) ? operation.parameters : [])];
      const usedParamNames = new Set();
      const params = allParams.map((p) => ({
        name: normalizeParameterName(p.name, usedParamNames),
        originalName: p.name,
        type: getLanguageType(p.schema, doc),
        refType: getRootRef(p.schema),
        referencedTypes: collectSchemaRefNames(p.schema),
        description: p.description,
        isRequired: Boolean(p.required),
        inPath: String(p.in || '').toLowerCase() === 'path',
      }));

      if (functions.some((f) => f.name === name)) {
        name = `${name}${method}`;
      }

      functions.push({
        name,
        description: operation.summary || operation.description || name,
        method: method.toUpperCase(),
        responseType: getLanguageType(respSchema, doc),
        responseRefType: getRootRef(respSchema),
        responseReferencedTypes: collectSchemaRefNames(respSchema),
        requestType: getLanguageType(reqSchema, doc),
        requestRefType: getRootRef(reqSchema),
        requestReferencedTypes: collectSchemaRefNames(reqSchema),
        params,
        path: routePath,
        tag,
      });
    }
  }

  return functions;
}

function groupByTag(functions) {
  const map = new Map();
  for (const fn of functions) {
    const tag = fn.tag || 'default';
    if (!map.has(tag)) map.set(tag, []);
    map.get(tag).push(fn);
  }
  return map;
}

function generateModel(meta) {
  if (meta.isEnum) {
    const lines = [];
    if (meta.comment) {
      lines.push('/**');
      lines.push(` * ${meta.comment}`);
      lines.push(' */');
    }
    lines.push(`export enum ${formatSchemaKey(meta.name)} {`);
    for (const p of meta.propertyInfos) {
      lines.push(`  /** ${p.commentSummary || p.name} */`);
      lines.push(`  ${p.name} = ${p.defaultValue},`);
    }
    lines.push('}');
    return `${lines.join('\n')}\n`;
  }

  const importLines = [];
  const imports = new Map();

  for (const p of meta.propertyInfos) {
    if (/^T\d*(\[\])?$/.test(String(p.type || ''))) {
      continue;
    }

    const references = Array.isArray(p.referencedTypes) && p.referencedTypes.length > 0
      ? p.referencedTypes
      : (p.navigationName ? [p.navigationName] : []);

    for (const reference of references) {
      if (!reference || reference === meta.fullName) continue;
      const ref = formatSchemaKey(reference);
      const ns = toHyphen(getNamespaceFirstPart(getNamespace(reference)));
      const importPath = ns ? `../${ns}/${toHyphen(ref)}.model` : `../${toHyphen(ref)}.model`;
      imports.set(ref, `import { ${ref} } from '${importPath}';`);
    }
  }

  for (const line of imports.values()) importLines.push(line);
  if (importLines.length > 0) importLines.push('');

  const lines = [...importLines];
  if (meta.comment) {
    lines.push('/**');
    lines.push(` * ${meta.comment}`);
    lines.push(' */');
  }
  const genericSuffix = meta.genericParams?.length
    ? `<${meta.genericParams.map((_, i) => `T${i + 1}`).join(',')}>`
    : '';
  lines.push(`export interface ${formatSchemaKey(meta.name)}${genericSuffix} {`);
  for (const p of meta.propertyInfos) {
    const propKey = isValidTsIdentifier(p.name) ? p.name : `'${escapeSingle(p.name)}'`;
    lines.push(`  /** ${p.commentSummary || p.name} */`);
    lines.push(`  ${propKey}${p.isNullable ? '?' : ''}: ${p.type || 'any'};`);
  }
  lines.push('}');
  return `${lines.join('\n')}\n`;
}

function generateAngularService(tag, functions, schemaMetas, doc) {
  const className = `${tag}Service`;
  const imports = getServiceModelImports(functions, schemaMetas);
  const lines = [
    "import { BaseService } from '../base.service';",
    "import { Injectable } from '@angular/core';",
    "import { Observable } from 'rxjs';",
    ...imports,
    '/**',
    ` * ${getTagDescription(doc, tag)}`,
    ' */',
    "@Injectable({ providedIn: 'root' })",
    `export class ${className} extends BaseService {`,
  ];

  for (const fn of functions) {
    const b = buildFunctionCommon(fn, false);
    const resp = b.responseType || 'any';
    lines.push(`  /**`);
    lines.push(`   * ${fn.description || b.name}`);
    for (const c of b.paramComments) lines.push(`   * @param ${c.name} ${c.text}`);
    lines.push(`   */`);
    lines.push(`  ${b.name}(${b.paramsString}): Observable<${resp}> {`);
    lines.push(`    const _url = \`${b.path}\`;`);
    lines.push(`    return this.request<${resp}>('${fn.method.toLowerCase()}', _url${b.dataString});`);
    lines.push('  }');
  }

  lines.push('}');
  return `${lines.join('\n')}\n`;
}

function generateAngularClient(clientName, serviceNames, tags) {
  const lines = ["import { inject, Injectable } from '@angular/core';"];
  for (const s of serviceNames) {
    lines.push(`import { ${s}Service } from './services/${toHyphen(s)}.service';`);
  }
  lines.push('@Injectable({');
  lines.push("  providedIn: 'root'");
  lines.push('})');
  lines.push(`export class ${toPascalCase(clientName)}Client {`);
  for (const s of serviceNames) {
    const tag = tags.find((t) => t?.name === s);
    lines.push(`  /** ${tag?.description || s} */`);
    lines.push(`  public ${toCamelCase(s)} = inject(${s}Service);`);
  }
  lines.push('}');
  return `${lines.join('\n')}\n`;
}

function generateAxiosService(tag, functions, schemaMetas) {
  const imports = getServiceModelImports(functions, schemaMetas);
  const lines = [
    "import { BaseService, ExtOptions } from '../base.service';",
    ...imports,
    `class ${tag}Service extends BaseService {`,
    '  constructor() {',
    '    super();',
    '  }',
  ];

  for (const fn of functions) {
    const b = buildFunctionCommon(fn, true);
    const resp = b.responseType || 'any';
    lines.push('');
    lines.push('  /**');
    lines.push(`   * ${fn.description || b.name}`);
    for (const c of b.paramComments) lines.push(`   * @param ${c.name} ${c.text}`);
    lines.push('   */');
    lines.push(`  ${b.name}(${b.paramsString}): Promise<${resp}> {`);
    lines.push(`    const _url = \`${b.path}\`;`);
    lines.push(`    return this.request<${resp}>('${fn.method.toLowerCase()}', _url${b.dataString});`);
    lines.push('  }');
  }

  lines.push('}');
  lines.push('');
  lines.push(`export default new ${tag}Service();`);
  return `${lines.join('\n')}\n`;
}

function getServiceModelImports(functions, schemaMetas) {
  const metaMap = new Map(schemaMetas.map((m) => [m.fullName, m]));
  const refs = new Set();

  for (const fn of functions) {
    if (fn.requestRefType) refs.add(fn.requestRefType);
    if (fn.responseRefType) refs.add(fn.responseRefType);
    for (const ref of fn.requestReferencedTypes || []) refs.add(ref);
    for (const ref of fn.responseReferencedTypes || []) refs.add(ref);

    for (const arg of extractGenericArgumentTypeNames(fn.requestRefType)) refs.add(arg);
    for (const arg of extractGenericArgumentTypeNames(fn.responseRefType)) refs.add(arg);

    for (const p of fn.params || []) {
      if (p.refType) refs.add(p.refType);
      for (const ref of p.referencedTypes || []) refs.add(ref);
    }
  }

  const lines = [];
  for (const ref of refs) {
    const meta = metaMap.get(ref);
    if (!meta) continue;
    const dir = toHyphen(getNamespaceFirstPart(meta.namespace));
    const importPath = dir
      ? `../models/${dir}/${toHyphen(meta.name)}.model`
      : `../models/${toHyphen(meta.name)}.model`;
    lines.push(`import { ${meta.name} } from '${importPath}';`);
  }
  return lines;
}

function buildFunctionCommon(fn, addExtOptions) {
  let name = String(fn.name || '').replace(`${fn.tag || ''}_`, '');
  name = toCamelCase(name);

  const params = Array.isArray(fn.params) ? [...fn.params] : [];
  const ordered = params.sort((a, b) => Number(Boolean(b.isRequired)) - Number(Boolean(a.isRequired)));

  const paramDecls = ordered.map((p) => {
    const type = normalizeParamType(p.type || 'any', p.isRequired);
    const paramName = p.name || p.originalName || 'value';
    return `${paramName}: ${type}`;
  });

  const paramComments = ordered.map((p) => ({ name: p.name || p.originalName || 'value', text: p.description || p.type || 'any' }));

  let dataString = '';
  let paramsString = paramDecls.join(', ');
  const requestType = replaceGenericPlaceholders(fn.requestType || '', fn);

  if (requestType) {
    paramsString = paramsString ? `${paramsString}, data: ${requestType}` : `data: ${requestType}`;
    dataString = ', data';
    paramComments.push({ name: 'data', text: requestType });
  }

  if (addExtOptions) {
    paramsString = paramsString ? `${paramsString}, extOptions?: ExtOptions` : 'extOptions?: ExtOptions';
  }

  let routePath = fn.path || '';
  const pathParams = ordered.filter((p) => p.inPath);
  for (const p of pathParams) {
    const originalName = p.originalName || p.name || '';
    const paramName = p.name || originalName;
    routePath = routePath.replace(`{${originalName}}`, ` ${paramName} `);
  }

  const queryParams = ordered.filter((p) => !p.inPath && p.type !== 'FormData');
  if (queryParams.length > 0) {
    routePath += `?${queryParams.map((p) => {
      const originalName = p.originalName || p.name || '';
      const paramName = p.name || originalName;
      return `${originalName}= ${paramName} ?? '' `;
    }).join('&')}`;
  }

  routePath = routePath.replace(/\u0000([^\u0000]+)\u0000/g, '${$1}');

  const fileParam = ordered.find((p) => p.type === 'FormData');
  if (fileParam) {
    dataString = `, ${fileParam.name || fileParam.originalName || 'data'}`;
  }

  if (addExtOptions) {
    dataString = dataString ? `${dataString}, extOptions` : ', null, extOptions';
  }

  return {
    name,
    paramsString,
    dataString,
    path: routePath,
    responseType: replaceGenericPlaceholders(fn.responseType || 'any', fn),
    paramComments,
  };
}

function replaceGenericPlaceholders(text, fn) {
  if (!text) return text;

  let result = text;
  const replacements = new Map();

  extractGenericConcrete(fn.requestRefType, fn.requestType);
  extractGenericConcrete(fn.responseRefType, fn.responseType);

  for (const [from, to] of replacements.entries()) {
    const escaped = from.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    result = result.replace(new RegExp(`\\b${escaped}\\b`, 'g'), to);
  }

  return result;

  function extractGenericConcrete(refType, tsType) {
    if (!refType || !tsType) return;

    const lt = tsType.indexOf('<');
    const gt = tsType.lastIndexOf('>');
    if (lt <= 0 || gt <= lt) return;

    const placeholders = tsType
      .slice(lt + 1, gt)
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean);
    if (placeholders.length === 0 || !placeholders.every((p) => /^T\d*$/.test(p))) {
      return;
    }

    const concrete = extractGenericArgumentTypeNames(refType).map(formatSchemaKey);
    for (let i = 0; i < placeholders.length && i < concrete.length; i += 1) {
      replacements.set(placeholders[i], concrete[i]);
    }
  }
}

function normalizeParamType(type, isRequired) {
  const normalized = String(type || 'any').trim();
  if (isRequired) {
    return normalized;
  }

  if (/\|\s*null\b/.test(normalized)) {
    return normalized;
  }
  return `${normalized} | null`;
}

function getClientName(doc) {
  const title = doc?.info?.title || '';
  if (!title) return 'api';

  const parts = String(title).split('|').map((s) => s.trim()).filter(Boolean);
  const servicePart = parts.find((s) => /Service$/i.test(s)) || parts[0] || 'api';
  return toHyphen(servicePart.replace(/Service$/i, '')) || 'api';
}

function getTagDescription(doc, tagName) {
  const hit = (doc?.tags || []).find((t) => t?.name === tagName);
  return hit?.description || tagName;
}

function getLanguageType(schema, doc) {
  if (!schema) return '';

  if (schema.$ref) {
    const refName = getRefName(schema.$ref);
    if (refName.includes('`')) {
      return parseGenericFullName(refName);
    }
    return formatSchemaKey(refName);
  }

  if (Array.isArray(schema.oneOf) && schema.oneOf.length > 0) {
    const first = schema.oneOf[0];
    return getLanguageType(first, doc);
  }

  let tsType = 'any';
  const type = normalizeType(schema);

  if (type === 'array') {
    tsType = `${getLanguageType(schema.items, doc) || 'any'}[]`;
  } else if (type === 'object' && schema.additionalProperties) {
    tsType = `Record<string, ${getLanguageType(schema.additionalProperties, doc) || 'any'}>`;
  } else if (type === 'string') {
    if (schema.format === 'date-time' || schema.format === 'date') tsType = 'Date';
    else if (schema.format === 'binary') tsType = 'FormData';
    else tsType = 'string';
  } else if (type === 'integer' || type === 'number') {
    tsType = 'number';
  } else if (type === 'boolean') {
    tsType = 'boolean';
  }

  if (isNullableSchema(schema) && !tsType.includes('| null')) {
    tsType += ' | null';
  }

  return tsType;
}

function normalizeType(schema) {
  if (!schema) return 'object';
  if (typeof schema.type === 'string') return schema.type;
  if (Array.isArray(schema.type)) {
    const t = schema.type.find((x) => x !== 'null');
    return t || 'object';
  }
  if (schema.properties) return 'object';
  return 'object';
}

function resolveSchema(schema, doc) {
  if (!schema?.$ref) return schema;
  const name = getRefName(schema.$ref);
  return doc?.components?.schemas?.[name] || schema;
}

function getRootRef(schema) {
  return collectSchemaRefNames(schema)[0] || null;
}

function collectSchemaRefNames(schema, visited = new Set()) {
  const refs = [];
  visit(schema);
  return refs;

  function visit(current) {
    if (!current || typeof current !== 'object' || visited.has(current)) {
      return;
    }

    visited.add(current);

    if (current.$ref) {
      refs.push(getRefName(current.$ref));
      return;
    }

    if (current.items) visit(current.items);
    if (current.additionalProperties && typeof current.additionalProperties === 'object') {
      visit(current.additionalProperties);
    }

    if (current.properties && typeof current.properties === 'object') {
      for (const property of Object.values(current.properties)) {
        visit(property);
      }
    }

    for (const groupName of ['allOf', 'oneOf', 'anyOf']) {
      if (Array.isArray(current[groupName])) {
        for (const item of current[groupName]) {
          visit(item);
        }
      }
    }
  }
}

function isEnumSchema(schema) {
  return Array.isArray(schema?.enum) && schema.enum.length > 0 || Array.isArray(schema?.['x-enumData']);
}

function isArraySchema(schema) {
  return normalizeType(schema) === 'array';
}

function isNullableSchema(schema) {
  return Boolean(schema?.nullable) || (Array.isArray(schema?.type) && schema.type.includes('null'));
}

function getEnumData(schema) {
  if (Array.isArray(schema?.['x-enumData'])) {
    return schema['x-enumData']
      .filter((x) => x && typeof x === 'object')
      .map((x) => ({
        name: String(x.name || x.value || 'Unknown'),
        value: Number(x.value ?? 0),
        description: x.description ? String(x.description) : '',
      }));
  }

  if (Array.isArray(schema?.enum)) {
    return schema.enum.map((v, i) => ({
      name: `Value${i}`,
      value: Number(v),
      description: String(v),
    }));
  }

  return [];
}

function formatSchemaKey(name) {
  if (!name) return '';
  let s = String(name);
  const tick = s.indexOf('`');
  if (tick > 0) s = s.slice(0, tick);
  const dot = s.lastIndexOf('.');
  if (dot >= 0) s = s.slice(dot + 1);
  const plus = s.indexOf('+');
  if (plus >= 0) s = s.slice(plus + 1);
  return s;
}

function getNamespace(fullName) {
  if (!fullName) return '';
  let s = String(fullName);
  const tick = s.indexOf('`');
  if (tick > 0) s = s.slice(0, tick);
  const dot = s.lastIndexOf('.');
  return dot > 0 ? s.slice(0, dot) : '';
}

function getNamespaceFirstPart(ns) {
  if (!ns) return '';
  return String(ns).split('.').filter(Boolean)[0] || ns;
}

function getRefName(ref) {
  return String(ref).split('/').at(-1) || ref;
}

function parseGenericParams(fullName) {
  const types = extractAllTypeNames(fullName).slice(1).map(formatSchemaKey);
  return types;
}

function extractGenericArgumentTypeNames(typeName) {
  if (!typeName || !String(typeName).includes('`')) return [];
  return extractAllTypeNames(typeName).slice(1);
}

function extractAllTypeNames(typeName) {
  if (!typeName) return [];
  const value = String(typeName);
  const tick = value.indexOf('`');
  if (tick <= 0) return [value.trim()];

  const result = [value.split('[')[0].trim()];
  const start = value.indexOf('[[');
  const end = value.lastIndexOf(']]');
  if (start > 0 && end > start) {
    const inner = value.slice(start + 2, end);
    const args = inner.split('],[').map((s) => s.trim()).filter(Boolean);
    for (const arg of args) {
      const argType = arg.split(',')[0].trim();
      result.push(...extractAllTypeNames(argType));
    }
  }
  return result;
}

function parseGenericFullName(fullName) {
  if (!fullName) return fullName;
  const match = /([^\.]+)`(\d+)/.exec(String(fullName));
  if (!match) return formatSchemaKey(fullName);

  const typeName = match[1];
  const genericCount = Number.parseInt(match[2], 10);
  const placeholders = Array.from({ length: genericCount }, (_, i) => (genericCount === 1 ? 'T' : `T${i + 1}`));
  return `${typeName}<${placeholders.join(',')}>`;
}

function toHyphen(value, separator = '-') {
  if (!value) return '';
  const str = String(value);
  let result = '';
  let upperNumber = 0;
  for (let i = 0; i < str.length; i++) {
    const ch = str[i];
    const pre = i >= 1 ? str[i - 1] : 'a';
    if (/[A-Z]/.test(ch) && /[a-z]/.test(pre)) {
      upperNumber += 1;
      if (upperNumber > 1) result += separator;
    } else if (ch === '_' || ch === ' ') {
      result += separator;
      continue;
    }
    result += ch.toLowerCase();
  }
  return result;
}

function toPascalCase(value) {
  if (!value) return '';
  return String(value)
    .replace(/[^a-zA-Z0-9]+/g, ' ')
    .split(' ')
    .filter(Boolean)
    .map((x) => x.charAt(0).toUpperCase() + x.slice(1))
    .join('');
}

function toCamelCase(value) {
  const p = toPascalCase(value);
  if (!p) return '';
  return p.charAt(0).toLowerCase() + p.slice(1);
}

function isValidTsIdentifier(value) {
  if (!value) return false;
  if (!/^[A-Za-z_$]/.test(value)) return false;
  return /^[A-Za-z0-9_$]+$/.test(value);
}

function normalizeParameterName(value, usedNames = new Set()) {
  let name = toCamelCase(value);
  if (!name) {
    name = 'value';
  }

  if (!/^[A-Za-z_$]/.test(name)) {
    name = `arg${name.charAt(0).toUpperCase()}${name.slice(1)}`;
  }

  if (JS_RESERVED_WORDS.has(name)) {
    name = `${name}Value`;
  }

  let candidate = name;
  let index = 2;
  while (usedNames.has(candidate)) {
    candidate = `${name}${index}`;
    index += 1;
  }
  usedNames.add(candidate);
  return candidate;
}

const JS_RESERVED_WORDS = new Set([
  'await', 'break', 'case', 'catch', 'class', 'const', 'continue', 'debugger', 'default', 'delete',
  'do', 'else', 'enum', 'export', 'extends', 'false', 'finally', 'for', 'function', 'if', 'import',
  'in', 'instanceof', 'new', 'null', 'return', 'super', 'switch', 'this', 'throw', 'true', 'try',
  'typeof', 'var', 'void', 'while', 'with', 'yield', 'let', 'static', 'implements', 'interface',
  'package', 'private', 'protected', 'public'
]);

function stripArraySuffix(typeName) {
  const value = String(typeName || '');
  return value.endsWith('[]') ? value.slice(0, -2) : value;
}

function isHttpMethod(v) {
  return ['get', 'post', 'put', 'patch', 'delete', 'head', 'options'].includes(String(v).toLowerCase());
}

function escapeSingle(v) {
  return String(v).replaceAll("'", "\\'");
}

async function writeFile(filePath, content) {
  await fsp.mkdir(path.dirname(filePath), { recursive: true });
  await fsp.writeFile(filePath, content, 'utf8');
}

async function cleanupGeneratedDirs(clientRootDir) {
  const modelsDir = path.join(clientRootDir, 'models');
  const servicesDir = path.join(clientRootDir, 'services');

  await fsp.rm(modelsDir, { recursive: true, force: true });
  await fsp.rm(servicesDir, { recursive: true, force: true });
}

async function fileExists(filePath) {
  try {
    await fsp.access(filePath);
    return true;
  } catch {
    return false;
  }
}

async function loadOpenApiDocument(pathOrUrl) {
  try {
    return await SwaggerParser.parse(pathOrUrl);
  } catch (error) {
    if (!isHttpUrl(pathOrUrl)) {
      throw error;
    }

    const raw = await downloadText(pathOrUrl);
    const json = JSON.parse(raw);
    return SwaggerParser.parse(json);
  }
}

function isHttpUrl(value) {
  return /^https?:\/\//i.test(String(value || ''));
}

function downloadText(url) {
  return new Promise((resolve, reject) => {
    const lib = url.startsWith('https://') ? https : http;
    const req = lib.get(url, {
      rejectUnauthorized: false,
      headers: {
        Accept: 'application/json',
      },
    }, (res) => {
      if (!res || (res.statusCode && res.statusCode >= 400)) {
        reject(new Error(`Failed to fetch ${url}, status: ${res?.statusCode}`));
        return;
      }

      const chunks = [];
      res.on('data', (chunk) => chunks.push(chunk));
      res.on('end', () => resolve(Buffer.concat(chunks).toString('utf8')));
    });

    req.on('error', reject);
  });
}

module.exports = {
  generateRequestClient,
  getSupportedRequestTypes,
};
