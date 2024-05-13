using System.Text;
using System.Text.RegularExpressions;
using Content.Server.Chat.Systems;

namespace Content.Server.TTS;

// ReSharper disable once InconsistentNaming
public sealed partial class TTSSystem
{
    private void OnTransformSpeech(TransformSpeechEvent args)
    {
        if (!_isEnabled) return;
        args.Message = args.Message.Replace("+", "");
    }

    private string Sanitize(string text)
    {
        text = text.Trim();
        text = Regex.Replace(text, @"[^a-zA-Zа-яА-ЯёЁ0-9-Є-ЯҐа-їґ,\-+?!. ]", "");
        text = Regex.Replace(text, @"[a-zA-Z]", ReplaceLat2Cyr, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"(?<![a-zA-Zа-яёА-ЯЁ-Є-ЯҐа-їґ])[a-zA-Zа-яёА-ЯЁ-Є-ЯҐа-їґ]+?(?![a-zA-Zа-яёА-ЯЁ-Є-ЯҐа-їґ])", ReplaceMatchedWord, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"(?<=[1-90])(\.|,)(?=[1-90])", " цілих ");
        text = Regex.Replace(text, @"\d+", ReplaceWord2Num);
        text = text.Trim();
        return text;
    }

    private string ReplaceLat2Cyr(Match oneChar)
    {
        if (ReverseTranslit.TryGetValue(oneChar.Value.ToLower(), out var replace))
            return replace;
        return oneChar.Value;
    }

    private string ReplaceMatchedWord(Match word)
    {
        if (WordReplacement.TryGetValue(word.Value.ToLower(), out var replace))
            return replace;
        return word.Value;
    }

    private string ReplaceWord2Num(Match word)
    {
        if (!long.TryParse(word.Value, out var number))
            return word.Value;
        return NumberConverter.NumberToText(number);
    }

    private static readonly IReadOnlyDictionary<string, string> WordReplacement =
        new Dictionary<string, string>()
        {
            {"нт", "Ен Те"},
            {"гл", "Ге Ел"},
            {"гп", "Ге Пе"},
            {"днв", "Де Ен Ве"},
            {"гб", "Ге Бе"},
            {"км", "Ке Ем"},
            {"сі", "Ес І"},
            {"цк", "Це Каа"},
            {"пцк", "Пе Це Каа"},
            {"сб", "Ес Бе"},
            {"кпк", "Ке Пе Каа"},
            {"пда", "Пе Де А"},
            {"id", "Ай Ди"},
            {"вт", "Ва Те"},
            {"wt", "Ва Те"},
            {"ерп", "Ее Ер Пе"},
            {"апц", "А Пе Це"},
            {"тк", "Те Ка"},
            {"бщ", "Бе Ще"},
            {"кк", "Ка Ка"},
            {"ск", "Ес Ка"},
            {"зк", "Зе Ка"},
            {"ерт", "ЕЕ Ер Те"},
            {"ссд", "Ес Ес Де"},
            {"жпс", "Джи Пі Ес"},
            {"gps", "Джи Пі Ес"},
            {"днк", "Де Ен Ка"},
            {"рпг", "Ер Пе Ге"},
            {"с4", "Сі 4"}, // cyrillic
            {"c4", "Сі 4"}, // latinic
        };

    private static readonly IReadOnlyDictionary<string, string> ReverseTranslit =
        new Dictionary<string, string>()
        {
            {"a", "а"},
            {"b", "б"},
            {"v", "в"},
            {"g", "г"},
            {"d", "д"},
            {"e", "е"},
            {"je", "є"},
            {"zh", "ж"},
            {"z", "з"},
            {"i", "і"},
            {"y", "й"},
            {"k", "к"},
            {"l", "л"},
            {"m", "м"},
            {"n", "н"},
            {"o", "о"},
            {"p", "п"},
            {"r", "р"},
            {"s", "с"},
            {"t", "т"},
            {"u", "у"},
            {"f", "ф"},
            {"h", "х"},
            {"c", "ц"},
            {"x", "кс"},
            {"ch", "ч"},
            {"sh", "ш"},
            {"jsh", "щ"},
            {"hh", "хʼ"},
            {"ih", "и"},
            {"jh", "и"},
            {"eh", "еʼ"},
            {"ju", "ю"},
            {"ja", "я"},
        };
}

// Source: https://codelab.ru/s/csharp/digits2phrase
public static class NumberConverter
{
    private static readonly string[] Frac20Male =
    {
        "", "один", "два", "три", "чотири", "пʼять", "шість",
        "сім", "вісім", "девʼять", "десять", "одиннадцять",
        "дванадцять", "тринадцять", "чотирнадцять", "пʼятнадцять",
        "шістнадцять", "сімнадцять", "вісемнадцять", "девʼятнадцять"
    };

    private static readonly string[] Frac20Female =
    {
        "", "одна", "дві", "три", "чотири", "пʼять", "шість",
        "сімь", "вісім", "девʼять", "десять", "одиннадцять",
        "дванадцять", "тринадцять", "чотирнадцять", "пʼятнадцять",
        "шістнадцять", "сімнадцять", "вісемнадцять", "девʼятнадцять"
    };

	private static readonly string[] Hunds =
	{
		"", "сто", "двісті", "триста", "чотириста",
		"пʼятсот", "шістсот", "сімсот", "вісімсот", "девʼятьсот"
	};

	private static readonly string[] Tens =
	{
		"", "десять", "двадцять", "тридцять", "сорок", "пʼятдесят",
		"шістдесять", "сімдесять", "вісімдесять", "девʼяносто"
	};

	public static string NumberToText(long value, bool male = true)
    {
        if (value >= (long)Math.Pow(10, 15))
            return String.Empty;

        if (value == 0)
            return "нуль";

		var str = new StringBuilder();

		if (value < 0)
		{
			str.Append("мінус");
			value = -value;
		}

        value = AppendPeriod(value, 1000000000000, str, "трильйон", "трильйона", "трильйонів", true);
        value = AppendPeriod(value, 1000000000, str, "мільярд", "мільярда", "мільярдів", true);
        value = AppendPeriod(value, 1000000, str, "мільйон", "мільйона", "мільйонів", true);
        value = AppendPeriod(value, 1000, str, "тисяча", "тисячі", "тисяч", false);

		var hundreds = (int)(value / 100);
		if (hundreds != 0)
			AppendWithSpace(str, Hunds[hundreds]);

		var less100 = (int)(value % 100);
        var frac20 = male ? Frac20Male : Frac20Female;
		if (less100 < 20)
			AppendWithSpace(str, frac20[less100]);
		else
		{
			var tens = less100 / 10;
			AppendWithSpace(str, Tens[tens]);
			var less10 = less100 % 10;
			if (less10 != 0)
				str.Append(" " + frac20[less100%10]);
		}

		return str.ToString();
	}

	private static void AppendWithSpace(StringBuilder stringBuilder, string str)
	{
		if (stringBuilder.Length > 0)
			stringBuilder.Append(" ");
		stringBuilder.Append(str);
	}

	private static long AppendPeriod(
        long value,
        long power,
		StringBuilder str,
		string declension1,
		string declension2,
		string declension5,
		bool male)
	{
		var thousands = (int)(value / power);
		if (thousands > 0)
		{
			AppendWithSpace(str, NumberToText(thousands, male, declension1, declension2, declension5));
			return value % power;
		}
		return value;
	}

	private static string NumberToText(
        long value,
        bool male,
		string valueDeclensionFor1,
		string valueDeclensionFor2,
		string valueDeclensionFor5)
	{
		return
            NumberToText(value, male)
			+ " "
			+ GetDeclension((int)(value % 10), valueDeclensionFor1, valueDeclensionFor2, valueDeclensionFor5);
	}

	private static string GetDeclension(int val, string one, string two, string five)
	{
		var t = (val % 100 > 20) ? val % 10 : val % 20;

		switch (t)
		{
			case 1:
				return one;
			case 2:
			case 3:
			case 4:
				return two;
			default:
				return five;
		}
	}
}
