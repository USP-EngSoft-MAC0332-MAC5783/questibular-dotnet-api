namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// GetExamsRequest
// Query params for GET /api/v1/exams?start=YYYY&end=YYYY.
// Both parameters are required and the start value can never be greater than the end value.
// =============================================================================
public record GetExamsRequest(int Start, int End);