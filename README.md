# Swish för handel .NET Standard Library

---

Enkelt class library byggt i .NET 6 för att hantera API anrop för Swish för Handel.

API dokumentation direkt från Swish själva hittas på https://developer.getswish.se/merchants/ och är från den jag utgått.

## Updatering 2022-09-22
Uppdaterat projektet att köra .Net 6.

Jag har byggt nya C# klasser för varje typ av Swish function för att göra koden renare och uppdaterat med ett par önskemål som funnits.

Här är namnen på klient klasserna: ECommerceClient, MCommerceClient, RefundClient och RefundClient och om ni är insatta i Swish För Handel så tror jag ni känner igen namnen och förstår vad varje är till för.

Det har byggts in funktion att kunna skicka in en Stream av klient certifikatet istället för en sökväg till filen. 

Jag har även lagt in så vid varje anrop läggs en host header in för att undvika ett problem med SendAsync kommandot för HttpClient.

SwishApiConsoleTest projektet är uppdaterat och kör med dem nya klient class koden.

Den gamla Client.cs koden är fortfarande kvar så det är just nu bakåt kompatibelt men jag har satt ett Obsolete attribute på C# classen och jag vill att ni går över till dem nya klient classerna, den gamla kommer tas bort framöver.

Sista grejen jag gjort är att jag inkluderat en model som heter PaymentCallback och tror namnet säger sig själv och då uppdaterat Callback kod exemplet nedan att använda denna class.

## Updatering 2021-07-08

Godkände och mergeat in ändringar från en Pull Request att köra detta med .Net 5 + support för att inte ha certifikatet lokalt, dessa ändringar är gjorda av Per Samuellsson (https://github.com/per-samuelsson), stort tack!

## Updatering 2021-03-04

Godkände och mergeat in ändringar från en Pull Request för att köra Siwsh Payouts, ändringarna är gjorda av Pierre Schönbeck (https://github.com/ikinz), stort tack!

## Updatering 2020-05-20

Jag har uppdateat GetSwish ABs test certifikat så koden fungerar i github repositoryt igen i test miljö.

## Updatering 2020-05-09

Efter en lång period som koden inte fungerat så har jag fått hjälp av en rad olika utvecklare och till slut fick vi veta vad som var felet och har nu åtgärdat det.

NuGet paketet uppdaterat med senaste koden.

## Updatering 2021-03-04

Pierre Schönbeck (ikinz på github) har skickat in en pull request på implementation av Payout apierna, stort tack för det Pierre!

## Testa

I detta git repository ingår en test console application för testning, som är uppsatt att köras direkt mot Swish testmiljö med testcertifikat.

Console appen gör en Payment Request, en kontroll av statusen på Payment Request och sedan återköper betalningen och kollar statusen på återköpet.

## Installera
Antingen installerar du class library:et från detta repository eller så installerar du det genom NuGet: https://www.nuget.org/packages/SwishApi

```powershell
PM> Install-Package SwishApi -Version 1.2.0
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
