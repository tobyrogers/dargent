using System.Threading.Tasks;

namespace Dargent.Core;

public interface IOutput
{
    public void Write(string text);
    
    public Task Stream(IAsyncEnumerable<string> stream);
    
    public bool Approve(string text);

    public Task<string> Prompt(CancellationToken cancellationToken);
}