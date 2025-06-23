using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

public class StripeController : Controller
{
    public IActionResult CreateCheckoutSession()
    {
        var amountString = TempData["TotalAmount"]?.ToString();

        int amount = 1000; // default
        if (decimal.TryParse(amountString, out decimal parsedDecimal))
        {
            amount = (int)(parsedDecimal * 100); // convert to cents
        }

        var domain = "https://localhost:5001";
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = amount,
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Product Name",
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = domain + "/Stripe/Success",
            CancelUrl = domain + "/Stripe/Cancel",
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return Redirect(session.Url);
    }

    public IActionResult Success()
    {
        return View();
    }

    public IActionResult Cancel()
    {
        return View();
    }
}
