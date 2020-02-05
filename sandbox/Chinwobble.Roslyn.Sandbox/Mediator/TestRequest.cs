using Chinwobble.Roslyn.MediatR;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stream = System.IO.MemoryStream;

namespace Chinwobble.Roslyn.Sandbox.Mediator
{
    [Chinwobble.Roslyn.MediatR.GenerateRequestHandler]
    public class TestRequest : RequestHandler<string>?IRequest
    {
        public Task<Unit> Handle(string test, Stream stream)
        {
            global::MediatR.RequestHandler < T >
            return Task.FromResult(new Unit());
        }
    }
}
