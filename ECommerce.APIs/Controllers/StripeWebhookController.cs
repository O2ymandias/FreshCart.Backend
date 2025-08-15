﻿using ECommerce.APIs.ErrorModels;
using ECommerce.Core.Common;
using ECommerce.Core.Common.Enums;
using ECommerce.Core.Common.Options;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.OrderModule;
using ECommerce.Core.Specifications.OrderSpecifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace ECommerce.APIs.Controllers;
[Route("webhook")]
[ApiController]
public class StripeWebhookController : ControllerBase
{
	private readonly StripeOptions _stripe;
	private readonly IOrderService _orderService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IEmailSender _emailSender;
	private readonly IApiErrorResponseFactory _errorFactory;

	public StripeWebhookController(
		IOrderService orderService,
		IUnitOfWork unitOfWork,
		IEmailSender emailSender,
		IOptions<StripeOptions> stripe,
		IApiErrorResponseFactory errorFactory
		)
	{
		_orderService = orderService;
		_unitOfWork = unitOfWork;
		_emailSender = emailSender;
		_errorFactory = errorFactory;
		_stripe = stripe.Value;
	}

	[HttpPost]
	public async Task<IActionResult> Index()
	{
		var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
		var signatureHeader = Request.Headers["Stripe-Signature"];

		try
		{
			var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _stripe.WebhookSecret);

			switch (stripeEvent.Type)
			{
				case EventTypes.CheckoutSessionCompleted:
					await HandleCheckoutSessionCompleted(stripeEvent);
					break;

				case EventTypes.CheckoutSessionExpired:
					await HandleCheckoutSessionExpired(stripeEvent);
					break;
			}

			return Ok();
		}
		catch (StripeException ex)
		{
			var error = _errorFactory.CreateExceptionErrorResponse(ex);
			return BadRequest(error);
		}
		catch (Exception ex)
		{
			var error = _errorFactory.CreateExceptionErrorResponse(ex);
			return BadRequest(error);
		}
	}

	private async Task HandleCheckoutSessionExpired(Event stripeEvent)
	{
		if (
			stripeEvent.Data.Object is Session expiredSession &&
			int.TryParse(expiredSession.ClientReferenceId, out var expiredOrderId)
			)
		{
			var specs = new OrderSpecs(
				specsParams: new() { OrderId = expiredOrderId },
				enablePagination: false,
				enableSorting: false,
				enableTracking: false
				);

			var order = await _unitOfWork
				.Repository<Order>()
				.GetAsync(specs);

			if (order is not null)
				await _orderService.CancelOrderAsync(expiredOrderId, order.UserId);
		}
	}

	private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
	{
		if (
			stripeEvent.Data.Object is Session completedSession &&
			string.Equals(completedSession.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase) &&
			int.TryParse(completedSession.ClientReferenceId, out var completedOrderId))
		{
			var specs = new OrderSpecs(
				specsParams: new() { OrderId = completedOrderId },
				enablePagination: false,
				enableSorting: false,
				enableTracking: true
				);

			specs.IncludeRelatedData(o => o.User);

			var order = await _unitOfWork
				.Repository<Order>()
				.GetAsync(specs, checkLocalCache: false);

			if (order is not null)
			{
				order.PaymentStatus = PaymentStatus.PaymentReceived;
				order.OrderStatus = OrderStatus.Processing;
				var result = await _unitOfWork.SaveChangesAsync();
				if (result > 0)
				{
					await _emailSender.SendEmailAsync(new Email()
					{
						IsHtml = true,
						Subject = $"Order #{order.Id} - Payment Confirmation",
						Body = GenerateSuccessfulPaymentMessage(order),
						To = [order.User.Email]
					});
				}
			}
		}
	}

	private static string GenerateSuccessfulPaymentMessage(Order order) =>
		$@"
		<p>Dear {order.User.DisplayName},</p>
		<p>Thank you for your order!</p>
		<p>We've received your payment and are now processing your order (#{order.Id}).</p>
		<p>We'll notify you once your items are shipped.</p>
		<p>Best regards,<br/>FreshCart Team</p>
		";
}

/* 
	stripe listen --forward-to localhost:5115/webhook --events checkout.session.completed,checkout.session.expired
*/