using PropertyInfo = CodeGenerator.Models.PropertyInfo;

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
        var tplContent = GetTplContent("angular.add.add.component.ts");
        // 替换名称
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen());
        // 解析属性，并生成相应代码
        var typeHelper = new EntityParseHelper(Path.Combine(DtoPath, "models", DtoDirName, EntityName + "UpdateDto.cs"));
        typeHelper.Parse();
        var props = typeHelper.PropertyInfos?
            .Where(p => !p.IsNavigation && !p.Type.StartsWith("Guid"))
            .ToList();

        var definedProperties = "";
        var definedFormControls = "";
        var definedValidatorMessage = "";
        if (props != null)
            GetFormControlAndValidate(props, false, ref definedProperties, ref definedFormControls, ref definedValidatorMessage);

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
        var formGen = new NgFormGenerate();
        var htmlContent = NgFormGenerate.GenerateAddForm(props);
        var cssContent = GetTplContent("angular.add.add.component.css.tpl");

        var component = new NgComponentInfo("add")
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
        var tplContent = GetTplContent("angular.edit.edit.component.ts");
        // 替换名称
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen());
        // 解析属性，并生成相应代码
        var typeHelper = new EntityParseHelper(Path.Combine(DtoPath, "models", DtoDirName, EntityName + "UpdateDto.cs"));
        typeHelper.Parse();
        var props = typeHelper.PropertyInfos?
            .Where(p => !p.IsNavigation && !p.Type.StartsWith("Guid"))
            .ToList();

        var definedProperties = "";
        var definedFormControls = "";
        var definedValidatorMessage = "";
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
        var formGen = new NgFormGenerate();
        var htmlContent = NgFormGenerate.GenerateEditForm(props);
        var cssContent = GetTplContent("angular.edit.edit.component.css.tpl");

        var component = new NgComponentInfo("edit")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;
    }

    public NgComponentInfo BuildIndexPage()
    {
        var cssContent = GetTplContent("angular.index.index.component.css.tpl");
        var typeHelper = new EntityParseHelper(Path.Combine(DtoPath, "models", DtoDirName, EntityName + "ItemDto.cs"));
        typeHelper.Parse();
        // 需要展示的列
        var columns = typeHelper.PropertyInfos?.Where(p => !p.IsList && !p.IsNavigation)
            .Select(p => p.Name)
            .Skip(0).Take(3)
            .ToList();

        var columnsDef = Array.Empty<string>();
        if (columns != null && columns.Any())
        {
            columnsDef = columns.Select(s =>
            {
                var type = typeHelper.PropertyInfos?
                    .Where(p => p.Name.Equals(s))
                    .Select(p => p.Type)
                    .FirstOrDefault();
                var pipe = "";
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

        var htmlContent = GetTplContent("angular.index.index.component.html.tpl");
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

        var tplContent = GetTplContent("angular.index.index.component.ts");
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen())
            .Replace("{$Columns}", string.Join(", ", columnsDef));

        var component = new NgComponentInfo("index")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;
    }

    public NgComponentInfo BuildDetailPage()
    {
        var cssContent = GetTplContent("angular.detail.detail.component.css.tpl");
        var typeHelper = new EntityParseHelper(Path.Combine(DtoPath, "models", DtoDirName, EntityName + "ShortDto.cs"));
        typeHelper.Parse();
        var props = typeHelper.PropertyInfos?.Where(p => !p.IsList && !p.IsNavigation)?.ToList();

        // html
        var htmlContent = GetTplContent("angular.detail.detail.component.html.tpl");
        var content = Array.Empty<string>();
        if (props != null)
        {
            content = props.Select(p =>
            {
                return $@"      <p>{p.Name}: <span class=""text-primary"">{{{{data.{p.Name.ToCamelCase()}}}}}</span></p>
";
            }).ToArray();
        }
        htmlContent = htmlContent.Replace("{$Content}", string.Join("", content));

        // ts
        var tplContent = GetTplContent("angular.detail.detail.component.ts");
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen());

        var component = new NgComponentInfo("detail")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;
    }

    public NgComponentInfo BuildLayout()
    {
        var cssContent = GetTplContent("angular.layout.layout.component.css.tpl");
        var htmlContent = GetTplContent("angular.layout.layout.component.html.tpl");
        var tplContent = GetTplContent("angular.layout.layout.component.ts");
        tplContent = tplContent.Replace("{$ModulePathName}", EntityName.ToHyphen());

        var component = new NgComponentInfo("layout")
        {
            HtmlContent = htmlContent,
            TsContent = tplContent,
            CssContent = cssContent,
        };
        return component;

    }

    public static NgComponentInfo BuildConfirmDialog()
    {
        var cssContent = GetTplContent("angular.confirmDialog.confirm-dialog.component.css.tpl");
        var htmlContent = GetTplContent("angular.confirmDialog.confirm-dialog.component.html.tpl");
        var tplContent = GetTplContent("angular.confirmDialog.confirm-dialog.component.ts");

        var component = new NgComponentInfo("confirm-dialog")
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
    public string GetModule()
    {
        var tplContent = GetTplContent("angular.module.ts");
        var pathName = EntityName.ToHyphen();
        tplContent = tplContent.Replace("{$ModuleName}", EntityName)
            .Replace("{$ModulePathName}", pathName);
        return tplContent;

    }
    public string GetRoutingModule()
    {
        var pathName = EntityName.ToHyphen();
        var tplContent = GetTplContent("angular.routing.module.ts");
        tplContent = tplContent.Replace("{$ModuleName}", EntityName)
            .Replace("{$ModulePathName}", pathName);
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
        var importStrings = "";
        var declareStrings = "";
        foreach (var item in props)
        {
            importStrings += @$"import {{ {item.Name} }} from 'src/app/share/models/enum/{item.Name.ToHyphen()}.model';" + Environment.NewLine;
            declareStrings += @$"{item.Name} = {item.Name};" + Environment.NewLine;
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
        return tsContent.Replace("[@Imports]", @"import * as ClassicEditor from 'ng-ckeditor5-classic';
import { environment } from 'src/environments/environment';
import { CKEditor5 } from '@ckeditor/ckeditor5-angular';
// import { OidcSecurityService } from 'angular-auth-oidc-client';
[@Imports]")
            .Replace("[@Declares]", @"public editorConfig!: CKEditor5.Config;
  public editor: CKEditor5.EditorConstructor = ClassicEditor;
  [@Declares]")
            .Replace("[@DI]", @"
    // private authService: OidcSecurityService,")
            .Replace("[@Methods]", @"  initEditor(): void {
    this.editorConfig = {
      // placeholder: '请添加图文信息提供证据，也可以直接从Word文档中复制',
      simpleUpload: {
        uploadUrl: environment.uploadEditorFileUrl,
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(""accessToken"")
        }
      },
      language: 'zh-cn'
    };
  }
  onReady(editor: any) {
    editor.ui.getEditableElement().parentElement.insertBefore(
      editor.ui.view.toolbar.element,
      editor.ui.getEditableElement()
    );
  }").Replace("[@Init]", "this.initEditor();");
    }


    /// <summary>
    /// 清除模板中的点位符
    /// </summary>
    /// <returns></returns>
    private string CleanTsTplVariables(string tplContent)
    {
        foreach (var item in TplVariables)
        {
            tplContent = tplContent.Replace(item, "");
        }
        return tplContent;
    }
    private static void GetFormControlAndValidate(List<PropertyInfo> props, bool isEdit, ref string definedProperties, ref string definedFormControls, ref string definedValidatorMessage)
    {
        foreach (var property in props)
        {
            var name = property.Name.ToCamelCase();
            definedProperties += $@"    get {name}() {{ return this.formGroup.get('{name}'); }}
";
            var validators = new List<string>();
            if (property.IsRequired) validators.Add("Validators.required");
            if (property.MinLength != null) validators.Add($"Validators.minLength({property.MinLength})");
            if (property.MaxLength != null) validators.Add($"Validators.maxLength({property.MaxLength})");
            var defaultValue = isEdit? $"this.data.{name}":"null";
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
