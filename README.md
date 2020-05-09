# Swish för handel .NET Standard Library

---

Enkelt class library byggt i .NET Standard Library för att hantera API anrop för Swish för Handel.

API dokumentation direkt från Swish själva hittas på https://developer.getswish.se/merchants/ och är från den jag utgått.

## Updatering 2020-05-09

Efter en lång period som koden inte fungerat så har jag fått hjälp av en rad olika utvecklare och till slut fick vi veta vad som var felet och har nu åtgärdat det.

NuGet paketet uppdaterat med senaste koden.

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

### Gör en Payment Request

```C#
// Get the path for the test certificate in the TestCert folder in the console application folder, being always copy to the output folder
string certificatePath = Environment.CurrentDirectory + "\\TestCert\\Swish_Merchant_TestCertificate_1231181189.p12";

// Create a SwishApi Client object with all data needed to run a Swish test payment
SwishApi.Client client = new SwishApi.Client(certificatePath, "swish", "https://tabetaltmedswish.se/Test/Callback/");

// Make the Payement Request
var response = client.MakePaymentRequest("0731596605", 1, "Test");

// Check if the payment request got success and not got any error
if (string.IsNullOrEmpty(response.Error))
{
    // All OK
    string urlForCheckingPaymentStatus = response.Location;

    // If you do a web application you here should wait some time, showing a "loading" view or something and try to do the payment status check as below, you maybe have some ajax request doing a call to a ActionResult doing this code
    // Wait so that the payment request has been processed
    System.Threading.Thread.Sleep(5000);

    // Make the payment status check
    var statusResponse = client.CheckPaymentStatus(urlForCheckingPaymentStatus);

    // Check if the call is done correctly
    if (string.IsNullOrEmpty(statusResponse.errorCode))
    {
        // Call was made without any problem
        Console.WriteLine("Status: " + statusResponse.status);

        // Here is where you do your thing example setting a order status to paid if statusResponse.status is PAID and save the value from statusResponse.paymentReference to be enable to do refunds
    }
    else
    {
        // ERROR
        Console.WriteLine("CheckPaymentResponse: " + statusResponse.errorCode + " - " + statusResponse.errorMessage);
    }
}
else
{
    // ERROR
    Console.WriteLine("MakePaymentRequest - ERROR: " + response.Error);
}
```


### Gör ett återköp

```C#
// The values in to this Refund method you should have saved from the payment, this code snippet using the value from the console code with response object
var refundResponse = client.Refund(statusResponse.paymentReference, statusResponse.amount, "Återköp", "https://tabetaltmedswish.se/Test/RefundCallback/");

if (string.IsNullOrEmpty(refundResponse.Error))
{
    // Request OK
    string urlForCheckingRefundStatus = refundResponse.Location;

    // If you do a web application you here should wait some time, showing a "loading" view or something and try to do the refund status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
    // Wait so that the refund has been processed
    System.Threading.Thread.Sleep(5000);

    // Check refund status
    var refundCheckResposne = client.CheckRefundStatus(urlForCheckingRefundStatus);

    if (string.IsNullOrEmpty(refundCheckResposne.errorCode))
    {
        // Call was made without any problem
        Console.WriteLine("RefundChecKResponse - Status: " + statusResponse.status);

    }
    else
    {
        // ERROR
        Console.WriteLine("RefundCheckResponse: " + refundCheckResposne.errorCode + " - " + refundCheckResposne.errorMessage);
    }
}
else
{
    // ERROR
    Console.WriteLine("Refund Error: " + refundResponse.Error);
}
```

### Callbacks

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

## Bakgrund för projektet

Getswish AB har lanserat Swish för handel men har inte släppt några kodexempel förutom cURL exempel vilket gör det svårt att testa i Windows och att implementera Swish för handel i sitt programmeringsprojekt.

Efter jag la ner väldigt många timmar för att få Swish för handel att fungera i C# så släppte jag ett GitHub repository med ett ASP.NET MVC projekt: https://github.com/RickardPettersson/swish-for-handel-csharp

Men efter att fått många förfrågningar runt koden så har jag nu byggt detta .NET Standard library som jag vill försöka hålla uppdaterad.

## Programmerat av

Jag som gjort detta projekt heter Rickard Nordström Pettersson och ni hittar mina kontaktuppgifter på http://www.rickardp.se
