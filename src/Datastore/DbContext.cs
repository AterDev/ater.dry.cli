﻿using Core.Entities;
using Core.Infrastructure.Helper;
using LiteDB;

namespace Datastore;
public class DbContext : IDisposable
{
    private static LiteDatabase? LiteDb { get; set; }
    public ILiteCollection<Project> Projects { get; set; }
    public ILiteCollection<EntityInfo> EntityInfos { get; set; }
    public ILiteCollection<ApiDocInfo> ApiDocInfos { get; set; }
    public ILiteCollection<TemplateFile> TemplateFile { get; set; }
    public ILiteCollection<ConfigData> Configs { get; set; }

    public DbContext()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localDir = AssemblyHelper.GetStudioPath();
        var connectionString = $"Filename={Path.Combine(localDir, "dry.db")};Upgrade=true;initialSize=5MB";

        LiteDb ??= new LiteDatabase(connectionString);
        LiteDb.Mapper.EmptyStringToNull = false;
        Projects = LiteDb.GetCollection<Project>();
        EntityInfos = LiteDb.GetCollection<EntityInfo>();
        ApiDocInfos = LiteDb.GetCollection<ApiDocInfo>();
        TemplateFile = LiteDb.GetCollection<TemplateFile>();
        Configs = LiteDb.GetCollection<ConfigData>();
    }

    public void Dispose()
    {
        LiteDb?.Dispose();
    }
}
