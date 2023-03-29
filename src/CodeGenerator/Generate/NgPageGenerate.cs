using PropertyInfo = Core.Models.PropertyInfo;

namespace CodeGenerator.Generate;

/// <summary>
/// angular 根据模型生成页面
/// </summary>
public class NgPageGenerate : GenerateBase
{
    public string EntityName { get; }
    public string DtoPath { get; }
    public string Output { get; }
    public string DtoDirName { get; set; }

    public string[] TplVariables = new string[]
    {
        "[@Imports]","[@Declares]","[@DI]","[@Init]","[@Methods]",
        "{$DefinedProperties}","{$DefinedFormControls}","{$DefinedValidatorMessage}"
    };

    public NgPageGenerate(string entityName, string dtoPath, string output)
    {
        EntityName = entityName;
        DtoPath = dtoPath;
        Output = Path.Combine(output, "src", "app", entityName.ToHyphen());
        DtoDirName = EntityName + "Dtos";
    }

    public NgComponentInfo BuildAddPage()
    {
        // 生成.ts
        string tplContent = GetTplContent("angular.add.add.component.ts");
        // 替换名称
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen());
        // 解析属性，并生成相应代码
        EntityParseHelper typeHelper = new(Path.Combine(DtoPath, "models", DtoDirName, EntityName + "AddDto.cs"));
        typeHelper.Parse();
        List<PropertyInfo>? props = typeHelper.PropertyInfos?
            .Where(p => !p.IsNavigation && !p.Type.StartsWith("Guid"))
            .ToList();

        string definedProperties = "";
        string definedFormControls = "";
        string definedValidatorMessage = "";
        if (props != null)
        {
            GetFormControlAndValidate(props, false, ref definedProperties, ref definedFormControls, ref definedValidatorMessage);
        }

        tplContent = tplContent.Replace("{$DefinedProperties}", definedProperties)
            .Replace("{$DefinedFormControls}", definedFormControls)
            .Replace("{$DefinedValidatorMessage}", definedValidatorMessage);

        // 是否需要引用富文本编辑器
        if (props != null && props.Any(p => p.MaxLength > 1000 || p.MinLength >= 100))
        {
            tplContent = InsertEditor(tplContent);
        }
        // 是否需要引用枚举类型
        if (props != null && props.Any(p => p.IsEnum))
        {
            tplContent = InsertEnum(tplContent, props.Where(p => p.IsEnum).ToList());
        }

        tplContent = CleanTsTplVariables(tplContent);

        // 生成html
        NgFormGenerate formGen = new();
        string htmlContent = NgFormGenerate.GenerateAddForm(props);
        string cssContent = GetTplContent("angular.add.add.component.css.tpl");

        NgComponentInfo component = new("add")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;
    }
    public NgComponentInfo BuildEditPage()
    {
        // 生成.ts
        string tplContent = GetTplContent("angular.edit.edit.component.ts");
        // 替换名称
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen());
        // 解析属性，并生成相应代码
        EntityParseHelper typeHelper = new(Path.Combine(DtoPath, "models", DtoDirName, EntityName + "UpdateDto.cs"));
        typeHelper.Parse();
        List<PropertyInfo>? props = typeHelper.PropertyInfos?
            .Where(p => !p.IsNavigation && !p.Type.StartsWith("Guid"))
            .ToList();

        string definedProperties = "";
        string definedFormControls = "";
        string definedValidatorMessage = "";
        if (props != null)
        {
            GetFormControlAndValidate(props, true, ref definedProperties, ref definedFormControls, ref definedValidatorMessage);
            tplContent = tplContent.Replace("{$DefinedProperties}", definedProperties)
                .Replace("{$DefinedFormControls}", definedFormControls)
                .Replace("{$DefinedValidatorMessage}", definedValidatorMessage);
        }
        // 是否需要引用富文本编辑器
        if (props != null && props.Any(p => p.MaxLength > 1000 || p.MinLength >= 100))
        {
            tplContent = InsertEditor(tplContent);
        }
        // 是否需要引用枚举类型
        if (props != null && props.Any(p => p.IsEnum))
        {
            tplContent = InsertEnum(tplContent, props.Where(p => p.IsEnum).ToList());
        }

        tplContent = CleanTsTplVariables(tplContent);

        // 生成html
        NgFormGenerate formGen = new();
        string htmlContent = NgFormGenerate.GenerateEditForm(props);
        string cssContent = GetTplContent("angular.edit.edit.component.css.tpl");

        NgComponentInfo component = new("edit")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;
    }
    public NgComponentInfo BuildIndexPage()
    {
        string cssContent = GetTplContent("angular.index.index.component.css.tpl");
        EntityParseHelper typeHelper = new(Path.Combine(DtoPath, "models", DtoDirName, EntityName + "ItemDto.cs"));
        typeHelper.Parse();
        // 需要展示的列
        List<string>? columns = typeHelper.PropertyInfos?.Where(p => !p.IsList && !p.IsNavigation)
            .Where(p => p.Name.ToLower() != "id")
            .Select(p => p.Name)
            .Skip(0).Take(5)
            .ToList();

        string[] columnsDef = Array.Empty<string>();
        if (columns != null && columns.Any())
        {
            columnsDef = columns.Select(s =>
            {
                string? type = typeHelper.PropertyInfos?
                    .Where(p => p.Name.Equals(s))
                    .Select(p => p.Type)
                    .FirstOrDefault();
                string pipe = "";
                if (type != null)
                {
                    if (type.Equals("DateTime") || type.Equals("DateTimeOffset"))
                    {
                        pipe = s.EndsWith("Date") ? " | date: 'yyyy-MM-dd'" : " | date: 'yyy-MM-dd HH:mm:ss'";
                    }
                }

                return $@"  <ng-container matColumnDef=""{s.ToCamelCase()}"">
    <th mat-header-cell *matHeaderCellDef>{s}</th>
    <td mat-cell *matCellDef=""let element"">
      {{{{element.{s.ToCamelCase()}{pipe}}}}}
    </td>
  </ng-container>
";
            }).ToArray();
        }

        string htmlContent = GetTplContent("angular.index.index.component.html.tpl");
        htmlContent = htmlContent.Replace("{$ColumnsDef}", string.Join("", columnsDef));

        // 解析属性，并生成相应ts代码
        columnsDef = Array.Empty<string>();
        if (columns != null && columns.Any())
        {
            columns.Add("actions");
            columnsDef = columns.Select(s =>
            {
                return $@"'{s.ToCamelCase()}'";
            }).ToArray();
        }

        string tplContent = GetTplContent("angular.index.index.component.ts");
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen())
            .Replace("{$Columns}", string.Join(", ", columnsDef));

        NgComponentInfo component = new("index")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;
    }
    public NgComponentInfo BuildDetailPage()
    {
        string cssContent = GetTplContent("angular.detail.detail.component.css.tpl");
        EntityParseHelper typeHelper = new(Path.Combine(DtoPath, "models", DtoDirName, EntityName + "ShortDto.cs"));
        typeHelper.Parse();
        List<PropertyInfo>? props = typeHelper.PropertyInfos?.Where(p => !p.IsList && !p.IsNavigation)?.ToList();

        // html
        string htmlContent = GetTplContent("angular.detail.detail.component.html.tpl");
        string[] content = Array.Empty<string>();
        if (props != null)
        {
            content = props.Select(p =>
            {
                // 管道处理
                string pipe = "";
                if (p.Type.ToLower().Contains("datetime"))
                {
                    pipe = "|date:'yyyy-MM-dd HH:mm:ss'";
                }
                return $@"      <p><strong>{p.Name}</strong><br> <span class=""text-primary"">{{{{data.{p.Name.ToCamelCase()}{pipe}}}}}</span></p>
";
            }).ToArray();
        }
        htmlContent = htmlContent.Replace("{$Content}", string.Join("", content));

        // ts
        string tplContent = GetTplContent("angular.detail.detail.component.ts");
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen());

        NgComponentInfo component = new("detail")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;
    }
    public NgComponentInfo BuildLayout()
    {
        string cssContent = GetTplContent("angular.layout.layout.component.css.tpl");
        string htmlContent = GetTplContent("angular.layout.layout.component.html.tpl");
        string tplContent = GetTplContent("angular.layout.layout.component.ts");
        tplContent = tplContent.Replace("{$ModulePathName}", EntityName.ToHyphen());

        NgComponentInfo component = new("layout")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;

    }
    public static NgComponentInfo BuildConfirmDialog()
    {
        string cssContent = GetTplContent("angular.confirmDialog.confirm-dialog.component.css.tpl");
        string htmlContent = GetTplContent("angular.confirmDialog.confirm-dialog.component.html.tpl");
        string tplContent = GetTplContent("angular.confirmDialog.confirm-dialog.component.ts");

        NgComponentInfo component = new("confirm-dialog")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;
    }

    /// <summary>
    /// 创建模块路由
    /// </summary>
    public string GetModule(string? route = null)
    {
        string tplContent = GetTplContent("angular.module.ts");
        string pathName = route?.ToHyphen() ?? EntityName.ToHyphen();
        tplContent = tplContent.Replace("{$ModuleName}", route ?? EntityName)
            .Replace("{$ModulePathName}", pathName);
        return tplContent;

    }

    /// <summary>
    /// 构造导航内容
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="modules"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public static string GetNavigation(string groupName, List<string> modules, List<KeyValuePair<string, string>> map)
    {
        string navListTmp = "";
        foreach (string item in modules)
        {
            navListTmp += $@"
    <mat-nav-list>
      <a mat-list-item routerLink=""/{groupName.ToHyphen()}/{item.ToHyphen()}"" routerLinkActive=""active"">
        <mat-icon>edit_note</mat-icon>
        <span *ngIf=""opened"">{map.Where(m => m.Key == item).FirstOrDefault().Value}</span>
      </a>
    </mat-nav-list>";
        }
        string temp = @$"      
  <mat-expansion-panel hideToggle>
    <mat-expansion-panel-header>
      <mat-panel-title>
        <mat-icon>view_list</mat-icon>
          <span *ngIf=""opened"">{groupName.ToHyphen()}</span>
      </mat-panel-title>
    </mat-expansion-panel-header>
{navListTmp}
  </mat-expansion-panel>
";
        return temp;
    }

    /// <summary>
    /// 路由模块
    /// </summary>
    /// <param name="modulePath">模块内容</param>
    /// <returns></returns>
    public string GetRoutingModule(string? modulePath = null, string? route = null)
    {
        string pathName = route?.ToHyphen() ?? EntityName.ToHyphen();
        string tplContent = GetTplContent("angular.routing.module.ts");
        tplContent = tplContent.Replace("{$ModuleName}", route ?? EntityName)
            .Replace("{$RoutePathName}", pathName)
            .Replace("{$ModulePathName}", modulePath ?? "");
        return tplContent;
    }

    /// <summary>
    /// 组模块内容
    /// </summary>
    /// <param name="groupName">模块名称</param>
    /// <param name="modules">子模块</param>
    /// <returns></returns>
    public static string GetGroupModule(string groupName, List<string> modules)
    {
        string tplContent = GetTplContent("angular.group.module.ts");
        string pathName = groupName.ToHyphen();
        tplContent = tplContent.Replace("{$ModuleName}", groupName)
            .Replace("{$ModulePathName}", pathName);

        // 导入的模块内容
        string importModules = string.Join(",\n    ", modules.Select(m => m + "Module").ToArray());

        string[] importString = modules.Select(s => $@"import {{ {s}Module }} from './{s.ToHyphen()}/{s.ToHyphen()}.module';")
            .ToArray();
        string importModulesPath = string.Join("\n", importString);
        tplContent = tplContent.Replace("{$ImportModulesPath}", importModulesPath)
            .Replace("{$ImportModules}", importModules);

        return tplContent;
    }

    /// <summary>
    /// 生成组模块routing内容
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public static string GetGroupRoutingModule(string groupName)
    {
        string tplContent = GetTplContent("angular.group.routing.module.ts");
        tplContent = tplContent.Replace("{$ModuleName}", groupName)
            .Replace("{$ModulePathName}", groupName.ToHyphen() ?? "");

        return tplContent;
    }

    public static string GetComponentModule()
    {
        return GetTplContent("angular.components.module.ts");
    }

    /// <summary>
    /// 插入枚举导入
    /// </summary>
    /// <param name="content"></param>
    /// <param name="props"></param>
    /// <returns></returns>
    private static string InsertEnum(string content, List<PropertyInfo> props)
    {
        string importStrings = "";
        string declareStrings = "";
        foreach (PropertyInfo item in props)
        {
            importStrings += @$"import {{ {item.Type} }} from 'src/app/share/admin/models/enum/{item.Type.ToHyphen()}.model';" + Environment.NewLine;
            declareStrings += @$"{item.Type} = {item.Type};" + Environment.NewLine;
        }
        return content.Replace("[@Imports]", importStrings + "[@Imports]")
            .Replace("[@Declares]", declareStrings + "[@Declares]");
    }

    /// <summary>
    /// 插入富文本编辑器内容
    /// </summary>
    /// <param name="tsContent"></param>
    /// <returns></returns>
    private static string InsertEditor(string tsContent)
    {
        return tsContent.Replace("[@Imports]", @"import { environment } from 'src/environments/environment';
[@Imports]")
            .Replace("[@Declares]", @"[@Declares]")
            .Replace("[@DI]", @"")
            .Replace("[@Methods]", @"  initEditor(): void {  }
").Replace("[@Init]", "this.initEditor();");
    }


    /// <summary>
    /// 清除模板中的点位符
    /// </summary>
    /// <returns></returns>
    private string CleanTsTplVariables(string tplContent)
    {
        foreach (string item in TplVariables)
        {
            tplContent = tplContent.Replace(item, "");
        }
        return tplContent;
    }
    private static void GetFormControlAndValidate(List<PropertyInfo> props, bool isEdit, ref string definedProperties, ref string definedFormControls, ref string definedValidatorMessage)
    {
        foreach (PropertyInfo property in props)
        {
            string name = property.Name.ToCamelCase();
            definedProperties += $@"    get {name}() {{ return this.formGroup.get('{name}'); }}
";
            List<string> validators = new();
            if (property.IsRequired)
            {
                validators.Add("Validators.required");
            }

            if (property.MinLength != null)
            {
                validators.Add($"Validators.minLength({property.MinLength})");
            }

            if (property.MaxLength != null)
            {
                validators.Add($"Validators.maxLength({property.MaxLength})");
            }

            string defaultValue = isEdit ? $"this.data.{name}" : "null";
            definedFormControls += $@"      {name}: new FormControl({defaultValue}, [{string.Join(",", validators)}]),
";
            definedValidatorMessage += @$"      case '{name}':
        return this.{name}?.errors?.['required'] ? '{property.Name}必填' :
          this.{name}?.errors?.['minlength'] ? '{property.Name}长度最少{property.MinLength}位' :
            this.{name}?.errors?.['maxlength'] ? '{property.Name}长度最多{property.MaxLength}位' : '';
";
        }
    }
}
