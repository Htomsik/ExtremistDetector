
using ExtremistDetector.Contracts.Models;
using Inquisitor.Infrastructure;

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
    
    public ViolationType Check(ImageContentReport content)
    {
        var text =  _ocrProvider.GetTextFromUrl(content.Url); 
        
        var type = _textChecker.Check(text.Result);
        
        return type;
    }
}