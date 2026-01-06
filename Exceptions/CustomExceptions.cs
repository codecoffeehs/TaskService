using System;

namespace TaskService.Exceptions;

public class BadRequestException(string message) : Exception(message);
public class UnauthorizedException(string message) : Exception(message);
public class ForbiddenException(string message) : Exception(message);
public class NotFoundException(string message) : Exception(message);
public class ConflictException(string message) : Exception(message);
public class RequestTimeoutException(string message) : Exception(message);