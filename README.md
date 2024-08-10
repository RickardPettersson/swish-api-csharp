# Swish för handel .NET 7 class library

---

Enkelt class library byggt i .NET 7 för att hantera API anrop för Swish för Handel.

API dokumentation direkt från Swish själva hittas på https://developer.swish.nu/ och är från den jag utgått.

## Updatering 2024-08-10
Uppdaterat projektet att köra .Net 7

Certifikat hanteringen är nu utflyttad till en eget class fil och används nu av samtliga clienter istället för samma kod i flera clienter.

Inlagt i certifikat hanteringen är även kod fixar som inkommit från github communityt.

Jag har lagt in ett nytt certifikat som heter "Swish_Merchant_TestCertificate2_1234679304.p12" i test console appen som jag fått från GetSwish AB då den nya emulator paketet sedan 2023 innehåller ett certfikat som .Net säger inte har samma lösenord som dem utger sig att ha i dokumentationen, supporten gav mig detta certifikat som fungerar för emuleringen så bifogar den numera i test projektet.

Har även tagit bort gamla Client.cs som sedan tidigare haft Obsolete flagga på sig i koden så ingen bör använda den längre.

## Testa

I detta git repository ingår en test console application för testning, som är uppsatt att köras direkt mot Swish testmiljö med testcertifikat.

Console appen gör en Payment Request, en kontroll av statusen på Payment Request och sedan återköper betalningen och kollar statusen på återköpet.

## Installera
Antingen installerar du class library:et från detta repository eller så installerar du det genom NuGet: https://www.nuget.org/packages/SwishApi

```powershell
PM> Install-Package SwishApi -Version 2.0.7
```

## Kom igång enkelt

Förutom att du har test console appen så kommer här lite kodsnuttar på från console appen.

### Kodexempel

I repositoryt finns en Console Application som visar kod exempel hur man använder libraryt förutom Callbacks, se nedan.

### Callbacks kodexempel

Här finns kod exempel för hur Swish Payment Callback kan se ut i ett .Net 6 Web API projekt:

```C#
[HttpPost("/p/Swish/Callback")]
public string SwishCallback([FromBody] JsonElement jsonElement)
{
	string json = jsonElement.ToString();

	PaymentCallback callback = Newtonsoft.Json.JsonConvert.DeserializeObject<PaymentCallback>(json);

	// Check if the call is done correct
	if (string.IsNullOrEmpty(callback.errorCode))
	{
		switch (callback.status)
		{
			case "CREATED":
				// Maybe never happening but the payment created
				break;
			case "PAID":
				// Payment is done
				break;
			case "DECLINED":
				// The user cancelled the payment
				break;
			case "ERROR":
				// Something got wrong, if it takes 3 minutes its timeouts to ERROR
				break;
		}
	}
	else
	{
		// ERROR
	}

	if (!string.IsNullOrEmpty(callback.payeePaymentReference))
	{
		string myReference = callback.payeePaymentReference;

	}

	return "OK";
}
```

## Certifikat hantering

Du hitta information om hur du skapar och hanterar ditt egna certifikat för produktion på wiki sidan [Hantera certifikat](https://github.com/RickardPettersson/swish-api-csharp/wiki/Hantera-certifikat)

## Bakgrund för projektet

Getswish AB har lanserat Swish för handel men har inte släppt några kodexempel förutom cURL exempel vilket gör det svårt att testa i Windows och att implementera Swish för handel i sitt programmeringsprojekt.

Efter jag la ner väldigt många timmar för att få Swish för handel att fungera i C# så har jag släppt lite olika kod exempel och till slut släppte jag detta class library för att hjälpa andra komma igång.

## Programmerat av

Jag som gjort detta projekt heter Rickard Nordström Pettersson och ni hittar mina kontaktuppgifter på http://www.rickardp.se

Jag vill även tacka de personer som inkommit med pull requests och idéer på förbättringar, dem syns under Contributors på höger spalten.
