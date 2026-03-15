
using ExtremistDetector.Contracts.Models;
using Inquisitor.Infrastructure;
using Inquisitor.Models;

namespace Inquisitor.Services;

public class ViolationImageChecker : IViolationChecker<ImageContentReport>
{
    private readonly ILogger<ViolationImageChecker> _logger;
    private readonly IViolationChecker<string> _textChecker;
    private readonly IOCRProvider _ocrProvider;

    public ViolationImageChecker(ILogger<ViolationImageChecker> logger, 
        IViolationChecker<string> textChecker,
        IOCRProvider ocrProvider)
    {
        _logger = logger;
        _textChecker = textChecker;
        _ocrProvider = ocrProvider;
    }
    
    public async Task<CheckerResult> Check(ImageContentReport content, CancellationToken cancellationToken = default)
    {
        var text = await  _ocrProvider.GetTextFromUrl(content.Url, cancellationToken); 
        
        var type = await _textChecker.Check(text, cancellationToken);
        
        // TODO add Yolo or smth for symbolic check
        
        return type;
    }
}