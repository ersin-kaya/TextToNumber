using TextToNumber.Models.Requests;
using TextToNumber.Models.Responses;

namespace TextToNumber.Services;

public interface IConvertService
{
    ConvertTextToNumberResponse ConvertTextToNumber(ConvertTextToNumberRequest request);
}