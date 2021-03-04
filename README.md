# Swish för handel .NET Standard Library

---

Enkelt class library byggt i .NET Standard Library för att hantera API anrop för Swish för Handel.

API dokumentation direkt från Swish själva hittas på https://developer.getswish.se/merchants/ och är från den jag utgått.

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

Här finns kod för ett ASP.NET MVC 5 projekt och Callback kod för både payment och refund, tagna från GitHub repositoryt: https://github.com/RickardPettersson/swish-for-handel-csharp

```C#
public string Callback()
{
    Stream req = Request.InputStream;
    req.Seek(0, System.IO.SeekOrigin.Begin);
    string json = new StreamReader(req).ReadToEnd();

    SwishCheckPaymentRequestStatusResponse resultObject = JsonConvert.DeserializeObject<SwishCheckPaymentRequestStatusResponse>(json);

    switch (resultObject.status)
    {
        case "CREATED":
            // Borde kanske aldrig få CREATED här...
            break;
        case "PAID":
            // Betalningen är klar
            break;
        case "DECLINED":
            // Användaren avbröt betalningen
            break;
        case "ERROR":
            // Något gick fel, om betalningen inte sker inom 3 minuter skickas ERROR
            break;
    }

    // When someone like to use this live I should log this and maybe change the status of some order or somethign to be paid or what the status says.
    // To make a refund you need to save the value of paymentReference
    // var paymentReference = resultObject.paymentReference;

    return "OK";
}

public string RefundCallback()
{
    // Exempel Callback JSON sträng
    Stream req = Request.InputStream;
    req.Seek(0, System.IO.SeekOrigin.Begin);
    string json = new StreamReader(req).ReadToEnd();

    SwishRefundSatusCheckResponse resultObject = JsonConvert.DeserializeObject<SwishRefundSatusCheckResponse>(json);

    switch (resultObject.status)
    {
        case "DEBITED":
            // Återköpt
            break;
        case "PAID":
            // Betald
            break;
        case "ERROR":
            // Något gick fel
            break;
    }

    // When someone like to use this live I should log this and maybe change the status of some order or something to be repaid or what the status says.
    // Use payerPaymentReference to get the order
    // var paymentref = resultObject.payerPaymentReference;

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

Jag vill även tacka Pierre Schönbeck (ikinz på github) för sitt jobb att implementera Payout i detta library.
