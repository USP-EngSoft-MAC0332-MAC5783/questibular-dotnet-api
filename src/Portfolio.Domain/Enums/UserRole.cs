namespace Portfolio.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
public enum UserRole
{
    Student,
    Teacher
}