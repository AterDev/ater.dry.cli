﻿namespace CodeGenerator.Generate;

/// <summary>
/// angular material 控件生成
/// </summary>
public class NgComponentBuilder(string type, string name, string? label)
{
    public string Type { get; } = type;
    public string Name { get; } = name;
    public string? Label { get; set; } = label ?? name ?? type;
    public bool IsRequired { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public bool IsDecimal { get; set; }
    public bool IsList { get; set; }
    public bool IsEnum { get; set; }
    /// <summary>
    /// 是否为筛选控件
    /// </summary>
    public bool IsFilter { get; set; }

    public string ToFormControl()
    {
        // 过滤常规字段
        string[] filterNames = ["CreatedTime", "UpdatedTime", "CreateTime", "UpdateTime"];
        if (filterNames.Any(fn => fn.Equals(Name, StringComparison.OrdinalIgnoreCase)))
        {
            return string.Empty;
        }
        var formControl = "";
        if (IsEnum)
        {
            formControl = BuildSelect();
        }
        else
        {
            formControl = Type switch
            {
                "string" => BuildInputText(),
                "DateTimeOffset" or "DateTime" => BuildInputDate(),
                "short" or "int" or "decimal" or "double" or "float" or "uint" or "ushort" => BuildInputNumber(),
                "bool" => BuildSlide(),
                _ => BuildInputText(),
            };
        }
        return formControl;
    }
    public string BuildInputText()
    {
        string name = Name.ToCamelCase();
        string bindValue = IsFilter ? $"[(ngModel)]=\"filter.{name}\"" : $"formControlName=\"{name}\"";
        string html = "";
        if (MaxLength <= 200)
        {
            html = $$$"""
                    <mat-form-field>
                      <mat-label>{{{Label}}}</mat-label>
                      <input matInput placeholder="{{{Label}}},不超过{{{MaxLength}}}字" {{{bindValue}}} {{{(IsRequired ? "required" : "")}}} minlength="{{{MinLength}}}" maxlength="{{{MaxLength}}}">
                      <mat-error *ngIf="{{{name}}}?.invalid">
                        {{getValidatorMessage('{{{name}}}')}}
                      </mat-error>
                    </mat-form-field>
                """;
        }
        else if (MaxLength <= 1000)
        {
            html = $$$"""
                  <mat-form-field>
                    <mat-label>{{{Label}}}</mat-label>
                    <textarea matInput placeholder="{{{Label}}},不超过{{{MaxLength}}}字" {{{bindValue}}} {{{(IsRequired ? "required" : "")}}} minlength="{{{MinLength}}}" maxlength="{{{MaxLength}}}"
                      rows="3"></textarea>
                    <mat-error *ngIf="{{{name}}}?.invalid">
                    {{getValidatorMessage('{{{name}}}')}}
                    </mat-error>
                  </mat-form-field>
                """;
        }
        else if (MaxLength > 1000 || MinLength >= 100)
        {
            html = $$$"""
                <mat-form-field>
                <mat-label>{{{Label}}}</mat-label>
                  <textarea matInput placeholder="{{{Label}}},不超过{{{MaxLength}}}字" {{{bindValue}}} {{{(IsRequired ? "required" : "")}}} minlength="{{{MinLength}}}" maxlength="{{{MaxLength}}}" rows="6" ></textarea>
                  <mat-error *ngIf="{{{name}}}?.invalid">
                    {{getValidatorMessage('{{{name}}}')}}
                  </mat-error>
                </mat-form-field>
                """;
        }
        else
        {
            html = $$$"""
                  <mat-form-field>
                    <mat-label>{{{Label}}}</mat-label>
                    <input matInput placeholder="{{{Label}}}" {{{bindValue}}} {{{(IsRequired ? "required" : "")}}}>
                    <mat-error *ngIf="{{{name}}}?.invalid">
                    {{getValidatorMessage('{{{name}}}')}}
                    </mat-error>
                  </mat-form-field>
                """;
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
        string bindValue = IsFilter ? $"[(ngModel)]=\"filter.{name}\"" : $"formControlName=\"{name}\"";

        string html = $$$"""
              <mat-form-field>
                <mat-label>{{{Label}}}</mat-label>
                <input matInput type="number" placeholder="{{{Label}}}" {{{bindValue}}} {{{(IsRequired ? "required" : "")}}} step="{{{step}}}" min="{{{min}}}">
                <mat-error *ngIf="{{{name}}}?.invalid">
                {{getValidatorMessage('{{{name}}}')}}
                </mat-error>
              </mat-form-field>

            """
;
        return html;
    }
    public string BuildInputDate()
    {
        string name = Name.ToCamelCase();
        string bindValue = IsFilter ? $"[(ngModel)]=\"filter.{name}\"" : $"formControlName=\"{name}\"";
        string html = $$$"""
              <mat-form-field>
                <mat-label>{{{Label}}}</mat-label>
                <input matInput [matDatepicker]="{{{name}}}Picker" placeholder="{{{Label}}}" {{{bindValue}}}>
                <mat-datepicker-toggle matSuffix [for]="{{{name}}}Picker"></mat-datepicker-toggle>
                <mat-datepicker #{{{name}}}Picker></mat-datepicker>
                <mat-error *ngIf="{{{name}}}?.invalid">
                {{getValidatorMessage('{{{name}}}')}}
                </mat-error>
              </mat-form-field>

            """;
        return html;
    }
    public string BuildSelect()
    {
        string name = Name.ToCamelCase();
        string bindValue = IsFilter ? $"[(ngModel)]=\"filter.{name}\"" : $"formControlName=\"{name}\"";

        string list = IsEnum ? Type.ToPascalCase() : name;
        string html = $$$"""
                <mat-form-field>
                  <mat-label>{{{Label}}}</mat-label>
                  <mat-select {{{bindValue}}}>
                    <mat-option *ngFor="let item of {{{list}}} | toKeyValue" [value]="item.value">
                      {{item.value | enumText:'{{{list}}}'}}
                    </mat-option>
                  </mat-select>
                  <mat-error *ngIf="{{{name}}}?.invalid">
                    {{getValidatorMessage('{{{name}}}')}}
                  </mat-error>
                </mat-form-field>

            """;
        return html;
    }

    public string BuildCheckbox()
    {
        string name = Name.ToCamelCase();
        string html = $"""
                <mat-checkbox color="primary" formControlName="{name}">{Label}</mat-checkbox>

            """;
        return html;
    }
    public string BuildSlide()
    {
        string name = Name.ToCamelCase();
        string html = $"""
                <mat-slide-toggle class="my-2" color="primary" formControlName="{name}">{Label}</mat-slide-toggle>
            """;
        return html;
    }
    public string BuildRadioGroup()
    {
        string name = Name.ToCamelCase();
        string html = $"""
            <mat-radio-group color="primary" aria-label="选择{Label}" formControlName="{name}">
              <mat-radio-button [value]="{name}">选项1</mat-radio-button>
            </mat-radio-group>
            """;
        return html;
    }

}
