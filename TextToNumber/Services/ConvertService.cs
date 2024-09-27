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
        List<object> finalTrimmedWords = new List<object>();
        int currentNumber = 0;
        bool isThousandUsed = false;
        bool isHundredUsed = false;

        // Her kelimeyi tek tek kontrol ediyoruz
        for (int i = 0; i < words.Length; i++)
        {
            // Eğer kelime sayı ise sayıya çeviriyoruz
            if (numberWords.ContainsKey(words[i].ToLower()))
            {
                int value = numberWords[words[i].ToLower()];

                if (value % 100 == 0)
                {
                    if (!isHundredUsed)
                        isHundredUsed = value == 100;
                    if (!isThousandUsed)
                        isThousandUsed = value == 1000;
                    /*
                        Bir sayıda hem 'bin' hem de 'yüz' ifadeleri geçiyorsa örn. iki bin sekiz yüz (2800),
                        currentNumber'ı hem 1000 hem 100 ile çarptığımızda hatalı bir hesaplama yapmış oluyoruz,
                        bu hesaplama hatasını isFirst isimli bir flag kullanarak düzelttim,
                        fakat 'bin' ifadesinden önce 'yüz' ifadesi kullanılırsa
                        yine hatalı bir sonuca sebep oluyor, bu logic değişecek...
                        Not: isFirst isimli flag'i kaldırıp, isHundredUsed ve isThousandUsed isimli flag'leri ekleyip hatalı hesaplamanın önüne geçildi
                    */
                    if (isThousandUsed && !isHundredUsed)
                    {
                        if (currentNumber == 0)
                            currentNumber = 1;
                        currentNumber *= value;
                    }
                    else
                    {
                        int previousValue = numberWords[words[i - 1].ToLower()];
                        currentNumber += value * previousValue - previousValue; 
                        /*
                            Bu logic ile, örn. iki bin sekiz yüz (2800) ifadesi hesaplanırken
                            öncelikle 2008 değeri elde edildiği için, sonradan gelen 'bin', 'yüz' gibi ifadelerin
                            hatalı sonuca sebep olmasının önüne geçiyorum
                        */ 
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
                    finalTrimmedWords.Add(currentNumber);
                    currentNumber = 0;
                }
                finalTrimmedWords.Add(words[i]);
            }
        }

        // Son sayıyı da ekliyoruz
        if (currentNumber > 0)
            finalTrimmedWords.Add(currentNumber);

        List<object> resultingWords = GetResultingWords(userText:request.UserText, trimmedWords: finalTrimmedWords);
        string resultString = string.Join(" ", resultingWords.ConvertAll(element => element.ToString()));

        return new ConvertTextToNumberResponse { Output = resultString };
    }

    private List<object> GetNonNumberWords(string[] words)
    {
        List<object> nonNumberWords = new();
        foreach (string word in words)
        {
            if (!numberWords.ContainsKey(word) && !string.IsNullOrEmpty(word))
                nonNumberWords.Add(word);
        }

        return nonNumberWords;
    }

    private List<object> GetResultingWords(string userText, List<object> trimmedWords)
    {
        List<object> resultingWords = GetNonNumberWords(SplitNumberWords(userText, removeSpaces:false));
        foreach (var item in trimmedWords)
        {
            if (item is int)
                resultingWords.Insert(trimmedWords.IndexOf(item), item);
        }

        return resultingWords;
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