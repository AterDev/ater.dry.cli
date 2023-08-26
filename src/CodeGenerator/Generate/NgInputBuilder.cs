namespace CodeGenerator.Generate;

/// <summary>
/// angular material 表单生成
/// </summary>
public class NgInputBuilder
{
    public string Type { get; }
    public string Name { get; }
    public string? Label { get; set; }
    public bool IsRequired { get; set; } = false;
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public bool IsDecimal { get; set; } = false;
    public bool IsList { get; set; } = false;
    public bool IsEnum { get; set; } = false;

    public NgInputBuilder(string type, string name, string? label)
    {
        Type = type;
        Name = name;
        Label = label ?? name ?? type;
    }

    public string ToFormControl()
    {
        // 过滤常规字段
        string[] filterNames = new string[] { "createdtime", "updatedtime", "createtime", "updatetime" };
        if (filterNames.Contains(Name.ToLower()))
        {
            return string.Empty;
        }
        string str = Type switch
        {
            "string" => BuildInputText(),
            "DateTimeOffset" or "DateTime" => BuildInputDate(),
            "short" or "int" or "decimal" or "double" or "float" or "uint" or "ushort" => BuildInputNumber(),
            "bool" => BuildSlide(),
            _ => BuildInputText(),
        };
        if (IsEnum)
        {
            str = BuildSelect();
        }

        return str;
    }

    public string BuildInputText()
    {
        string name = Name.ToCamelCase();
        string html = "";
        if (MaxLength <= 200)
        {
            html = $@"  <mat-form-field>
    <mat-label>{Label}</mat-label>
    <input matInput placeholder=""{Label},不超过{MaxLength}字"" formControlName=""{name}"" {(IsRequired ? "required" : "")} minlength=""{MinLength}"" maxlength=""{MaxLength}"">
    <mat-error *ngIf=""{name}?.invalid"">
    {{{{getValidatorMessage('{name}')}}}}
    </mat-error>
  </mat-form-field>
";
        }
        else if (MaxLength <= 1000)
        {
            html = $@"  <mat-form-field>
    <mat-label>{Label}</mat-label>
    <textarea matInput placeholder=""{Label},不超过{MaxLength}字"" formControlName=""{name}"" {(IsRequired ? "required" : "")} minlength=""{MinLength}"" maxlength=""{MaxLength}""
      cols=""5""></textarea>
    <mat-error *ngIf=""{name}?.invalid"">
    {{{{getValidatorMessage('{name}')}}}}
    </mat-error>
  </mat-form-field>
";
        }
        else if (MaxLength > 1000 || MinLength >= 100)
        {
            html = $@" <textarea formControlName=""{name}"" rows=""10"" ></textarea>
";
        }

        return html;
    }

    public string BuildInputNumber()
    {
        string step = "1", min = "0";
        if (IsDecimal)
        {
            step = "0.01";
        }

        string name = Name.ToCamelCase();
        string html = $@"  <mat-form-field>
    <mat-label>{Label}</mat-label>
    <input matInput type=""number"" placeholder=""{Label}"" formControlName=""{name}"" {(IsRequired ? "required" : "")} step=""{step}"" min=""{min}"">
    <mat-error *ngIf=""{name}?.invalid"">
    {{{{getValidatorMessage('{name}')}}}}
    </mat-error>
  </mat-form-field>
"
;
        return html;
    }
    public string BuildInputDate()
    {
        string name = Name.ToCamelCase();
        string html = $@"  <mat-form-field>
    <mat-label>{Label}</mat-label>
    <input matInput [matDatepicker]=""{name}Picker"" placeholder=""{Label}"" formControlName=""{name}"">
    <mat-datepicker-toggle matSuffix [for]=""{name}Picker""></mat-datepicker-toggle>
    <mat-datepicker #{name}Picker startView=""multi-year""></mat-datepicker>
    <mat-error *ngIf=""{name}?.invalid"">
    {{{{getValidatorMessage('{name}')}}}}
    </mat-error>
  </mat-form-field>
";
        return html;
    }
    public string BuildSelect()
    {
        string name = Name.ToCamelCase();
        string list = IsEnum ? Type.ToPascalCase() : name;
        string html = @$"<mat-form-field>
  <mat-label>{Label}</mat-label>
  <mat-select formControlName=""{name}"">
    <mat-option *ngFor=""let item of {list} | toKeyValue"" [value]=""item.value"">
      {{{{item.value | enumText:'{list}'}}}}
    </mat-option>
  </mat-select>
  <mat-error *ngIf=""{name}?.invalid"">
    {{{{getValidatorMessage('{name}')}}}}
  </mat-error>
</mat-form-field>
";
        return html;
    }

    public string BuildCheckbox()
    {
        string name = Name.ToCamelCase();
        string html = @$"    <mat-checkbox color=""primary"" formControlName=""{name}"">{Label}</mat-checkbox>
";
        return html;
    }
    public string BuildSlide()
    {
        string name = Name.ToCamelCase();
        string html = @$"    <mat-slide-toggle class=""my-2"" color=""primary"" formControlName=""{name}"">{Label}</mat-slide-toggle>
";
        return html;
    }
    public string BuildRadioGroup()
    {
        string name = Name.ToCamelCase();
        string html = $@"<mat-radio-group color=""primary"" aria-label=""选择{Label}"" formControlName=""{name}"">
  <mat-radio-button [value]=""{name}"">选项1</mat-radio-button>
</mat-radio-group>";
        return html;
    }

}
