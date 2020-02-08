using Chinwobble.Roslyn.MediatR;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stream = System.IO.MemoryStream;

namespace Chinwobble.Roslyn.Sandbox.Mediator
{
    [Chinwobble.Roslyn.MediatR.GenerateRequestHandler]
    public class TestRequest : IRequest
    {
        public Task Handle(CancellationToken cancellationToken, string test, Stream stream)
        {
            return Task.FromResult(new Unit());
        }
    }

    [Chinwobble.Roslyn.MediatR.GenerateRequestHandler]
    public class TestRequestWithResponse : IRequest<string>
    {
        public Task<string> Handle(string test, Stream stream)
        {
            return Task.FromResult("test");
        }
    }
}
