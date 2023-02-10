using CMSService.GrpcService;
using Grpc.Core;
using static  CMSService.GrpcService.${EntityName};

namespace CMSService.Services;

public class ${EntityName}Service : ${EntityName}Base
{
    public override Task<${EntityName}Reply> Add(AddRequest request, ServerCallContext context)
    {
        return base.Add(request, context);
    }

    public override Task<${EntityName}Reply> Delete(IdRequest request, ServerCallContext context)
    {
        return base.Delete(request, context);
    }

    public override Task<PageReply> Filter(FilterRequest request, ServerCallContext context)
    {
        return base.Filter(request, context);
    }

    public override Task<${EntityName}Reply> Update(UpdateRequest request, ServerCallContext context)
    {
        return base.Update(request, context);
    }

    public override Task<${EntityName}Reply> Detail(IdRequest request, ServerCallContext context)
    {
        return base.Detail(request, context);
    }
}
