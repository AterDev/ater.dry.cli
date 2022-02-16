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
        var filterNames = new string[] { "createdtime", "updatedtime", "createtime", "updatetime" };
        if (filterNames.Contains(Name.ToLower()))
            return string.Empty;
        var str = "";
        switch (Type)
        {
            case "string":
                str = BuildInputText();
                break;
            case "DateTimeOffset":
            case "DateTime":
                str = BuildInputDate();
                break;
            case "int":
            case "decimal":
            case "double":
            case "float":
                str = BuildInputNumber();
                break;
            case "bool":
                str = BuildSlide();
                break;
            default:
                break;
        }
        if (IsEnum)
            str = BuildSelect();
        return str;
    }

    public string BuildInputText()
    {
        var name = Name.ToCamelCase();
        var html = "";
        if (MaxLength < 200)
        {
            html = $@"  <mat-form-field>
    <mat-label>{Label}</mat-label>
    <input matInput placeholder=""{Label}"" formControlName=""{name}"" {(IsRequired ? "required" : "")} minlength=""{MinLength}"" maxlength=""{MaxLength}"">
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
    <textarea matInput placeholder=""{Label}"" formControlName=""{name}"" {(IsRequired ? "required" : "")} minlength=""{MinLength}"" maxlength=""{MaxLength}""
      cols=""5""></textarea>
    <mat-error *ngIf=""{name}?.invalid"">
    {{{{getValidatorMessage('{name}')}}}}
    </mat-error>
  </mat-form-field>
";
        }
        else if (MaxLength > 1000 || MinLength >= 100)
        {
            html = $@" <ckeditor [editor]=""editor"" [config]=""editorConfig"" formControlName=""{name}"" (ready)=""onReady($event)"">
    </ckeditor>";
        }

        return html;
    }

    public string BuildInputNumber()
    {
        string step = "1", min = "0";
        if (IsDecimal)
            step = "0.01";
        var name = Name.ToCamelCase();
        var html = $@"  <mat-form-field>
    <mat-label>{Label}</mat-label>
    <input matInput type=""number"" placeholder=""{Label}"" formControlName=""{name}"" {(IsRequired ? "required" : "")} step=""{step}"" min=""{min}"">
    <mat-error *ngIf=""{name}?.invalid"">
    {{{{getValidatorMessage('{name}')}}}}
    </mat-error>
  </mat-form-field>"
;
        return html;
    }
    public string BuildInputDate()
    {
        var name = Name.ToCamelCase();
        var html = $@"  <mat-form-field>
    <mat-label>{Label}</mat-label>
    <input matInput [matDatepicker]=""{name}Picker"" placeholder=""{Label}"" formControlName=""{name}"">
    <mat-datepicker-toggle matSuffix [for]=""{name}Picker""></mat-datepicker-toggle>
    <mat-datepicker #{name}Picker startView=""multi-year""></mat-datepicker>
    <mat-error *ngIf=""{name}?.invalid"">
    {{{{getValidatorMessage('{name}')}}}}
    </mat-error>
  </mat-form-field>";
        return html;
    }
    public string BuildSelect()
    {
        var name = Name.ToCamelCase();
        var html = @$"<mat-form-field>
  <mat-label>{Label}</mat-label>
  <mat-select formControlName=""{name}"">
    <mat-option *ngFor=""let item of {name.ToPascalCase()} | toKeyValue"" [value]=""item.value"">
      {{{{item.key}}}}
    </mat-option>
  </mat-select>
  <mat-error *ngIf=""{name}?.invalid"">
    {{{{getValidatorMessage('{name}')}}}}
  </mat-error>
</mat-form-field>";
        return html;
    }

    public string BuildCheckbox()
    {
        var name = Name.ToCamelCase();
        var html = @$"    <mat-checkbox color=""primary"" formControlName=""{name}"">{Label}</mat-checkbox>
";
        return html;
    }
    public string BuildSlide()
    {
        var name = Name.ToCamelCase();
        var html = @$"    <mat-slide-toggle color=""primary"" formControlName=""{name}"">{Label}</mat-slide-toggle>
";
        return html;
    }
    public string BuildRadioGroup()
    {
        var name = Name.ToCamelCase();
        var html = $@"<mat-radio-group color=""primary"" aria-label=""选择{Label}"" formControlName=""{name}"">
  <mat-radio-button [value]=""{name}"">选项1</mat-radio-button>
</mat-radio-group>";
        return html;
    }

}
