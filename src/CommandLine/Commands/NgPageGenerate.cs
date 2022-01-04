using CodeGenerator.Infrastructure.Helper;

namespace Droplet.CommandLine.Commands;

/// <summary>
/// angular 根据模型生成页面
/// </summary>
public class NgPageGenerate : GenerateBase
{
    public string EntityName { get; }
    public string ServicePath { get; }
    public string Output { get; }

    public NgPageGenerate(string modelName, string servicePath, string output)
    {
        EntityName = modelName;
        ServicePath = servicePath;
        Output = Path.Combine(output, "src", "app", modelName.ToHyphen());
    }

    public void Build(bool hasLayout = true, bool hasModule = true)
    {
        try
        {
            if (hasModule) BuildModule();
            if (hasLayout) BuildLayout();
            BuildAddPage();
            BuildEditPage();
            BuildIndexPage();
            BuildDetailPage();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message + e.StackTrace);
        }

    }


    public void BuildAddPage()
    {
        // 生成.ts
        var tplContent = GetTplContent("NgViews.add.add.component.ts");
        // 替换名称
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen());
        // 解析属性，并生成相应代码
        var typeHelper = new EntityParseHelper(Path.Combine(ServicePath, "models", EntityName, EntityName + "AddDto.cs"));
        var props = typeHelper.PropertyInfos?.Where(p => !p.IsList && !p.IsReference)?.ToList();

        var definedProperties = "";
        var definedFormControls = "";
        var definedValidatorMessage = "";
        if (props != null)
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
                definedFormControls += $@"      {name}: new FormControl(null, [{string.Join(",", validators)}]),
";

                definedValidatorMessage += @$"      case '{name}':
        return this.{name}?.errors?.required ? '{property.Name}必填' :
          this.{name}?.errors?.minlength ? '{property.Name}长度最少{property.MinLength}位' :
            this.{name}?.errors?.maxlength ? '{property.Name}长度最多{property.MaxLength}位' : '';
";
            }
        }

        tplContent = tplContent.Replace("{$DefinedProperties}", definedProperties)
            .Replace("{$DefinedFormControls}", definedFormControls)
            .Replace("{$DefinedValidatorMessage}", definedValidatorMessage);
        SaveToFile(Path.Combine(Output, "add"), "add.component.ts", tplContent);
        // 生成html
        var formGen = new NgFormGenerate();
        var htmlContent = formGen.GenerateAddForm(props, EntityName);
        SaveToFile(Path.Combine(Output, "add"), "add.component.html", htmlContent);

        tplContent = GetTplContent("NgViews.add.add.component.css.tpl");
        SaveToFile(Path.Combine(Output, "add"), "add.component.css", tplContent);
        Console.WriteLine("Generate add component finished!");
    }

    public void BuildEditPage()
    {
        // 生成.ts
        var tplContent = GetTplContent("NgViews.edit.edit.component.ts");
        // 替换名称
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen());
        // 解析属性，并生成相应代码
        var typeHelper = new EntityParseHelper(Path.Combine(ServicePath, "models", EntityName, EntityName + "UpdateDto.cs"));
        var props = typeHelper.PropertyInfos?.Where(p => !p.IsList && !p.IsReference)?.ToList();

        var definedProperties = "";
        var definedFormControls = "";
        var definedValidatorMessage = "";
        if (props != null)
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
                definedFormControls += $@"      {name}: new FormControl(this.data.{name}, [{string.Join(",", validators)}]),
";

                definedValidatorMessage += @$"      case '{name}':
        return this.{name}?.errors?.required ? '{property.Name}必填' :
          this.{name}?.errors?.minlength ? '{property.Name}长度最少{property.MinLength}位' :
            this.{name}?.errors?.maxlength ? '{property.Name}长度最多{property.MaxLength}位' : '';
";
            }
        }

        tplContent = tplContent.Replace("{$DefinedProperties}", definedProperties)
            .Replace("{$DefinedFormControls}", definedFormControls)
            .Replace("{$DefinedValidatorMessage}", definedValidatorMessage);
        SaveToFile(Path.Combine(Output, "edit"), "edit.component.ts", tplContent);
        // 生成html
        var formGen = new NgFormGenerate();
        var htmlContent = formGen.GenerateEditForm(props, EntityName);
        SaveToFile(Path.Combine(Output, "edit"), "edit.component.html", htmlContent);

        tplContent = GetTplContent("NgViews.edit.edit.component.css.tpl");
        SaveToFile(Path.Combine(Output, "edit"), "edit.component.css", tplContent);

        Console.WriteLine("Generate add component finished!");
    }

    public void BuildIndexPage()
    {
        var tplContent = GetTplContent("NgViews.index.index.component.css.tpl");
        SaveToFile(Path.Combine(Output, "index"), "index.component.css", tplContent);

        var typeHelper = new EntityParseHelper(Path.Combine(ServicePath, "models", EntityName, EntityName + "Dto.cs"));
        // 需要展示的列
        var columns = typeHelper.PropertyInfos.Where(p => !p.IsList && !p.IsReference)
            .Select(p => p.Name)
            .Skip(0).Take(3)
            .ToList();

        var columnsDef = columns.Select(s =>
        {
            var type = typeHelper.PropertyInfos
                .Where(p => p.Name.Equals(s))
                .Select(p => p.Type)
                .FirstOrDefault();
            var pipe = "";
            if (type.Equals("DateTime") || type.Equals("DateTimeOffset"))
            {
                pipe = s.EndsWith("Date") ? " | date: 'yyyy-MM-dd'" : " | date: 'yyy-MM-dd HH:mm:ss'";
            }
            return $@"  <ng-container matColumnDef=""{s.ToCamelCase()}"">
    <th mat-header-cell *matHeaderCellDef>{s}</th>
    <td mat-cell *matCellDef=""let element"">
      {{{{element.{s.ToCamelCase()}{pipe}}}}}
    </td>
  </ng-container>
";
        }).ToArray();
        tplContent = GetTplContent("NgViews.index.index.component.html.tpl");
        tplContent = tplContent.Replace("{$ColumnsDef}", string.Join("", columnsDef));
        SaveToFile(Path.Combine(Output, "index"), "index.component.html", tplContent);

        // 解析属性，并生成相应ts代码
        columns.Add("actions");
        columnsDef = columns.Select(s =>
        {
            return $@"'{s.ToCamelCase()}'";
        }).ToArray();
        tplContent = GetTplContent("NgViews.index.index.component.ts");
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen())
            .Replace("{$Columns}", string.Join(",　", columnsDef));
        SaveToFile(Path.Combine(Output, "index"), "index.component.ts", tplContent);
        Console.WriteLine("Generate index component finished!");
    }

    public void BuildDetailPage()
    {
        var tplContent = GetTplContent("NgViews.detail.detail.component.css.tpl");
        SaveToFile(Path.Combine(Output, "detail"), "detail.component.css", tplContent);

        var typeHelper = new EntityParseHelper(Path.Combine(ServicePath, "models", EntityName, EntityName + "Dto.cs"));
        var props = typeHelper.PropertyInfos?.Where(p => !p.IsList && !p.IsReference)?.ToList();

        // html
        tplContent = GetTplContent("NgViews.detail.detail.component.html.tpl");
        var content = props.Select(p =>
        {
            return $@"      <p>{p.Name}: <span class=""text-primary"">{{{{data.{p.Name.ToCamelCase()}}}}}</span></p>
";

        }).ToArray();
        tplContent = tplContent.Replace("{$Content}", string.Join("", content));
        SaveToFile(Path.Combine(Output, "detail"), "detail.component.html", tplContent);

        // ts
        tplContent = GetTplContent("NgViews.detail.detail.component.ts");
        tplContent = tplContent.Replace("{$EntityName}", EntityName)
            .Replace("{$EntityPathName}", EntityName.ToHyphen());
        SaveToFile(Path.Combine(Output, "detail"), "detail.component.ts", tplContent);
        Console.WriteLine("Generate detail component finished!");
    }

    public void BuildLayout()
    {
        var tplContent = GetTplContent("NgViews.layout.layout.component.css.tpl");
        SaveToFile(Path.Combine(Output, "layout"), "layout.component.css", tplContent);
        tplContent = GetTplContent("NgViews.layout.layout.component.html.tpl");
        SaveToFile(Path.Combine(Output, "layout"), "layout.component.html", tplContent);

        tplContent = GetTplContent("NgViews.layout.layout.component.ts");
        tplContent = tplContent.Replace("{$ModulePathName}", EntityName.ToHyphen());
        SaveToFile(Path.Combine(Output, "layout"), "layout.component.ts", tplContent);

        Console.WriteLine("Generate Layout component finished!");
    }

    /// <summary>
    /// 创建模块路由
    /// </summary>
    public void BuildModule()
    {
        var tplContent = GetTplContent("NgViews.module.ts");
        var pathName = EntityName.ToHyphen();
        tplContent = tplContent.Replace("{$ModuleName}", EntityName)
            .Replace("{$ModulePathName}", pathName);
        SaveToFile(Output, pathName + ".module.ts", tplContent);

        tplContent = GetTplContent("NgViews.routing.module.ts");
        tplContent = tplContent.Replace("{$ModuleName}", EntityName)
            .Replace("{$ModulePathName}", pathName);
        SaveToFile(Output, pathName + "-routing.module.ts", tplContent);
        Console.WriteLine("Generate module finished!");
    }
}
