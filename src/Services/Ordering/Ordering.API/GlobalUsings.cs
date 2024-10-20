﻿global using BuildingBlocks.Exceptions.Handler;
global using BuildingBlocks.Pagination;
global using Carter;
global using HealthChecks.UI.Client;
global using Mapster;
global using MediatR;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Ordering.API;
global using Ordering.Application;
global using Ordering.Application.Dtos;
global using Ordering.Application.Orders.Commands.CreateOrder;
global using Ordering.Application.Orders.Commands.DeleteOrder;
global using Ordering.Application.Orders.Commands.UpdateOrder;
global using Ordering.Application.Orders.Queries.GetOrders;
global using Ordering.Application.Orders.Queries.GetOrdersByCustomer;
global using Ordering.Application.Orders.Queries.GetOrdersByName;
global using Ordering.Infrastructure;
global using Ordering.Infrastructure.Data.Extensions;