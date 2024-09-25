using System.Text.RegularExpressions;
using TextToNumber.Models.Requests;
using TextToNumber.Models.Responses;

namespace TextToNumber.Services;

public class ConvertService : IConvertService
{
    private static readonly Dictionary<string, int> numberWords = new()
    {
        { "bir", 1 }, { "iki", 2 }, { "üç", 3 }, { "dört", 4 }, { "beş", 5 },
        { "altı", 6 }, { "yedi", 7 }, { "sekiz", 8 }, { "dokuz", 9 },
        { "on", 10 }, { "yirmi", 20 }, { "otuz", 30 }, { "kırk", 40 },
        { "elli", 50 }, { "altmış", 60 }, { "yetmiş", 70 }, { "seksen", 80 }, { "doksan", 90 },
        { "yüz", 100 }, { "bin", 1000 }
    };

    public ConvertTextToNumberResponse ConvertTextToNumber(ConvertTextToNumberRequest request)
    {
        // Kelimeleri tanımlamak için düzenli ifade kullanıyoruz
        string[] words = SplitNumberWords(request.UserText, removeSpaces:true);
        string[] wordsWithWhiteSpaces = SplitNumberWords(request.UserText, removeSpaces:false);
        List<object> numberParts = new List<object>();
        int currentNumber = 0;
        bool isFirst = false;

        List<object> testResult = new();
        foreach (string word in wordsWithWhiteSpaces)
        {
            if (word != "" && !numberWords.ContainsKey(word))
            {
                testResult.Add(word);
            }
        }

        // Her kelimeyi tek tek kontrol ediyoruz
        for (int i = 0; i < words.Length; i++)
        {
            // Eğer kelime sayı ise sayıya çeviriyoruz
            if (numberWords.ContainsKey(words[i].ToLower()))
            {
                int value = numberWords[words[i].ToLower()];

                if (value == 100 || value == 1000)
                {
                    if (!isFirst)
                    {
                        isFirst = true;
                        if (currentNumber == 0)
                            currentNumber = 1;
                        currentNumber *= value;
                    }
                    else
                    {
                        int previousValue = numberWords[words[i - 1].ToLower()];
                        currentNumber += value * previousValue - previousValue;
                    }
                }
                else
                {
                    currentNumber += value;
                }
            }
            else
            {
                // Sayı olmayan kelimeyi ekliyoruz ve önceki sayıyı da ekliyoruz
                if (currentNumber > 0)
                {
                    numberParts.Add(currentNumber);
                    currentNumber = 0;
                }

                numberParts.Add(words[i]);
            }
        }

        // Son sayıyı da ekliyoruz
        if (currentNumber > 0)
        {
            numberParts.Add(currentNumber);
        }

        foreach (var item in numberParts)
        {
            if (item is int)
            {
                testResult.Insert(numberParts.IndexOf(item), item);
            }
        }

        // Listeyi string'e dönüştürüyoruz
        var result = string.Join(" ", testResult.ConvertAll(element => element.ToString()));

        return new ConvertTextToNumberResponse { Output = result };
    }

    // Bitişik yazılmış sayı kelimelerini ayrıştıran fonksiyon
    private string[] SplitNumberWords(string input, bool removeSpaces)
    {
        if (removeSpaces)
            input = input.Replace(" ", "");

        // Türkçe'de sayıları tanımlayan düzenli ifadeleri kullanarak bitişik yazılmışları buluyoruz
        string pattern =
            @"bir|iki|üç|dört|beş|altı|yedi|sekiz|dokuz|on|yirmi|otuz|kırk|elli|altmış|yetmiş|seksen|doksan|yüz|bin";
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
        List<string> splitWords = new List<string>();
        int lastIndex = 0;

        // Bitişik yazılmış sayıları bulup ayırıyoruz
        foreach (Match match in regex.Matches(input.ToLower()))
        {
            if (match.Index > lastIndex)
            {
                // Kelime olmayan kısmı ayırıyoruz
                splitWords.Add(input.Substring(lastIndex, match.Index - lastIndex).Trim());
            }

            // Sayı kelimesini ekliyoruz
            splitWords.Add(match.Value);
            lastIndex = match.Index + match.Length;
        }

        // Kalan kısmı ekliyoruz
        if (lastIndex < input.Length)
        {
            splitWords.Add(input.Substring(lastIndex).Trim());
        }

        return splitWords.ToArray();
    }
}