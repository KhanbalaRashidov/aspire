// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Authentication;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Proto.Collector.Metrics.V1;

namespace Aspire.Dashboard.Otlp.Grpc;

[Authorize(Policy = OtlpAuthorization.PolicyName)]
[SkipStatusCodePages]
public class OtlpGrpcMetricsService(OtlpMetricsService metricsService) : MetricsService.MetricsServiceBase
{
    private readonly OtlpMetricsService _metricsService = metricsService;

    public override Task<ExportMetricsServiceResponse> Export(ExportMetricsServiceRequest request, ServerCallContext context) =>
        Task.FromResult(_metricsService.Export(request));
}
