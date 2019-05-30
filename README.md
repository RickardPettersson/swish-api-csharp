# Swish för handel .Net Standard Library

---

Enkelt class library byggt i .Net Standard Library för att hantera API anrop för Swish för Handel.

API dokumentation direkt från Swish själva hittas på https://developer.getswish.se/merchants/ och är från den jag utgått.

## Testa

I detta git repository ingår en test console application för testning, som är uppsatt att köras direkt mot Swish test miljö med test certifikat.

Console appen gör en Payment Request, en kontroll av statusen på Payment Request och sedan återköper betalningen och kollar statusen på återköpet.

## Kom igång enkelt

Förutom att du har test console appen så kommer här lite kodsnuttar på från console appen.

### Gör en Payment Request

```C#
// Get the path for the test certificate in the TestCert folder in the console application folder, being always copy to the output folder
string certificatePath = Environment.CurrentDirectory + "\\TestCert\\Swish_Merchant_TestCertificate_1231181189.p12";

// Create a Swishpi Client object with all data needed to run a test Swish payment
SwishApi.Client client = new SwishApi.Client(certificatePath, "swish", "https://tabetaltmedswish.se/Test/Callback/");

// Make the Payement Request
var response = client.MakePaymentRequest("0731596605", 1, "Test");

// Check if the payment request got success and not got any error
if (string.IsNullOrEmpty(response.Error))
{
	// All OK
	string urlForCheckingPaymentStatus = response.Location;

	// If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the payment status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
	// Wait so that the payment request has been processed
	System.Threading.Thread.Sleep(5000);

	// Make the payment status check
	var statusResponse = client.CheckPaymentStatus(urlForCheckingPaymentStatus);

	// Check if the call is done correct
	if (string.IsNullOrEmpty(statusResponse.errorCode))
	{
		// Call was maked without any problem
		Console.WriteLine("Status: " + statusResponse.status);

		// Here is where you doing your thing example setting a order status to paid if statusResponse.status is PAID and save the value from statusResponse.paymentReference to be enable to do refunds
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
var refundResponse = client.Refund(statusResponse.paymentReference, statusResponse.amount, "Återköp");

if (string.IsNullOrEmpty(refundResponse.Error))
{
	// Request OK
	string urlForCheckingRefundStatus = refundResponse.Location;

	// If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the refund status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
	// Wait so that the refund has been processed
	System.Threading.Thread.Sleep(5000);

	// Check refund status
	var refundCheckResposne = client.CheckRefundStatus(urlForCheckingRefundStatus);

	if (string.IsNullOrEmpty(refundCheckResposne.errorCode))
	{
		// Call was maked without any problem
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

## Bakgrund för projektet

Getswish AB har lanserat Swish för handel men har inte släppt några kodexempel förutom cURL exempel vilket gör det svårt att testa i Windows och att implementera Swish för handel i sitt programmerings projekt.

Efter jag la ner väldigt många timmar för att få Swish för handel att fungera i C# så släppte jag ett Github repository med ett ASP.NET MVC projekt: https://github.com/RickardPettersson/swish-for-handel-csharp

Men efter att fått många förfrågningar runt koden så har jag nu byggt detta .Net Standard library som jag vill försöka hålla uppdaterad.

## Programmerat av

Jag som gjort detta projekt heter Rickard Nordström Pettersson och ni hittar mina kontaktuppgifter på http://www.rickardp.se