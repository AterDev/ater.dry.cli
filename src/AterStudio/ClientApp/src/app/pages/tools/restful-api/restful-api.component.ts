import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToolsService } from 'src/app/share/services/tools.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-restful-api',
  templateUrl: './restful-api.component.html',
  styleUrl: './restful-api.component.css'
})
export class RestfulAPIComponent {
  editorOptions = { theme: 'vs-dark', language: 'csharp', minimap: { enabled: false } };
  classCode: string | null = null;
  editor: any;
  modelName: string | null = null;
  description: string | null = null;
  useDto = false;

  constructor(
    private service: ToolsService,
    private snb: MatSnackBar
  ) {
  }

  onInit(editor: any) {
    this.editor = editor;
  }

  ngOnInit(): void {

  }

  convert(): void {
    if (!this.modelName && !this.description) {
      this.snb.open('请输入模型名称和说明', '关闭', { duration: 2000 });
      return;
    }
    const addDto = this.useDto ? this.modelName + 'AddDto' : this.modelName;
    const updateDto = this.useDto ? this.modelName + 'UpdateDto' : this.modelName;
    const filterDto = this.useDto ? this.modelName + 'FilterDto' : 'FilterBase';

    this.classCode = `
/// <summary>
/// 获取${this.description}列表
/// </summary>
/// <param name="filter"></param>
/// <returns></returns>
[HttpPost]
public async Task<ActionResult<List<${this.modelName}>>> Get${this.modelName}List(${filterDto} filter)
{
    return await _manager.FilterAsync(filter);
}

/// <summary>
/// 添加${this.description}
/// </summary>
/// <param name="entity"></param>
/// <returns></returns>
[HttpPost]
public async Task<ActionResult<${this.modelName}>> Add${this.modelName}Async(${addDto} entity)
{
    return await _manager.CreateAsync(entity);
}

/// <summary>
/// 更新${this.description}
/// </summary>
/// <param name="entity"></param>
/// <returns></returns>
[HttpPut]
public async Task<ActionResult<${this.modelName}>> Update${this.modelName}Async(${updateDto} entity)
{
    return await _manager.UpdateAsync(entity);
}

/// <summary>
/// 获取${this.description}
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
[HttpGet("{id}")]
public async Task<ActionResult<${this.modelName}>> Get${this.modelName}Async(Guid id)
{
    return await _manager.FindAsync(id);
}

/// <summary>
/// 删除${this.description}
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
[HttpDelete("{id}")]
public async Task<ActionResult<${this.modelName}>> Delete${this.modelName}Async(Guid id)
{
    return await _manager.DeleteAsync(id);
}
    `
  }
}
